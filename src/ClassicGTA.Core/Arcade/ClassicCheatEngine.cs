using System;
using System.Collections.Generic;
using ClassicGTA.Contracts;

namespace ClassicGTA.Core.Arcade
{
    /// <summary>
    /// Phase 7 Restoration #4: Classic Sandbox Cheat Hub (Story Mode only).
    /// Fail-closed on Online sessions, allowlist-validated models and weather IDs,
    /// and registry-enforced Detached tracking for spawned classic vehicles.
    /// </summary>
    public sealed class ClassicCheatEngine : IModModule
    {
        public string ModuleId { get { return "arcade.cheats"; } }
        public string DisplayName { get { return "Classic Sandbox Cheats"; } }
        public bool IsEnabled { get; set; }

        public bool SuperJumpEnabled { get; set; }

        private bool _neverWantedEnabled;
        public bool NeverWantedEnabled
        {
            get { return _neverWantedEnabled; }
            set { SetNeverWanted(value); }
        }

        private static readonly HashSet<string> AllowedVehicleModels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "cheetah2",
            "infernus2",
            "sanchez",
            "turismo2",
            "bati",
            "elegy",
            "comet2",
            "banshee",
            "bullet",
            "sultan"
        };

        private static readonly HashSet<string> AllowedWeatherIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "EXTRASUNNY",
            "CLEAR",
            "CLOUDS",
            "OVERCAST",
            "RAIN",
            "THUNDER",
            "CLEARING",
            "FOGGY",
            "XMAS"
        };

        private readonly IGameContext _context;
        private readonly ISandboxCheatAdapter _adapter;
        private readonly IEntityRegistry _registry;
        private readonly INotificationService _notifications;

        private bool _started;
        private bool _disposed;

        public ClassicCheatEngine(
            IGameContext context,
            ISandboxCheatAdapter adapter,
            IEntityRegistry registry,
            INotificationService notifications)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (adapter == null) throw new ArgumentNullException("adapter");
            if (registry == null) throw new ArgumentNullException("registry");
            if (notifications == null) throw new ArgumentNullException("notifications");

            _context = context;
            _adapter = adapter;
            _registry = registry;
            _notifications = notifications;

            IsEnabled = true;
        }

        public void Start()
        {
            if (_disposed) return;
            _started = true;
        }

        public void FastTick()
        {
            if (IsBlockedOnline()) return;
            if (!IsEnabled || !_started || _disposed) return;
            if (!_context.IsPlayerReady || _context.IsInCutscene || _context.IsPaused || _context.IsLoading) return;

            if (SuperJumpEnabled)
            {
                _adapter.ApplySuperJumpFrame();
            }
        }

        public void SlowTick(int gameTimeMs)
        {
            if (IsBlockedOnline()) return;
            if (!IsEnabled || !_started || _disposed) return;
            if (!_context.IsPlayerReady || _context.IsInCutscene || _context.IsPaused || _context.IsLoading) return;

            if (_neverWantedEnabled)
            {
                _adapter.SetNeverWanted(true);
            }
        }

        public bool RefillHealthAndArmor()
        {
            if (IsBlockedOnline() || _disposed || !_context.IsPlayerReady) return false;
            _adapter.RefillHealthAndArmor();
            _notifications.Show("Cheat: Health & Armor refilled.");
            return true;
        }

        public bool ClearWanted()
        {
            if (IsBlockedOnline() || _disposed || !_context.IsPlayerReady) return false;
            _adapter.ClearWantedLevel();
            _notifications.Show("Cheat: Wanted level cleared.");
            return true;
        }

        public bool SetNeverWanted(bool enabled)
        {
            if (IsBlockedOnline() || _disposed) return false;
            _neverWantedEnabled = enabled;
            if (_context.IsPlayerReady)
            {
                _adapter.SetNeverWanted(enabled);
            }
            return true;
        }

        public bool RepairVehicle()
        {
            if (IsBlockedOnline() || _disposed || !_context.IsPlayerReady) return false;
            bool repaired = _adapter.RepairCurrentVehicle();
            if (repaired)
            {
                _notifications.Show("Cheat: Vehicle repaired.");
            }
            return repaired;
        }

        public bool TrySpawnClassicVehicle(string modelId)
        {
            if (IsBlockedOnline() || _disposed || !_context.IsPlayerReady) return false;
            if (string.IsNullOrWhiteSpace(modelId)) return false;

            string normalized = modelId.Trim().ToLowerInvariant();
            if (!AllowedVehicleModels.Contains(normalized))
            {
                return false;
            }

            int handle = _adapter.SpawnClassicVehicle(normalized);
            if (handle <= 0)
            {
                return false;
            }

            if (!_registry.TryTrackEntity(handle, ModuleId, EntityOwnership.Owned))
            {
                _registry.DeleteUntrackedEntity(handle);
                _notifications.Show("Entity budget full; cannot spawn classic vehicle.");
                return false;
            }

            _notifications.Show("Cheat: Spawned " + normalized + ".");
            return true;
        }

        public bool SetWeather(string weatherId)
        {
            if (IsBlockedOnline() || _disposed || !_context.IsPlayerReady) return false;
            if (string.IsNullOrWhiteSpace(weatherId)) return false;

            string normalized = weatherId.Trim().ToUpperInvariant();
            if (!AllowedWeatherIds.Contains(normalized))
            {
                return false;
            }

            _adapter.SetWeather(normalized);
            _notifications.Show("Cheat: Weather set to " + normalized + ".");
            return true;
        }

        public bool AdvanceTime(int hours)
        {
            if (IsBlockedOnline() || _disposed || !_context.IsPlayerReady) return false;
            if (hours <= 0 || hours > 24) return false;

            _adapter.AdvanceTimeHours(hours);
            _notifications.Show("Cheat: Advanced clock +" + hours + "h.");
            return true;
        }

        public void OnKeyPressed(GameKey key)
        {
        }

        public void Stop()
        {
            if (_neverWantedEnabled && !_context.IsOnlineSession && _context.IsPlayerReady)
            {
                _adapter.SetNeverWanted(false);
            }
            _neverWantedEnabled = false;
            SuperJumpEnabled = false;
            _started = false;
            if (!_context.IsOnlineSession)
            {
                _registry.CleanupOwner(ModuleId, true);
            }
            else
            {
                _registry.DropOwnerWithoutDeleting(ModuleId);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            Stop();
            _disposed = true;
        }

        private bool IsBlockedOnline()
        {
            if (_context.IsOnlineSession) return true;
            return false;
        }
    }
}
