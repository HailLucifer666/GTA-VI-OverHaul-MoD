using System;
using ClassicGTA.Contracts;

namespace ClassicGTA.Core.Module2
{
    public enum DispatchState
    {
        Idle = 0,
        EnRoute = 1,
        Departing = 2
    }

    /// <summary>
    /// Module 2 Core Logic: 911 Emergency Services Dispatch (Cut Feature #25).
    /// Supports Police, Ambulance (heals strictly on physical arrival), and Fire Department.
    /// Enforces Online block, single-dispatch cap, cooldown, distance-gated re-tasking,
    /// arrival timeout, responder-death abort, and full EntityOwnershipRegistry cleanup.
    /// </summary>
    public sealed class EmergencyDispatchController : IModModule
    {
        private readonly IGameContext _context;
        private readonly IEmergencyDispatchAdapter _adapter;
        private readonly IEntityRegistry _registry;
        private readonly INotificationService _notifications;
        private readonly EmergencyDispatchConfig _config;

        private bool _isRunning;
        private bool _isEnabled;
        private DispatchState _state;
        private EmergencyServiceType _activeService;
        private int _vehicleHandle;
        private int _responderHandle;
        private int _dispatchStartMs;
        private bool _hasCooldown;
        private int _cooldownUntilMs;
        private WorldPoint _lastRoutedDestination;
        private EmergencyServiceType _quickCycleNext = EmergencyServiceType.Ambulance;

        public string ModuleId { get { return "classic.module2.dispatch"; } }
        public string DisplayName { get { return "911 Emergency Services Dispatch"; } }
        public DispatchState CurrentState { get { return _state; } }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }

        public EmergencyDispatchController(
            IGameContext context,
            IEmergencyDispatchAdapter adapter,
            IEntityRegistry registry,
            INotificationService notifications,
            EmergencyDispatchConfig config)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (adapter == null) throw new ArgumentNullException("adapter");
            if (registry == null) throw new ArgumentNullException("registry");
            if (notifications == null) throw new ArgumentNullException("notifications");
            if (config == null) throw new ArgumentNullException("config");

            config.Validate();

