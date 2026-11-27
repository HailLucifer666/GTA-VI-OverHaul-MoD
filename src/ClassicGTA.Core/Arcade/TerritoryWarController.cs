using System;
using System.Collections.Generic;
using ClassicGTA.Contracts;

namespace ClassicGTA.Core.Arcade
{
    public enum TerritoryWarState
    {
        Idle = 0,
        ActiveWave = 1
    }

    public enum TerritoryStartStatus
    {
        Started = 0,
        BlockedInOnline = 1,
        AlreadyActive = 2,
        UnknownZone = 3,
        AlreadyOwned = 4,
        CorruptLedger = 5,
        SpawnBudgetFailed = 6,
        PlayerNotReady = 7
    }

    public enum TurfIncomeStatus
    {
        Paid = 0,
        NothingDue = 1,
        NoZonesOwned = 2,
        BlockedInOnline = 3,
        ClockMovedBackward = 4,
        PersistenceFailed = 5
    }

    public sealed class TurfIncomeResult
    {
        public TurfIncomeStatus Status { get; private set; }
        public int AmountPaid { get; private set; }
        public int ElapsedDays { get; private set; }
        public string Message { get; private set; }

        public TurfIncomeResult(TurfIncomeStatus status, int amountPaid, int elapsedDays, string message)
        {
            Status = status;
            AmountPaid = amountPaid;
            ElapsedDays = elapsedDays;
            Message = message ?? string.Empty;
        }
    }

    /// <summary>
    /// Phase 7 Restoration #5: San Andreas-Style Gang Territory / Turf Wars.
    /// 4 districts, 3 waves (3 enemies/wave), $5,000 capture bonus, $2,500/UTC-day passive turf income
    /// (7-day cap, backward-clock safe), strict PersistenceCodec (TURF_SCHEMA=1), and fail-closed Online guard.
    /// </summary>
    public sealed class TerritoryWarController : IModModule
    {
        public const string SchemaHeader = "TURF_SCHEMA=1";
        public const int TotalWaves = 3;
        public const int EnemiesPerWave = 3;
        public const int CaptureBonus = 5000;
        public const int DailyIncomePerZone = 2500;
        public const int WaveTimeoutMs = 180000;

        private const string WaveOwnerSuffix = ".wave";

        public string ModuleId { get { return "arcade.turf_wars"; } }
        public string DisplayName { get { return "Gang Territory / Turf Wars"; } }
        public bool IsEnabled { get; set; }

        public TerritoryWarState State { get; private set; }
        public string ActiveZoneId { get; private set; }
        public int CurrentWave { get; private set; }
        public int CapturedZoneCount { get { return _capturedZones.Count; } }
        public string SaveFileName { get { return _saveFileName; } }
        public bool LoadFailedCorrupt { get { return _loadFailedCorrupt; } }

        private readonly IGameContext _context;
        private readonly ITerritoryWarAdapter _adapter;
        private readonly IEconomyService _economy;
        private readonly IEntityRegistry _registry;
        private readonly IPersistenceStore _store;
        private readonly INotificationService _notifications;
        private readonly string _slotFingerprint;
        private readonly string _saveFileName;
        private readonly int _offlineCapDays;
        private readonly Func<int> _utcDayProvider;

        private readonly Dictionary<string, string> _zones =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> _capturedZones =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly List<int> _activeWaveEnemies = new List<int>();

        private bool _started;
        private bool _disposed;
        private bool _loadFailedCorrupt;
        private bool _hasLastPayoutDay;
        private int _lastPayoutUtcDay;

        private bool _hasLastTick;
        private int _lastTickMs;
        private int _waveElapsedMs;

        public TerritoryWarController(
            IGameContext context,
            ITerritoryWarAdapter adapter,
            IEconomyService economy,
            IEntityRegistry registry,
            IPersistenceStore store,
            INotificationService notifications,
            string slotFingerprint = "default",
            int offlineCapDays = 7,
            Func<int> utcDayProvider = null)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (adapter == null) throw new ArgumentNullException("adapter");
            if (economy == null) throw new ArgumentNullException("economy");
            if (registry == null) throw new ArgumentNullException("registry");
            if (store == null) throw new ArgumentNullException("store");
            if (notifications == null) throw new ArgumentNullException("notifications");
            if (offlineCapDays < 1 || offlineCapDays > 30) throw new ArgumentOutOfRangeException("offlineCapDays");

