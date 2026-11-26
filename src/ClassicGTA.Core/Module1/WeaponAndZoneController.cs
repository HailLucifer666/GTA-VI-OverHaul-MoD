using System;
using System.Collections.Generic;
using ClassicGTA.Contracts;

namespace ClassicGTA.Core.Module1
{
    /// <summary>
    /// Module 1 Core Logic: Safe-Zone/Interior Weapon Restoration + Non-Destructive Pocket Arsenal.
    /// Zero dependency on GTA V or GTA VI runtime DLLs; 100% unit-testable offline.
    /// </summary>
    public sealed class WeaponAndZoneController : IModModule
    {
        private readonly IGameContext _context;
        private readonly IWeaponService _weapons;
        private readonly INotificationService _notifications;
        private readonly WeaponAndZoneConfig _config;

        private bool _isRunning;
        private bool _isEnabled;
        private bool _zoneRestoreEnabled;
        private int _lastSlowTickMs;
        private bool _hasSlowTicked;
        private int _trackedPlayerHandle;
        private uint _lastEquippedWeaponId;
        private bool _wasInInterior;

        public string ModuleId { get { return "classic.module1.weapons"; } }
        public string DisplayName { get { return "Weapon & Safe-Zone Restoration"; } }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }

        public bool ZoneRestoreEnabled
        {
            get { return _zoneRestoreEnabled; }
            set { _zoneRestoreEnabled = value; }
        }

        public WeaponAndZoneController(
            IGameContext context,
            IWeaponService weapons,
            INotificationService notifications,
            WeaponAndZoneConfig config)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (weapons == null) throw new ArgumentNullException("weapons");
            if (notifications == null) throw new ArgumentNullException("notifications");
            if (config == null) throw new ArgumentNullException("config");

            config.Validate();

            _context = context;
            _weapons = weapons;
            _notifications = notifications;
            _config = config;

            _isEnabled = config.Enabled;
            _zoneRestoreEnabled = config.ZoneRestoreEnabled;
            _lastEquippedWeaponId = weapons.UnarmedWeaponId;
        }

        public void Start()
        {
            _isRunning = true;
            _hasSlowTicked = false;
            _trackedPlayerHandle = 0;
            _lastEquippedWeaponId = _weapons.UnarmedWeaponId;
            _wasInInterior = false;
            _notifications.Log("[Module1] Started.");
        }

        public void FastTick()
        {
            if (!_isRunning || !_isEnabled || !_zoneRestoreEnabled)
            {
                return;
            }

            if (!IsSafeGameplayState())
            {
                return;
            }

            _weapons.SetDisablePlayerFiring(false);
            _weapons.EnableCombatControlsThisFrame();
        }

        public void SlowTick(int gameTimeMs)
        {
            if (!_isRunning || !_isEnabled)
            {
                return;
            }

            if (!ShouldRunSlowTick(gameTimeMs))
            {
                return;
            }

            _lastSlowTickMs = gameTimeMs;
            _hasSlowTicked = true;

            if (!IsSafeGameplayState())
            {
                return;
            }

            SyncTrackedProtagonist();

            if (!_zoneRestoreEnabled || _context.IsInVehicle)
            {
                return;
            }

            ProcessInteriorRestoration(gameTimeMs);
        }

        public void OnKeyPressed(GameKey key)
        {
            if (!_isRunning || !_isEnabled || key == GameKey.None)
            {
                return;
            }

            if (key == _config.ToggleKey)
            {
                _zoneRestoreEnabled = !_zoneRestoreEnabled;
                string state = _zoneRestoreEnabled ? "ON" : "OFF";
                _notifications.Show("Classic GTA: Zone Weapon Restore " + state + ".");
                _notifications.Log("[Module1] ZoneRestore toggled " + state + ".");
                return;
            }

            if (key == _config.PocketArsenalKey)
            {
                GrantPocketArsenal();
            }
        }

        public int GrantPocketArsenal()
        {
            if (!_isRunning || !_isEnabled || !IsSafeGameplayState())
            {
                return 0;
            }

            uint previousWeapon = _weapons.CurrentWeaponId;
            _weapons.SetCanEquipAllWeapons(true);

            HashSet<uint> seen = new HashSet<uint>();
            int grantedCount = 0;

            foreach (uint weaponId in _weapons.GetAllValidWeaponIds())
            {
                if (weaponId == _weapons.UnarmedWeaponId || !seen.Add(weaponId))
                {
                    continue;
                }

                if (_weapons.GiveWeaponWithMaxAmmo(weaponId))
                {
                    grantedCount++;
                }
            }

            if (previousWeapon != _weapons.UnarmedWeaponId && _weapons.HasWeapon(previousWeapon))
            {
                _weapons.SelectWeapon(previousWeapon);
            }

            _notifications.Show("Classic GTA: Pocket Arsenal granted (" + grantedCount + " weapons).");
            _notifications.Log("[Module1] POCKET_ARSENAL granted=" + grantedCount);
            return grantedCount;
        }

        public void Stop()
        {
            _isRunning = false;
            // Never remove player weapons on Stop/Dispose; player may have earned or modified them.
            _notifications.Log("[Module1] Stopped.");
        }

        public void Dispose()
        {
            Stop();
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

        private bool ShouldRunSlowTick(int gameTimeMs)
        {
            if (!_hasSlowTicked)
            {
                return true;
            }

            // Guard against GameTime reset on save load or wrap-around.
            if (gameTimeMs < _lastSlowTickMs)
            {
                return true;
            }

            return (gameTimeMs - _lastSlowTickMs) >= _config.RestoreIntervalMs;
        }

        private void SyncTrackedProtagonist()
        {
            int currentHandle = _context.PlayerHandle;
            if (currentHandle != _trackedPlayerHandle)
            {
                _trackedPlayerHandle = currentHandle;
                _lastEquippedWeaponId = _weapons.UnarmedWeaponId;
                _wasInInterior = false;
            }
        }

        private void ProcessInteriorRestoration(int gameTimeMs)
        {
            _weapons.SetCanEquipAllWeapons(true);

            int interiorId = _weapons.GetPlayerInteriorId();
            bool inInterior = interiorId != 0;
            uint current = _weapons.CurrentWeaponId;

            if (current != _weapons.UnarmedWeaponId)
            {
                _lastEquippedWeaponId = current;
            }
            else if (inInterior && !_wasInInterior && _lastEquippedWeaponId != _weapons.UnarmedWeaponId)
            {
                if (_weapons.HasWeapon(_lastEquippedWeaponId))
                {
                    bool selected = _weapons.SelectWeapon(_lastEquippedWeaponId);
                    _notifications.Log(
                        "[RESTORE] t=" + gameTimeMs +
                        " ped=" + _trackedPlayerHandle +
                        " interior=" + interiorId +
                        " weapon=" + _lastEquippedWeaponId +
                        " ok=" + selected);
                }
            }

            _wasInInterior = inInterior;
        }
    }
}