            _context = context;
            _adapter = adapter;
            _registry = registry;
            _notifications = notifications;
            _config = config;
            _isEnabled = config.Enabled;
            _state = DispatchState.Idle;
        }

        public void Start()
        {
            _isRunning = true;
            _state = DispatchState.Idle;
            _hasCooldown = false;
            _notifications.Log("[Module2] Started.");
        }

        public void FastTick()
        {
            // No per-frame work required for 911 dispatch.
        }

        public void SlowTick(int gameTimeMs)
        {
            if (!_isRunning || !_isEnabled || _state == DispatchState.Idle)
            {
                return;
            }

            if (!IsSafeGameplayState())
            {
                return;
            }

            if (_state == DispatchState.EnRoute)
            {
                UpdateEnRoute(gameTimeMs);
            }
            else if (_state == DispatchState.Departing)
            {
                FinishAndCleanup("Completed", true);
            }
        }

        public void OnKeyPressed(GameKey key)
        {
            if (!_isRunning || !_isEnabled || key != _config.QuickDispatchKey)
            {
                return;
            }

            RequestDispatch(_quickCycleNext, _context.GameTimeMs);
            _quickCycleNext = (EmergencyServiceType)(((int)_quickCycleNext + 1) % 3);
        }

        public bool RequestDispatch(EmergencyServiceType service, int gameTimeMs)
        {
            if (!_isRunning || !_isEnabled || !IsSafeGameplayState())
            {
                return false;
            }

            if (_state != DispatchState.Idle)
            {
                _notifications.Show("911 Dispatch: A unit is already en route.");
                return false;
            }

            if (_hasCooldown && gameTimeMs >= _dispatchStartMs && gameTimeMs < _cooldownUntilMs)
            {
                _notifications.Show("911 Dispatch: Please wait before calling again.");
                return false;
            }

            WorldPoint playerPos = _adapter.GetPlayerPosition();
            WorldPoint spawnPos;
            if (!_adapter.TryFindRoadSpawnPoint(playerPos, _config.MinSpawnDistance, _config.MaxSpawnDistance, out spawnPos))
            {
                _notifications.Show("911 Dispatch: No nearby road found.");
                _notifications.Log("[Module2] Spawn road lookup failed.");
                return false;
            }

            DispatchSpawnResult spawned = _adapter.SpawnUnit(service, spawnPos);
            if (spawned == null || !spawned.Succeeded)
            {
                string reason = spawned != null ? spawned.FailureReason : "Null spawn result";
                _notifications.Log("[Module2] Spawn failed: " + reason);
                return false;
            }

            bool trackedVeh = _registry.TryTrackEntity(spawned.VehicleHandle, ModuleId, EntityOwnership.Owned);
            bool trackedPed = _registry.TryTrackEntity(spawned.ResponderPedHandle, ModuleId, EntityOwnership.Owned);
            if (!trackedVeh || !trackedPed)
            {
                // H-02 Fix: Clean up both tracked and untracked allocations on budget refusal.
                _registry.CleanupOwner(ModuleId, false);
                if (!trackedVeh) _registry.DeleteUntrackedEntity(spawned.VehicleHandle);
                if (!trackedPed) _registry.DeleteUntrackedEntity(spawned.ResponderPedHandle);
                _registry.DeleteUntrackedBlip(spawned.BlipHandle);
                _notifications.Log("[Module2] Entity budget exceeded; aborted dispatch.");
                return false;
            }

            if (spawned.BlipHandle > 0)
            {
                _registry.TrackBlip(spawned.BlipHandle, ModuleId);
            }

            _activeService = service;
            _vehicleHandle = spawned.VehicleHandle;
            _responderHandle = spawned.ResponderPedHandle;
            _dispatchStartMs = gameTimeMs;
            _cooldownUntilMs = gameTimeMs + _config.CooldownMs;
            _hasCooldown = true;
            _lastRoutedDestination = playerPos;
            _state = DispatchState.EnRoute;

            _adapter.TaskDriveTo(_responderHandle, _vehicleHandle, playerPos);
            _notifications.Show("911 Dispatch: " + service + " unit en route.");
            _notifications.Log("[Module2] DISPATCH service=" + service + " veh=" + _vehicleHandle + " ped=" + _responderHandle);
            return true;
        }

        public void Stop()
        {
            _isRunning = false;
            if (_state != DispatchState.Idle)
            {
                FinishAndCleanup("Stopped", false);
            }
            _notifications.Log("[Module2] Stopped.");
        }

        public void Dispose()
        {
            Stop();
        }

        private void UpdateEnRoute(int gameTimeMs)
        {
            // Responder or vehicle destroyed en route
            if (!_adapter.IsEntityAlive(_responderHandle) || !_adapter.IsEntityAlive(_vehicleHandle))
            {
                _notifications.Show("911 Dispatch: Unit was incapacitated.");
                FinishAndCleanup("ResponderDead", false);
                return;
            }

            // C-10 Fix: Handle GameTime wrap/reset cleanly using signed elapsed delta.
            bool timedOut = gameTimeMs < _dispatchStartMs || (gameTimeMs - _dispatchStartMs) >= _config.ArrivalTimeoutMs;
            if (timedOut)
            {
                _notifications.Show("911 Dispatch: Unit timed out.");
                FinishAndCleanup("Timeout", false);
                return;
            }

            WorldPoint unitPos;
            if (!_adapter.TryGetEntityPosition(_vehicleHandle, out unitPos))
            {
                FinishAndCleanup("LostPosition", false);
                return;
            }

            WorldPoint playerPos = _adapter.GetPlayerPosition();
            float distanceToPlayer = unitPos.DistanceTo(playerPos);

            if (distanceToPlayer <= _config.ArrivalRadiusMeters)
            {
                ExecuteArrivedService(playerPos);
                _adapter.TaskDepart(_responderHandle, _vehicleHandle);
                _state = DispatchState.Departing;
                return;
            }

            // Anti-pathing-reset rule: only re-task drive if player moved beyond threshold
            if (playerPos.DistanceTo(_lastRoutedDestination) >= _config.RetaskDistanceThreshold)
            {
                _lastRoutedDestination = playerPos;
                _adapter.TaskDriveTo(_responderHandle, _vehicleHandle, playerPos);
            }
        }

        private void ExecuteArrivedService(WorldPoint playerPos)
        {
            if (_activeService == EmergencyServiceType.Ambulance)
            {
                _adapter.HealPlayerToFull();
                _notifications.Show("Paramedics treated your injuries.");
            }
            else if (_activeService == EmergencyServiceType.Police)
            {
                _adapter.TaskEngageNearbyHostiles(_responderHandle);
                _notifications.Show("Police unit arrived on scene.");
            }
            else if (_activeService == EmergencyServiceType.FireDepartment)
            {
                bool extinguished = _adapter.TryExtinguishNearbyFires(playerPos, _config.FireExtinguishRadius);
                _notifications.Show(extinguished
                    ? "Fire Department extinguished nearby fires."
                    : "Fire Department arrived on scene (auto-extinguish unavailable).");
            }

            _notifications.Log("[Module2] ARRIVED service=" + _activeService);
        }

        private void FinishAndCleanup(string reason, bool releaseInsteadOfDelete)
        {
            _registry.CleanupOwner(ModuleId, releaseInsteadOfDelete);
            _vehicleHandle = 0;
            _responderHandle = 0;
            _state = DispatchState.Idle;
            _notifications.Log("[Module2] CLEANUP reason=" + reason + " release=" + releaseInsteadOfDelete);
        }

        private bool IsSafeGameplayState()
        {
            if (_context.IsOnlineSession) return false;
            if (!_context.IsPlayerReady) return false;
            if (_context.IsPaused) return false;
            if (_context.IsInCutscene) return false;
            if (_context.IsLoading) return false;
            if (_context.IsScreenFadedOut) return false;
            return true;
        }
    }
}
