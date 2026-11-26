using System;
using System.Collections.Generic;
using System.Text;
using ClassicGTA.Contracts;

namespace ClassicGTA.Core.Economy
{
    /// <summary>
    /// Manages Safehouses (#1), Passive-Income Businesses (#2), Boats/Planes/Helicopters (#3),
    /// and Submarines (#4) using stable ContentId keys, save-slot isolation, idempotent refund-on-failure
    /// purchases, and monotonic cross-restart passive income capped at OfflineCapDays.
    /// </summary>
    public sealed class AssetOwnershipEngine
    {
        private const string SaveFileName = "ClassicGTA_Assets.dat";
        private const string SchemaHeader = "SCHEMA=1";
        private const string SchemaFooter = "END_OF_FILE";

        private readonly IGameContext _context;
        private readonly IEconomyService _economy;
        private readonly IPersistenceStore _store;
        private readonly INotificationService _notifications;
        private readonly int _offlineCapDays;
        private readonly string _slotFingerprint;

        private readonly Dictionary<string, AssetDefinition> _catalog =
            new Dictionary<string, AssetDefinition>(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> _ownedIds =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private bool _hasLastPayoutDay;
        private int _lastPayoutInGameDay;

        public int OwnedCount { get { return _ownedIds.Count; } }
        public string SlotFingerprint { get { return _slotFingerprint; } }

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
            if (!_store.TryLoad(SaveFileName, ValidatePayload, out raw))
            {
                return false;
            }

            string loadedSlot = "default";
            var loadedOwned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            bool hasDay = false;
            int loadedDay = 0;

            string[] lines = raw.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("SLOT=", StringComparison.Ordinal))
                {
                    loadedSlot = line.Substring("SLOT=".Length).Trim();
                }
                else if (line.StartsWith("LAST_DAY=", StringComparison.Ordinal))
                {
                    int day;
                    if (int.TryParse(line.Substring("LAST_DAY=".Length), out day))
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

            // C-1 Fix: Refuse to load ledger written under a different save-slot fingerprint.
            if (!string.Equals(loadedSlot, _slotFingerprint, StringComparison.OrdinalIgnoreCase))
            {
                _notifications.Log("[Assets] Save-slot mismatch (file=" + loadedSlot + " current=" + _slotFingerprint + "); starting clean.");
                return false;
            }

            _ownedIds.Clear();
            foreach (string id in loadedOwned) _ownedIds.Add(id);
            _hasLastPayoutDay = hasDay;
            _lastPayoutInGameDay = loadedDay;
            return true;
        }

        public PurchaseResult TryPurchase(string contentId)
        {
            if (_context.IsOnlineSession)
            {
                return new PurchaseResult(PurchaseStatus.BlockedInOnline, "Blocked in Online session.");
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

            _ownedIds.Add(asset.ContentId);

            try
            {
                PersistState();
            }
            catch (Exception ex)
            {
                _ownedIds.Remove(asset.ContentId);
                _economy.Credit(asset.Price, "Refund:" + asset.ContentId);
                _notifications.Log("[Assets] Persist failed for " + asset.ContentId + ", refunded: " + ex.Message);
                return new PurchaseResult(PurchaseStatus.PersistenceFailedAndRefunded, "Save failed; cash refunded.");
            }

            _notifications.Show("Purchased " + asset.DisplayName + " for $" + asset.Price + ".");
            _notifications.Log("[Assets] PURCHASED id=" + asset.ContentId + " price=" + asset.Price);
            return new PurchaseResult(PurchaseStatus.Succeeded, "Purchased " + asset.DisplayName + ".");
        }

        /// <summary>
        /// Collects passive business income using a monotonic day counter.
        /// If currentInGameDay &lt; _lastPayoutInGameDay and allowSessionRestartRollover is true (e.g., new game session),
        /// treats the restart as 1 elapsed day instead of freezing forever.
        /// </summary>
        public int CollectPassiveIncome(int currentInGameDay, bool allowSessionRestartRollover = false)
        {
            if (_context.IsOnlineSession || currentInGameDay < 0)
            {
                return 0;
            }

            if (!_hasLastPayoutDay)
            {
                _hasLastPayoutDay = true;
                _lastPayoutInGameDay = currentInGameDay;
                try
                {
                    PersistState();
                }
                catch (Exception ex)
                {
                    // R-1 Fix: Never let first-call persistence failure throw out of UI click handler.
                    _hasLastPayoutDay = false;
                    _notifications.Log("[Assets] Baseline persist failed: " + ex.Message);
                }
                return 0;
            }

            int elapsedDays;
            if (currentInGameDay < _lastPayoutInGameDay)
            {
                if (!allowSessionRestartRollover)
                {
                    return 0;
                }
                // C-2 Fix: Session restarted with reset clock; advance monotonically.
                elapsedDays = 1;
            }
            else if (currentInGameDay == _lastPayoutInGameDay)
            {
                return 0;
            }
            else
            {
                elapsedDays = currentInGameDay - _lastPayoutInGameDay;
            }

            if (elapsedDays > _offlineCapDays)
            {
                elapsedDays = _offlineCapDays;
            }

            long dailyTotal = 0;
            foreach (string id in _ownedIds)
            {
                AssetDefinition asset;
                if (_catalog.TryGetValue(id, out asset) && asset.Category == AssetCategory.Business)
                {
                    dailyTotal += asset.DailyPassiveIncome;
                }
            }

            long payoutLong = dailyTotal * elapsedDays;
            int payout = payoutLong > int.MaxValue ? int.MaxValue : (int)payoutLong;

            int previousDay = _lastPayoutInGameDay;
            _lastPayoutInGameDay = currentInGameDay;

            try
            {
                PersistState();
            }
            catch (Exception ex)
            {
                _lastPayoutInGameDay = previousDay;
                _notifications.Log("[Assets] Passive income persist failed: " + ex.Message);
                return 0;
            }

            if (payout > 0)
            {
                _economy.Credit(payout, "PassiveIncome:" + elapsedDays + "d");
                _notifications.Show("Business income deposited: $" + payout + " (" + elapsedDays + "d).");
                _notifications.Log("[Assets] PASSIVE_INCOME days=" + elapsedDays + " payout=" + payout);
            }

            return payout;
        }

        private void PersistState()
        {
            var sb = new StringBuilder();
            sb.AppendLine(SchemaHeader);
            sb.Append("SLOT=").AppendLine(_slotFingerprint);
            if (_hasLastPayoutDay)
            {
                sb.Append("LAST_DAY=").AppendLine(_lastPayoutInGameDay.ToString());
            }
            foreach (string id in _ownedIds)
            {
                sb.Append("OWNED=").AppendLine(id);
            }
            sb.AppendLine(SchemaFooter);
            _store.Save(SaveFileName, sb.ToString());
        }

        private static bool ValidatePayload(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            string trimmed = text.Trim();
            // V-16 Fix: Accept both new END_OF_FILE-terminated ledgers and legacy SCHEMA=1 ledgers (auto-migrated on next save).
            return trimmed.StartsWith(SchemaHeader, StringComparison.Ordinal);
        }
    }
}
