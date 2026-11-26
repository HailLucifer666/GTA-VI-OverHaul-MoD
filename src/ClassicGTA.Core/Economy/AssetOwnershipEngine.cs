using System;
using System.Collections.Generic;
using ClassicGTA.Contracts;

namespace ClassicGTA.Core.Economy
{
    public enum PassiveIncomeStatus
    {
        Paid = 0,
        NothingDue = 1,
        NoBusinessesOwned = 2,
        BlockedInOnline = 3,
        ClockMovedBackward = 4,
        PersistenceFailed = 5
    }

    public sealed class PassiveIncomeResult
    {
        public PassiveIncomeStatus Status { get; private set; }
        public int AmountPaid { get; private set; }
        public int ElapsedDays { get; private set; }
        public string Message { get; private set; }

        public PassiveIncomeResult(PassiveIncomeStatus status, int amountPaid, int elapsedDays, string message)
        {
            Status = status;
            AmountPaid = amountPaid;
            ElapsedDays = elapsedDays;
            Message = message ?? string.Empty;
        }
    }

    /// <summary>
    /// Manages Safehouses (#1), Passive-Income Businesses (#2), Boats/Planes/Helicopters (#3),
    /// and Submarines (#4) using stable ContentId keys, per-slot atomic files (C-01),
    /// strict PersistenceCodec validation with safe legacy v0.6.0 migration (I-02, I-04),
    /// idempotent refund-on-failure purchases, and pure UTC calendar passive income (I-01).
    /// </summary>
    public sealed class AssetOwnershipEngine
    {
        public const string ProductionDefaultSlot = "gtav_story_default";
        private const string LegacySaveFileName = "ClassicGTA_Assets.dat";
        private const string SchemaHeader = "SCHEMA=1";

        private readonly IGameContext _context;
        private readonly IEconomyService _economy;
        private readonly IPersistenceStore _store;
        private readonly INotificationService _notifications;
        private readonly int _offlineCapDays;
        private readonly string _slotFingerprint;
        private readonly string _saveFileName;