            _context = context;
            _adapter = adapter;
            _economy = economy;
            _registry = registry;
            _store = store;
            _notifications = notifications;
            _slotFingerprint = string.IsNullOrWhiteSpace(slotFingerprint) ? "default" : slotFingerprint.Trim();
            _saveFileName = "ClassicGTA_Turf_" + PersistenceCodec.SanitizeSlotId(_slotFingerprint) + ".dat";
            _offlineCapDays = offlineCapDays;
            _utcDayProvider = utcDayProvider;

            IsEnabled = true;
            State = TerritoryWarState.Idle;

            _zones.Add("turf.davis", "Davis");
            _zones.Add("turf.del_perro", "Del Perro");
            _zones.Add("turf.downtown", "Downtown");
            _zones.Add("turf.docks", "LS Docks");
        }

        public bool IsZoneCaptured(string zoneId)
        {
            return !string.IsNullOrWhiteSpace(zoneId) && _capturedZones.Contains(zoneId.Trim());
        }

        public bool LoadState()
        {
            string raw;
            if (_store.TryLoad(_saveFileName, ValidateStrictPayload, out raw))
            {
                return ApplyParsedRecords(raw);
            }

            if (_store.Exists(_saveFileName))
            {
                _loadFailedCorrupt = true;
                _notifications.Log("[Turf] Failed to load existing turf ledger " + _saveFileName + "; refusing to overwrite.");
                return false;
            }

            return false;
        }

        public void Start()
        {
            if (_disposed) return;
            _started = true;
        }

        public TerritoryStartStatus TryStartTakeover(string zoneId, int gameTimeMs)
        {
            if (_context.IsOnlineSession)
            {
                return TerritoryStartStatus.BlockedInOnline;
            }

            if (_disposed || !_context.IsPlayerReady || _context.IsLoading)
            {
                return TerritoryStartStatus.PlayerNotReady;
            }

            if (_loadFailedCorrupt)
            {
                _notifications.Show("~r~Turf ledger corrupt; refusing to start takeover.");
                return TerritoryStartStatus.CorruptLedger;
            }

            if (State != TerritoryWarState.Idle)
            {
                return TerritoryStartStatus.AlreadyActive;
            }

            string normalized = string.IsNullOrWhiteSpace(zoneId) ? string.Empty : zoneId.Trim();
            string zoneName;
            if (normalized.Length == 0 || !_zones.TryGetValue(normalized, out zoneName))
            {
                return TerritoryStartStatus.UnknownZone;
            }

            if (_capturedZones.Contains(normalized))
            {
                return TerritoryStartStatus.AlreadyOwned;
            }

            int blipHandle = _adapter.CreateZoneBlip(normalized);
            if (blipHandle > 0)
            {
                _registry.TrackBlip(blipHandle, ModuleId);
            }

            ActiveZoneId = normalized;
            CurrentWave = 1;
            State = TerritoryWarState.ActiveWave;
            _waveElapsedMs = 0;
            _hasLastTick = true;
            _lastTickMs = gameTimeMs;

            if (!SpawnWave(normalized, CurrentWave))
            {
                AbortTakeover("SpawnBudgetFailed", false);
                return TerritoryStartStatus.SpawnBudgetFailed;
            }

            _notifications.Show("Turf War started in " + zoneName + "! Wave 1/" + TotalWaves + ".");
            return TerritoryStartStatus.Started;
        }

        public void AbortTakeover(string reason, bool isOnline)
        {
            _activeWaveEnemies.Clear();
            State = TerritoryWarState.Idle;
            ActiveZoneId = null;
            CurrentWave = 0;
            _hasLastTick = false;
            _waveElapsedMs = 0;

            if (isOnline)
            {
                _registry.DropOwnerWithoutDeleting(WaveOwnerId);
                _registry.DropOwnerWithoutDeleting(ModuleId);
            }
            else
            {
                _registry.CleanupOwner(WaveOwnerId, false);
                _registry.CleanupOwner(ModuleId, false);
            }

            _notifications.Log("[Turf] Takeover aborted: " + (reason ?? "Unknown"));
        }

        public void FastTick()
        {
        }

