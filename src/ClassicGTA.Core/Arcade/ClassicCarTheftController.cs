using System;
using ClassicGTA.Contracts;

namespace ClassicGTA.Core.Arcade
{
    /// <summary>
    /// Phase 7 Restoration #1: Classic Instant Car Theft.
    /// Bypasses GTA VI lockpick/alarm/QTE friction in Story Mode while remaining 100% engine-agnostic.
    /// World/player vehicles touched by this module are NEVER tracked as Owned and NEVER deleted.
    /// </summary>
    public sealed class ClassicCarTheftController : IModModule
    {
        public string ModuleId { get { return "arcade.car_theft"; } }
        public string DisplayName { get { return "Classic Instant Car Theft"; } }
        public bool IsEnabled { get; set; }

        public bool InstantUnlockEnabled { get; set; }
        public bool DisableAlarmsEnabled { get; set; }
        public bool PacifyDriversEnabled { get; set; }
        public int VehiclesUnlockedCount { get; private set; }

        private readonly IGameContext _context;
        private readonly ICarTheftAdapter _adapter;
        private readonly INotificationService _notifications;

        private bool _started;
        private bool _disposed;
        private int _lastProcessedVehicleHandle;

        public ClassicCarTheftController(
            IGameContext context,
            ICarTheftAdapter adapter,
            INotificationService notifications)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (adapter == null) throw new ArgumentNullException("adapter");
            if (notifications == null) throw new ArgumentNullException("notifications");

            _context = context;
            _adapter = adapter;
            _notifications = notifications;

            IsEnabled = true;
            InstantUnlockEnabled = true;
            DisableAlarmsEnabled = true;
            PacifyDriversEnabled = true;
        }

        public void Start()
        {
            if (_disposed) return;
            _started = true;
            _lastProcessedVehicleHandle = 0;
        }

        public void FastTick()
        {
            // No per-frame native calls needed; handled cleanly on SlowTick.
        }

        public void SlowTick(int gameTimeMs)
        {
            if (!CanOperate())
            {
                if (_context.IsOnlineSession)
                {
                    _lastProcessedVehicleHandle = 0;
                }
                return;
            }

            if (!InstantUnlockEnabled) return;

            int handle = _adapter.GetTargetVehicleHandle();
            if (handle <= 0)
            {
                _lastProcessedVehicleHandle = 0;
                return;
            }

            if (handle == _lastProcessedVehicleHandle)
            {
                return;
            }

            if (!_adapter.IsVehicleLocked(handle))
            {
                _lastProcessedVehicleHandle = handle;
                return;
            }

            if (_adapter.UnlockVehicle(handle))
            {
                VehiclesUnlockedCount++;
                if (DisableAlarmsEnabled)
                {
                    _adapter.DisableAlarm(handle);
                }
                if (PacifyDriversEnabled)
                {
                    _adapter.PacifyDriver(handle);
                }
                _lastProcessedVehicleHandle = handle;
                _notifications.Show("Classic Theft: Vehicle unlocked.");
            }
        }

        public void OnKeyPressed(GameKey key)
        {
        }

        public void Stop()
        {
            _started = false;
            _lastProcessedVehicleHandle = 0;
        }

        public void Dispose()
        {
            if (_disposed) return;
            Stop();
            _disposed = true;
        }

        private bool CanOperate()
        {
            if (_context.IsOnlineSession) return false;
            if (!IsEnabled || !_started || _disposed) return false;
            if (!_context.IsPlayerReady || _context.IsInCutscene || _context.IsPaused || _context.IsLoading) return false;
            return true;
        }
    }
}