        private readonly Dictionary<string, AssetDefinition> _catalog =
            new Dictionary<string, AssetDefinition>(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> _ownedIds =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private bool _hasLastPayoutDay;
        private int _lastPayoutUtcDay;
        private bool _loadFailedCorrupt;

        public int OwnedCount { get { return _ownedIds.Count; } }
        public string SlotFingerprint { get { return _slotFingerprint; } }
        public string SaveFileName { get { return _saveFileName; } }
        public bool LoadFailedCorrupt { get { return _loadFailedCorrupt; } }

        public AssetOwnershipEngine(
            IGameContext context,
            IEconomyService economy,
            IPersistenceStore store,
            INotificationService notifications,
            int offlineCapDays = 7,
            string slotFingerprint = "default")
        {
            if (context == null) throw new ArgumentNullException("context");
            if (economy == null) throw new ArgumentNullException("economy");
            if (store == null) throw new ArgumentNullException("store");
            if (notifications == null) throw new ArgumentNullException("notifications");
            if (offlineCapDays < 1 || offlineCapDays > 30) throw new ArgumentOutOfRangeException("offlineCapDays");

            _context = context;
            _economy = economy;
            _store = store;
            _notifications = notifications;
            _offlineCapDays = offlineCapDays;
            _slotFingerprint = string.IsNullOrWhiteSpace(slotFingerprint) ? "default" : slotFingerprint.Trim();
            _saveFileName = "ClassicGTA_Assets_" + PersistenceCodec.SanitizeSlotId(_slotFingerprint) + ".dat";
        }

        public void RegisterAsset(AssetDefinition asset)
        {
            if (asset == null) throw new ArgumentNullException("asset");
            if (_catalog.ContainsKey(asset.ContentId))
            {
                throw new InvalidOperationException("Duplicate Asset ContentId: " + asset.ContentId);
            }
            _catalog.Add(asset.ContentId, asset);
        }

        public bool IsOwned(string contentId)
        {
            return !string.IsNullOrWhiteSpace(contentId) && _ownedIds.Contains(contentId);
        }

        public bool LoadState()
        {
            string raw;
            if (_store.TryLoad(_saveFileName, ValidateStrictPayload, out raw))
            {
                return ApplyParsedStrictRecords(raw);
            }

            // If a per-slot file exists on disk but failed strict validation across primary + backups,
            // refuse to silently clobber it on future saves (I-02 / I-04).
            if (_store.Exists(_saveFileName))
            {
                _loadFailedCorrupt = true;
                _notifications.Log("[Assets] Failed to load existing ledger " + _saveFileName + "; refusing to overwrite.");
                return false;
            }

            // I-02 Fix: Explicit legacy migration path for footer-less, slot-less v0.6.0 ClassicGTA_Assets.dat
            // when running under production default slot ("gtav_story_default") or legacy "default".
            if (IsLegacyMigrationEligibleSlot(_slotFingerprint) &&
                _store.TryLoad(LegacySaveFileName, ValidateLegacyPayload, out raw))
            {
                if (ApplyLegacyMigration(raw))
                {
                    return true;
                }
            }

            return false;
        }

        public PurchaseResult TryPurchase(string contentId, int? currentUtcDay = null)
        {
            if (_context.IsOnlineSession)
            {
                return new PurchaseResult(PurchaseStatus.BlockedInOnline, "Blocked in Online session.");
            }

            if (_loadFailedCorrupt)
            {
                return new PurchaseResult(PurchaseStatus.PersistenceFailedAndRefunded, "Ledger failed to load; refusing to overwrite existing save.");
            }

            AssetDefinition asset;
            if (string.IsNullOrWhiteSpace(contentId) || !_catalog.TryGetValue(contentId, out asset))
            {
                return new PurchaseResult(PurchaseStatus.UnknownAsset, "Unknown asset: " + contentId);
            }

            if (_ownedIds.Contains(asset.ContentId))
            {
                return new PurchaseResult(PurchaseStatus.AlreadyOwned, asset.DisplayName + " is already owned.");
            }

            if (_economy.GetCash() < asset.Price || !_economy.TrySpend(asset.Price, "Buy:" + asset.ContentId))
            {
                return new PurchaseResult(PurchaseStatus.InsufficientFunds, "Not enough cash for " + asset.DisplayName + ".");
            }

            bool prevHasDay = _hasLastPayoutDay;
            int prevDay = _lastPayoutUtcDay;

            _ownedIds.Add(asset.ContentId);

            // I-01 Fix: Stamp passive-income baseline at first business purchase so time between purchase
            // and first collection is never forfeited.
            if (asset.Category == AssetCategory.Business && !_hasLastPayoutDay && currentUtcDay.HasValue && currentUtcDay.Value >= 0)
            {
                _hasLastPayoutDay = true;
                _lastPayoutUtcDay = currentUtcDay.Value;
            }

            try
            {
                PersistState();
            }
            catch (Exception ex)
            {
                _ownedIds.Remove(asset.ContentId);
                _hasLastPayoutDay = prevHasDay;
                _lastPayoutUtcDay = prevDay;
                _economy.Credit(asset.Price, "Refund:" + asset.ContentId);
                _notifications.Log("[Assets] Persist failed for " + asset.ContentId + ", refunded: " + ex.Message);
                return new PurchaseResult(PurchaseStatus.PersistenceFailedAndRefunded, "Save failed; cash refunded.");
            }

            _notifications.Show("Purchased " + asset.DisplayName + " for $" + asset.Price + ".");
            _notifications.Log("[Assets] PURCHASED id=" + asset.ContentId + " price=" + asset.Price);
            return new PurchaseResult(PurchaseStatus.Succeeded, "Purchased " + asset.DisplayName + ".");
        }

        /// <summary>
        /// Convenience wrapper returning dollars paid for existing callers.
        /// Host UI should call CollectPassiveIncomeDetailed so persistence errors are never misreported as zero income (I-06).
        /// </summary>
        public int CollectPassiveIncome(int currentUtcDay)
        {
            return CollectPassiveIncomeDetailed(currentUtcDay).AmountPaid;
        }

        /// <summary>
        /// Collects passive business income on a single absolute UTC calendar-day timeline (I-01).
        /// Never adds session time, never invents a day on backward clock jumps, and reports
        /// persistence failures distinctly from ordinary zero payouts (I-06).
        /// </summary>
        public PassiveIncomeResult CollectPassiveIncomeDetailed(int currentUtcDay)
        {
            if (_context.IsOnlineSession)
            {
                return new PassiveIncomeResult(PassiveIncomeStatus.BlockedInOnline, 0, 0, "Blocked in Online session.");
            }

            if (currentUtcDay < 0)
            {
                return new PassiveIncomeResult(PassiveIncomeStatus.ClockMovedBackward, 0, 0, "System clock invalid; no business income paid.");
            }

            if (_loadFailedCorrupt)
            {
                return new PassiveIncomeResult(PassiveIncomeStatus.PersistenceFailed, 0, 0, "Business ledger failed to load; check ClassicGTA.log.");
            }

            long dailyTotal = ComputeDailyBusinessIncome();
            if (dailyTotal <= 0)
            {
                return new PassiveIncomeResult(PassiveIncomeStatus.NoBusinessesOwned, 0, 0, "No income-generating businesses owned yet.");
            }

            if (!_hasLastPayoutDay)
            {
                _hasLastPayoutDay = true;
                _lastPayoutUtcDay = currentUtcDay;
                try
                {
                    PersistState();
                }
                catch (Exception ex)
                {
                    _hasLastPayoutDay = false;
                    _notifications.Log("[Assets] Baseline persist failed: " + ex.Message);
                    return new PassiveIncomeResult(PassiveIncomeStatus.PersistenceFailed, 0, 0, "Business income save failed; check ClassicGTA.log.");
                }
                return new PassiveIncomeResult(PassiveIncomeStatus.NothingDue, 0, 0, "Business income tracking started today.");
            }

            if (currentUtcDay < _lastPayoutUtcDay)
            {
                _notifications.Log("[Assets] Clock moved backward (current=" + currentUtcDay + " last=" + _lastPayoutUtcDay + "); refusing payout.");
                return new PassiveIncomeResult(PassiveIncomeStatus.ClockMovedBackward, 0, 0, "System clock moved backward; no new business income due.");
            }

            int elapsedDays = currentUtcDay - _lastPayoutUtcDay;
            if (elapsedDays == 0)
            {
                return new PassiveIncomeResult(PassiveIncomeStatus.NothingDue, 0, 0, "No new business income due yet.");
            }

            if (elapsedDays > _offlineCapDays)
            {
                elapsedDays = _offlineCapDays;
            }

            long payoutLong = dailyTotal * elapsedDays;
            int payout = payoutLong > int.MaxValue ? int.MaxValue : (int)payoutLong;

            int previousDay = _lastPayoutUtcDay;
            _lastPayoutUtcDay = currentUtcDay;

            try
            {
                PersistState();
            }
            catch (Exception ex)
            {
                _lastPayoutUtcDay = previousDay;
                _notifications.Log("[Assets] Passive income persist failed: " + ex.Message);
                return new PassiveIncomeResult(PassiveIncomeStatus.PersistenceFailed, 0, 0, "Business income save failed; check ClassicGTA.log.");
            }

            if (payout > 0)
            {
                _economy.Credit(payout, "PassiveIncome:" + elapsedDays + "d");
                string msg = "Business income deposited: $" + payout + " (" + elapsedDays + "d).";
                _notifications.Show(msg);
                _notifications.Log("[Assets] PASSIVE_INCOME days=" + elapsedDays + " payout=" + payout);
                return new PassiveIncomeResult(PassiveIncomeStatus.Paid, payout, elapsedDays, msg);
            }

            return new PassiveIncomeResult(PassiveIncomeStatus.NothingDue, 0, 0, "No new business income due yet.");
        }

        private long ComputeDailyBusinessIncome()
        {
            long dailyTotal = 0;
            foreach (string id in _ownedIds)
            {
                AssetDefinition asset;
                if (_catalog.TryGetValue(id, out asset) && asset.Category == AssetCategory.Business)
                {
                    dailyTotal += asset.DailyPassiveIncome;
                }
            }
            return dailyTotal;
        }

        private bool ApplyParsedStrictRecords(string raw)
        {
            List<string> records;
            if (!PersistenceCodec.TryParseStrict(raw, SchemaHeader, IsValidStrictRecord, out records))
            {
                return false;
            }

            string loadedSlot = null;
            var loadedOwned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            bool hasDay = false;
            int loadedDay = 0;

            for (int i = 0; i < records.Count; i++)
            {
                string line = records[i];
                if (line.StartsWith("SLOT=", StringComparison.Ordinal))
                {
                    loadedSlot = line.Substring("SLOT=".Length).Trim();
                }
                else if (line.StartsWith("LAST_DAY=", StringComparison.Ordinal))
                {
                    int day;
                    if (int.TryParse(line.Substring("LAST_DAY=".Length), out day) && day >= 0)
                    {
                        hasDay = true;
                        loadedDay = day;
                    }
                }
                else if (line.StartsWith("OWNED=", StringComparison.Ordinal))
                {
                    string id = line.Substring("OWNED=".Length).Trim();
                    if (id.Length > 0 && _catalog.ContainsKey(id))
                    {
                        loadedOwned.Add(id);
                    }
                }
            }

            if (!string.Equals(loadedSlot, _slotFingerprint, StringComparison.OrdinalIgnoreCase))
            {
                _loadFailedCorrupt = true;
                _notifications.Log("[Assets] Save-slot mismatch (file=" + loadedSlot + " current=" + _slotFingerprint + "); refusing to load or overwrite.");
                return false;
            }

            _ownedIds.Clear();
            foreach (string id in loadedOwned) _ownedIds.Add(id);
            _hasLastPayoutDay = hasDay;
            _lastPayoutUtcDay = loadedDay;
            _loadFailedCorrupt = false;
            return true;
        }

        private bool ApplyLegacyMigration(string raw)
        {
            string[] lines = raw.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            string loadedSlot = "default";
            var loadedOwned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            bool hasDay = false;
            int loadedDay = 0;

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.Equals(line, PersistenceCodec.Footer, StringComparison.Ordinal)) break;
                if (line.StartsWith("SLOT=", StringComparison.Ordinal))
                {
                    loadedSlot = line.Substring("SLOT=".Length).Trim();
                }
                else if (line.StartsWith("LAST_DAY=", StringComparison.Ordinal))
                {
                    int day;
                    if (int.TryParse(line.Substring("LAST_DAY=".Length), out day) && day >= 0)
                    {
                        hasDay = true;
                        loadedDay = day;
                    }
                }
                else if (line.StartsWith("OWNED=", StringComparison.Ordinal))
                {
                    string id = line.Substring("OWNED=".Length).Trim();
                    if (id.Length > 0 && _catalog.ContainsKey(id))
                    {
                        loadedOwned.Add(id);
                    }
                }
            }

