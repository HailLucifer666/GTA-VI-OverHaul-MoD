using System;
using ClassicGTA.Contracts;

namespace ClassicGTA.Core.Module1
{
    /// <summary>
    /// Validated configuration for Module 1 (Weapon &amp; Safe-Zone Restoration + Pocket Arsenal).
    /// </summary>
    public sealed class WeaponAndZoneConfig
    {
        public bool Enabled { get; set; }
        public bool ZoneRestoreEnabled { get; set; }
        public int RestoreIntervalMs { get; set; }
        public GameKey ToggleKey { get; set; }
        public GameKey PocketArsenalKey { get; set; }

        public WeaponAndZoneConfig()
        {
            Enabled = true;
            ZoneRestoreEnabled = true;
            RestoreIntervalMs = 200;
            ToggleKey = GameKey.F8;
            PocketArsenalKey = GameKey.F10;
        }

        public void Validate()
        {
            if (RestoreIntervalMs < 50 || RestoreIntervalMs > 5000)
            {
                throw new ArgumentOutOfRangeException(
                    "RestoreIntervalMs",
                    "RestoreIntervalMs must be between 50 and 5000 ms.");
            }
        }
    }
}