        public void SlowTick(int gameTimeMs)
        {
            if (_context.IsOnlineSession)
            {
                if (State == TerritoryWarState.ActiveWave)
                {
                    _registry.LatchOnlineSession();
                    AbortTakeover("OnlineSession", true);
                }
                return;
            }

            if (!IsEnabled || !_started || _disposed || State != TerritoryWarState.ActiveWave)
            {
                return;
            }

            if (!_context.IsPlayerReady || _context.IsLoading)
            {
                return;
            }

            if (_context.IsInCutscene || _context.IsPaused)
            {
                // Pause wave timer without resetting progress.
                _hasLastTick = false;
                return;
            }

            if (!_hasLastTick || gameTimeMs < _lastTickMs)
            {
                _hasLastTick = true;
                _lastTickMs = gameTimeMs;
            }
            else
            {
                _waveElapsedMs += (gameTimeMs - _lastTickMs);
                _lastTickMs = gameTimeMs;
            }

            if (_waveElapsedMs >= WaveTimeoutMs)
            {
                AbortTakeover("WaveTimeout", false);
                _notifications.Show("Turf War timed out.");
                return;
            }

            bool anyAlive = false;
            for (int i = 0; i < _activeWaveEnemies.Count; i++)
            {
                if (_adapter.IsEntityAlive(_activeWaveEnemies[i]))
                {
                    anyAlive = true;
                    break;
                }
            }

            if (anyAlive)
            {
                return;
            }

            // Wave cleared!
            if (CurrentWave < TotalWaves)
            {
                _activeWaveEnemies.Clear();
                _registry.CleanupOwner(WaveOwnerId, false);
                CurrentWave++;
                _waveElapsedMs = 0;
                _hasLastTick = true;
                _lastTickMs = gameTimeMs;

                if (!SpawnWave(ActiveZoneId, CurrentWave))
                {
                    AbortTakeover("SpawnBudgetFailed", false);
                    return;
                }

                _notifications.Show("Turf War: Wave " + CurrentWave + "/" + TotalWaves + " incoming!");
                return;
            }

            // Final wave cleared -> capture district!
            string capturedZone = ActiveZoneId;
            _activeWaveEnemies.Clear();
            State = TerritoryWarState.Idle;
            ActiveZoneId = null;
            CurrentWave = 0;
            _hasLastTick = false;
            _waveElapsedMs = 0;

            _registry.CleanupOwner(WaveOwnerId, false);
            _registry.CleanupOwner(ModuleId, false);

            bool prevHasDay = _hasLastPayoutDay;
            int prevDay = _lastPayoutUtcDay;
            _capturedZones.Add(capturedZone);
            if (!_hasLastPayoutDay && _utcDayProvider != null)
            {
                int captureDay = _utcDayProvider();
                if (captureDay >= 0)
                {
                    _hasLastPayoutDay = true;
                    _lastPayoutUtcDay = captureDay;
                }
            }

            try
            {
                PersistState();
            }
            catch (Exception ex)
            {
                _capturedZones.Remove(capturedZone);
                _hasLastPayoutDay = prevHasDay;
                _lastPayoutUtcDay = prevDay;
                _notifications.Log("[Turf] Persist failed; rolled back capture for " + capturedZone + ": " + ex.Message);
                _notifications.Show("~r~Turf capture save failed; ownership rolled back.");
                return;
            }

            _economy.Credit(CaptureBonus, "TurfCapture:" + capturedZone);
            string displayName = _zones.ContainsKey(capturedZone) ? _zones[capturedZone] : capturedZone;
            _notifications.Show("District captured: " + displayName + "! Bonus ~g~+$" + CaptureBonus.ToString("N0"));
        }

        public TurfIncomeResult CollectTurfIncome(int currentUtcDay)
        {
            if (_context.IsOnlineSession)
            {
                return new TurfIncomeResult(TurfIncomeStatus.BlockedInOnline, 0, 0, "Blocked in Online session.");
            }

            if (currentUtcDay < 0)
            {
                return new TurfIncomeResult(TurfIncomeStatus.ClockMovedBackward, 0, 0, "System clock invalid; no turf income paid.");
            }

            if (_loadFailedCorrupt)
            {
                return new TurfIncomeResult(TurfIncomeStatus.PersistenceFailed, 0, 0, "Turf ledger failed to load; check ClassicGTA.log.");
            }

            if (_capturedZones.Count == 0)
            {
                return new TurfIncomeResult(TurfIncomeStatus.NoZonesOwned, 0, 0, "No districts captured yet.");
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
                    _notifications.Log("[Turf] Baseline persist failed: " + ex.Message);
                    return new TurfIncomeResult(TurfIncomeStatus.PersistenceFailed, 0, 0, "Turf income save failed.");
                }
                return new TurfIncomeResult(TurfIncomeStatus.NothingDue, 0, 0, "Turf income tracking started today.");
            }

            if (currentUtcDay < _lastPayoutUtcDay)
            {
                return new TurfIncomeResult(TurfIncomeStatus.ClockMovedBackward, 0, 0, "System clock moved backward; no turf income paid.");
            }