            if (!string.Equals(loadedSlot, "default", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(loadedSlot, _slotFingerprint, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            _ownedIds.Clear();
            foreach (string id in loadedOwned) _ownedIds.Add(id);
            _hasLastPayoutDay = hasDay;
            _lastPayoutUtcDay = loadedDay;
            _loadFailedCorrupt = false;

            try
            {
                PersistState();
                _notifications.Log("[Assets] Migrated legacy v0.6.0 ledger to " + _saveFileName + " (slot=" + _slotFingerprint + ").");
            }
            catch (Exception ex)
            {
                _notifications.Log("[Assets] Legacy ledger loaded into memory but migration write failed: " + ex.Message);
            }

            return true;
        }

        private void PersistState()
        {
            var records = new List<string> { "SLOT=" + _slotFingerprint };
            if (_hasLastPayoutDay)
            {
                records.Add("LAST_DAY=" + _lastPayoutUtcDay);
            }
            foreach (string id in _ownedIds)
            {
                records.Add("OWNED=" + id);
            }
            _store.Save(_saveFileName, PersistenceCodec.Serialize(SchemaHeader, records), ValidateStrictPayload);
        }

        private static bool IsLegacyMigrationEligibleSlot(string slot)
        {
            return string.Equals(slot, ProductionDefaultSlot, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(slot, "default", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ValidateStrictPayload(string text)
        {
            List<string> records;
            if (!PersistenceCodec.TryParseStrict(text, SchemaHeader, IsValidStrictRecord, out records))
            {
                return false;
            }
            for (int i = 0; i < records.Count; i++)
            {
                if (records[i].StartsWith("SLOT=", StringComparison.Ordinal)) return true;
            }
            return false;
        }

        private static bool IsValidStrictRecord(string line)
        {
            if (line.StartsWith("SLOT=", StringComparison.Ordinal))
            {
                return line.Length > "SLOT=".Length;
            }
            if (line.StartsWith("LAST_DAY=", StringComparison.Ordinal))
            {
                int day;
                return int.TryParse(line.Substring("LAST_DAY=".Length), out day) && day >= 0;
            }
            if (line.StartsWith("OWNED=", StringComparison.Ordinal))
            {
                return line.Length > "OWNED=".Length;
            }
            return false;
        }

        private static bool ValidateLegacyPayload(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            string[] lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 1 || !string.Equals(lines[0].Trim(), SchemaHeader, StringComparison.Ordinal))
            {
                return false;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.Equals(line, PersistenceCodec.Footer, StringComparison.Ordinal))
                {
                    return i == lines.Length - 1;
                }
                if (!IsValidStrictRecord(line))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
