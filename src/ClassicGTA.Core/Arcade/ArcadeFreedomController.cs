using System;
using ClassicGTA.Contracts;

namespace ClassicGTA.Core.Arcade
{
    /// <summary>
    /// Phase 7 Restoration #3: Arcade Freedom (No Refuelling, No Survival Chores, Optional No Vignette).
    /// Cleanly restores vignette state on Stop/Dispose and performs zero calls in Online sessions.
    /// </summary>
    public sealed class ArcadeFreedomController : IModModule
    {
        public string ModuleId { get { return "arcade.freedom"; } }
        public string DisplayName { get { return "Arcade Freedom"; } }
        public bool IsEnabled { get; set; }

        public bool NoRefuellingEnabled { get; set; }
        public bool NoSurvivalChoresEnabled { get; set; }
        public bool NoVignetteEnabled { get; set; }

        private readonly IGameContext _context;
        private readonly IArcadeFreedomAdapter _adapter;
        private readonly INotificationService _notifications;

        private bool _started;
        private bool _disposed;
        private bool _vignetteCurrentlySuppressed;

        public ArcadeFreedomController(
            IGameContext context,
            IArcadeFreedomAdapter adapter,
            INotificationService notifications)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (adapter == null) throw new ArgumentNullException("adapter");
            if (notifications == null) throw new ArgumentNullException("notifications");

            _context = context;
            _adapter = adapter;
            _notifications = notifications;

            IsEnabled = true;
            NoRefuellingEnabled = true;
            NoSurvivalChoresEnabled = true;
            NoVignetteEnabled = false;
        }

        public void Start()
        {
            if (_disposed) return;
            _started = true;
        }

        public void FastTick()
        {
        }

        public void SlowTick(int gameTimeMs)
        {
            if (_context.IsOnlineSession) return;
            if (!IsEnabled || !_started || _disposed) return;
            if (!_context.IsPlayerReady || _context.IsInCutscene || _context.IsPaused || _context.IsLoading) return;

            if (NoRefuellingEnabled && _context.IsInVehicle)
            {
                _adapter.TopUpVehicleFuelAndHealth();
            }

            if (NoSurvivalChoresEnabled)
            {
                _adapter.RestorePlayerStaminaAndVitals();
            }

            if (NoVignetteEnabled != _vignetteCurrentlySuppressed)
            {
                _adapter.SetVignetteSuppressed(NoVignetteEnabled);
                _vignetteCurrentlySuppressed = NoVignetteEnabled;
            }
        }

        public void OnKeyPressed(GameKey key)
        {
        }

        public void Stop()
        {
            if (_vignetteCurrentlySuppressed && !_context.IsOnlineSession)
            {
                _adapter.SetVignetteSuppressed(false);
                _vignetteCurrentlySuppressed = false;
            }
            _started = false;
        }

        public void Dispose()
        {
            if (_disposed) return;
            Stop();
            _disposed = true;
        }
    }
}