            int elapsedDays = currentUtcDay - _lastPayoutUtcDay;
            if (elapsedDays == 0)
            {
                return new TurfIncomeResult(TurfIncomeStatus.NothingDue, 0, 0, "No new turf income due yet.");
            }

            if (elapsedDays > _offlineCapDays)
            {
                elapsedDays = _offlineCapDays;
            }

            long payoutLong = (long)_capturedZones.Count * DailyIncomePerZone * elapsedDays;
            int payout = payoutLong > int.MaxValue ? int.MaxValue : (int)payoutLong;

            int prevDay = _lastPayoutUtcDay;
            _lastPayoutUtcDay = currentUtcDay;

            try
            {
                PersistState();
            }
            catch (Exception ex)
            {
                _lastPayoutUtcDay = prevDay;
                _notifications.Log("[Turf] Turf income persist failed: " + ex.Message);
                return new TurfIncomeResult(TurfIncomeStatus.PersistenceFailed, 0, 0, "Turf income save failed.");
            }

            _economy.Credit(payout, "TurfIncome:" + elapsedDays + "d");
            string msg = "Turf income deposited: $" + payout + " (" + _capturedZones.Count + " zones, " + elapsedDays + "d).";
            _notifications.Show(msg);
            return new TurfIncomeResult(TurfIncomeStatus.Paid, payout, elapsedDays, msg);
        }

        public void OnKeyPressed(GameKey key)
        {
        }

        public void Stop()
        {
            if (State == TerritoryWarState.ActiveWave)
            {
                AbortTakeover("ModuleStopped", _context.IsOnlineSession);
            }
            _started = false;
        }

        public void Dispose()
        {
            if (_disposed) return;
            Stop();
            _disposed = true;
        }

        private string WaveOwnerId { get { return ModuleId + WaveOwnerSuffix; } }

        private bool SpawnWave(string zoneId, int waveNumber)
        {
            for (int i = 0; i < EnemiesPerWave; i++)
            {
                int handle = _adapter.SpawnWaveEnemy(zoneId, waveNumber);
                if (handle <= 0)
                {
                    return false;
                }

                if (!_registry.TryTrackEntity(handle, WaveOwnerId, EntityOwnership.Owned))
                {
                    _registry.DeleteUntrackedEntity(handle);
                    return false;
                }

                _activeWaveEnemies.Add(handle);
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
            foreach (string zone in _capturedZones)
            {
                records.Add("CAPTURED=" + zone);
            }

            _store.Save(_saveFileName, PersistenceCodec.Serialize(SchemaHeader, records), ValidateStrictPayload);
        }

        private bool ApplyParsedRecords(string raw)
        {
            List<string> records;
            if (!PersistenceCodec.TryParseStrict(raw, SchemaHeader, IsValidRecord, out records))
            {
                return false;
            }

            string loadedSlot = null;
            bool hasDay = false;
            int loadedDay = 0;
            var loadedZones = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
                else if (line.StartsWith("CAPTURED=", StringComparison.Ordinal))
                {
                    string z = line.Substring("CAPTURED=".Length).Trim();
                    if (z.Length > 0 && _zones.ContainsKey(z))
                    {
                        loadedZones.Add(z);
                    }
                }
            }

            if (!string.Equals(loadedSlot, _slotFingerprint, StringComparison.OrdinalIgnoreCase))
            {
                _loadFailedCorrupt = true;
                _notifications.Log("[Turf] Save-slot mismatch (file=" + loadedSlot + " current=" + _slotFingerprint + "); refusing to load or overwrite.");
                return false;
            }

            _capturedZones.Clear();
            foreach (string z in loadedZones) _capturedZones.Add(z);
            _hasLastPayoutDay = hasDay;
            _lastPayoutUtcDay = loadedDay;
            _loadFailedCorrupt = false;
            return true;
        }

        private static bool ValidateStrictPayload(string text)
        {
            List<string> records;
            if (!PersistenceCodec.TryParseStrict(text, SchemaHeader, IsValidRecord, out records))
            {
                return false;
            }

            for (int i = 0; i < records.Count; i++)
            {
                if (records[i].StartsWith("SLOT=", StringComparison.Ordinal)) return true;
            }
            return false;
        }

        private static bool IsValidRecord(string line)
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
            if (line.StartsWith("CAPTURED=", StringComparison.Ordinal))
            {
                return line.Length > "CAPTURED=".Length;
            }
            return false;
        }
    }
}
