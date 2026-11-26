using System;
using ClassicGTA.Contracts;

namespace ClassicGTA.Core.Activities
{
    /// <summary>
    /// Free-Roam First-Person Camera Controller (Cut Feature #18).
    /// Clamps pitch to +/-75 degrees, switches between foot and vehicle attachment,
    /// and guarantees RestoreRenderingCamera() before camera deletion on cutscene/pause/death/abort.
    /// </summary>
    public sealed class FirstPersonCameraController : IModModule
    {
        public const float MinPitchDegrees = -75f;
        public const float MaxPitchDegrees = 75f;

        private readonly IGameContext _context;
        private readonly IFirstPersonCameraAdapter _adapter;
        private readonly IEntityRegistry _registry;
        private readonly INotificationService _notifications;
        private readonly GameKey _toggleKey;

        private bool _isRunning;
        private bool _isEnabled = true;
        private bool _isFirstPersonActive;
        private int _cameraHandle;
        private bool _cameraInVehicleMode;
        private float _pitchDegrees;
        private float _yawDegrees;

        public string ModuleId { get { return "classic.camera.firstperson"; } }
        public string DisplayName { get { return "Free-Roam First-Person Camera"; } }
        public bool IsFirstPersonActive { get { return _isFirstPersonActive; } }
        public float CurrentPitchDegrees { get { return _pitchDegrees; } }
        public float CurrentYawDegrees { get { return _yawDegrees; } }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }

        public FirstPersonCameraController(
            IGameContext context,
            IFirstPersonCameraAdapter adapter,
            IEntityRegistry registry,
            INotificationService notifications,
            GameKey toggleKey = GameKey.F11)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (adapter == null) throw new ArgumentNullException("adapter");
            if (registry == null) throw new ArgumentNullException("registry");
            if (notifications == null) throw new ArgumentNullException("notifications");

            _context = context;
            _adapter = adapter;
            _registry = registry;
            _notifications = notifications;
            _toggleKey = toggleKey;
        }

        public void Start()
        {
            _isRunning = true;
            _isFirstPersonActive = false;
            _cameraHandle = 0;
        }

        public void OnKeyPressed(GameKey key)
        {
            if (!_isRunning || !_isEnabled || key != _toggleKey) return;
            ToggleFirstPerson();
        }

        public bool ToggleFirstPerson()
        {
            if (_isFirstPersonActive)
            {
                DeactivateCamera("UserToggle");
                return false;
            }

            if (!IsSafeCameraState())
            {
                return false;
            }

            ActivateCamera(_context.IsInVehicle);
            return _isFirstPersonActive;
        }

        public void FastTick()
        {
            if (!_isRunning || !_isEnabled || !_isFirstPersonActive) return;

            // Immediately restore vanilla rendering camera if gameplay becomes unsafe (cutscene/pause/dead/Online).
            if (!IsSafeCameraState())
            {
                DeactivateCamera("UnsafeState");
                return;
            }

            // Re-attach camera cleanly when switching between on-foot and vehicle.
            if (_context.IsInVehicle != _cameraInVehicleMode)
            {
                DeactivateCamera("ModeSwitch");
                ActivateCamera(_context.IsInVehicle);
                if (!_isFirstPersonActive) return;
            }

            float deltaYaw;
            float deltaPitch;
            _adapter.ReadLookDelta(out deltaYaw, out deltaPitch);

            _yawDegrees = NormalizeYaw(_yawDegrees + deltaYaw);
            _pitchDegrees = Clamp(_pitchDegrees + deltaPitch, MinPitchDegrees, MaxPitchDegrees);

            _adapter.UpdateCameraLook(_cameraHandle, _pitchDegrees, _yawDegrees, _cameraInVehicleMode);
            if (!_cameraInVehicleMode)
            {
                _adapter.SyncPlayerHeading(_yawDegrees);
            }
        }

        public void SlowTick(int gameTimeMs) { }

        public void Stop()
        {
            _isRunning = false;
            if (_isFirstPersonActive)
            {
                DeactivateCamera("Stopped");
            }
        }

        public void Dispose() { Stop(); }

        private void ActivateCamera(bool inVehicle)
        {
            int handle = _adapter.CreateAttachedCamera(inVehicle);
            if (handle <= 0) return;

            _registry.TrackCamera(handle, ModuleId);
            _adapter.ActivateCamera(handle);

            _cameraHandle = handle;
            _cameraInVehicleMode = inVehicle;
            _pitchDegrees = 0f;
            _isFirstPersonActive = true;
            _notifications.Log("[FirstPerson] ACTIVATED cam=" + handle + " inVehicle=" + inVehicle);
        }

        private void DeactivateCamera(string reason)
        {
            // Registry guarantees RestoreRenderingCamera() is called strictly BEFORE DeleteCamera().
            _registry.CleanupOwner(ModuleId, false);
            _cameraHandle = 0;
            _isFirstPersonActive = false;
            _notifications.Log("[FirstPerson] DEACTIVATED reason=" + reason);
        }

        private bool IsSafeCameraState()
        {
            if (_context.IsOnlineSession) return false;
            if (!_context.IsPlayerReady) return false;
            if (_context.IsPaused) return false;
            if (_context.IsInCutscene) return false;
            if (_context.IsLoading) return false;
            if (_context.IsScreenFadedOut) return false;
            return true;
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private static float NormalizeYaw(float yaw)
        {
            while (yaw > 180f) yaw -= 360f;
            while (yaw < -180f) yaw += 360f;
            return yaw;
        }
    }
}
