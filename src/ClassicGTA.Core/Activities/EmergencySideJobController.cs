using System;
using ClassicGTA.Contracts;

namespace ClassicGTA.Core.Activities
{
    /// <summary>
    /// Classic 12-Level Vigilante, Paramedic &amp; Firefighter Side-Jobs (Cut Feature #9).
    /// Protects player's emergency vehicle as Borrowed and awards quadratic streak payouts + Level 12 mastery bonus.
    /// </summary>
    public sealed class EmergencySideJobController : IModModule
    {
        public const int MaxLevel = 12;
        public const int MasteryBonusCash = 50000;

        private readonly IGameContext _context;
        private readonly IEmergencySideJobAdapter _adapter;
        private readonly IEconomyService _economy;
        private readonly IEntityRegistry _registry;
        private readonly INotificationService _notifications;

        private bool _isRunning;
        private bool _isEnabled = true;
        private bool _jobActive;
        private EmergencyServiceType _activeJobType;
        private int _vehicleHandle;
        private int _currentLevel;
        private int _targetHandle;
        private int _waveStartMs;
        private int _waveDurationMs;

        public string ModuleId { get { return "classic.activity.emergency_jobs"; } }
        public string DisplayName { get { return "Vigilante / Paramedic / Firefighter Jobs"; } }
        public bool IsJobActive { get { return _jobActive; } }
        public int CurrentLevel { get { return _currentLevel; } }
        public EmergencyServiceType ActiveJobType { get { return _activeJobType; } }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }

        public EmergencySideJobController(
            IGameContext context,
            IEmergencySideJobAdapter adapter,
            IEconomyService economy,
            IEntityRegistry registry,
            INotificationService notifications)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (adapter == null) throw new ArgumentNullException("adapter");
            if (economy == null) throw new ArgumentNullException("economy");
            if (registry == null) throw new ArgumentNullException("registry");
            if (notifications == null) throw new ArgumentNullException("notifications");

            _context = context;
            _adapter = adapter;
            _economy = economy;
            _registry = registry;
            _notifications = notifications;
        }

        public void Start()
        {
            _isRunning = true;
            _jobActive = false;
            _currentLevel = 0;
        }

        public void FastTick() { }

        public bool StartJob(int gameTimeMs)
        {
            if (!_isRunning || !_isEnabled || _jobActive || _context.IsOnlineSession || !_context.IsPlayerReady)
            {
                return false;
            }

            EmergencyServiceType jobType;
            int vehHandle;
            if (!_adapter.IsPlayerInEmergencyVehicle(out jobType, out vehHandle))
            {
                _notifications.Show("Enter a Police, Ambulance, or Fire vehicle to start side-jobs.");
                return false;
            }

            _activeJobType = jobType;
            _vehicleHandle = vehHandle;
            _currentLevel = 1;
            _jobActive = true;

            // Player's emergency vehicle is Borrowed — NEVER delete it on abort/cleanup.
            _registry.TryTrackEntity(_vehicleHandle, ModuleId, EntityOwnership.Borrowed);

            if (!SpawnCurrentWave(gameTimeMs))
            {
                AbortJob("SpawnFailed");
                return false;
            }

            _notifications.Show(_activeJobType + " Mission Started — Level 1!");
            return true;
        }

        public void SlowTick(int gameTimeMs)
        {
            if (!_isRunning || !_isEnabled || !_jobActive) return;
            if (_context.IsOnlineSession || !_context.IsPlayerReady || _context.IsPaused || _context.IsInCutscene) return;

            EmergencyServiceType currentType;
            int currentVeh;
            if (!_adapter.IsPlayerInEmergencyVehicle(out currentType, out currentVeh) || currentVeh != _vehicleHandle)
            {
                AbortJob("ExitedVehicle");
                return;
            }

            int elapsed = gameTimeMs >= _waveStartMs ? (gameTimeMs - _waveStartMs) : 0;
            if (elapsed >= _waveDurationMs)
            {
                _notifications.Show(_activeJobType + " Mission Failed: Time expired.");
                AbortJob("Timeout");
                return;
            }

            WorldPoint playerPos = _adapter.GetPlayerPosition();
            if (_adapter.IsTargetResolved(_activeJobType, _targetHandle, playerPos))
            {
                CompleteCurrentWave(gameTimeMs);
            }
        }

        public void OnKeyPressed(GameKey key) { }

        public void AbortJob(string reason)
        {
            _registry.CleanupOwner(ModuleId, false);
            _jobActive = false;
            _vehicleHandle = 0;
            _targetHandle = 0;
            _currentLevel = 0;
            _notifications.Log("[EmergencyJobs] ABORT reason=" + reason);
        }

        public void Stop()
        {
            _isRunning = false;
            if (_jobActive) AbortJob("Stopped");
        }

        public void Dispose() { Stop(); }

        private bool SpawnCurrentWave(int gameTimeMs)
        {
            int target; WorldPoint loc; int blip;
            if (!_adapter.TrySpawnWaveTarget(_activeJobType, _currentLevel, out target, out loc, out blip))
            {
                return false;
            }

            if (!_registry.TryTrackEntity(target, ModuleId, EntityOwnership.Owned))
            {
                return false;
            }
            if (blip > 0)
            {
                _registry.TrackBlip(blip, ModuleId);
            }

            _targetHandle = target;
            _waveStartMs = gameTimeMs;
            _waveDurationMs = 60000 + (_currentLevel * 5000);
            return true;
        }

        private void CompleteCurrentWave(int gameTimeMs)
        {
            int completedLevel = _currentLevel;
            int waveReward = 50 * completedLevel * completedLevel;

            // Clean up finished wave target & blip (Borrowed vehicle remains untouched).
            _registry.CleanupOwner(ModuleId, true);

            if (completedLevel >= MaxLevel)
            {
                int total = waveReward + MasteryBonusCash;
                _jobActive = false;
                _currentLevel = 0;
                _economy.Credit(total, "EmergencyJobMastery:" + _activeJobType);
                _notifications.Show(_activeJobType + " Level 12 COMPLETE! Mastery Bonus: $" + total);
                return;
            }

            _economy.Credit(waveReward, "EmergencyJob:" + _activeJobType + ":L" + completedLevel);
            _currentLevel = completedLevel + 1;

            // Re-register borrowed vehicle after owner cleanup so next wave stays protected
            _registry.TryTrackEntity(_vehicleHandle, ModuleId, EntityOwnership.Borrowed);

            if (!SpawnCurrentWave(gameTimeMs))
            {
                AbortJob("NextWaveSpawnFailed");
                return;
            }

            _notifications.Show(_activeJobType + " Level " + completedLevel + " cleared (+$" + waveReward + ")! Next: Level " + _currentLevel);
        }
    }
}
