using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GTA;
using GTA.UI;

namespace ClassicGTA.GTA5
{
    /// <summary>
    /// GTA V Story Mode prototype.
    /// F10 toggles a broad weapon inventory. Only weapons added by this
    /// module during the current script lifetime are removed when toggled off.
    ///
    /// This class intentionally does NOT force weapon use in interiors or
    /// mission-restricted zones. Those restrictions require separate,
    /// verified, mission-safe behavior.
    /// </summary>
    public sealed class WeaponAndZoneUnlocker : Script
    {
        private const int AmmoPerWeapon = 9999;

        private readonly HashSet<WeaponHash> _addedByThisModule =
            new HashSet<WeaponHash>();

        private bool _arsenalEnabled;

        public WeaponAndZoneUnlocker()
        {
            Interval = 250;
            KeyDown += OnKeyDown;
            Aborted += OnAborted;

            Screen.ShowSubtitle(
                "Classic GTA: Pocket Arsenal loaded. Press F10 to toggle.",
                5000);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.F10)
                return;

            ToggleArsenal();
        }

        private void ToggleArsenal()
        {
            try
            {
                Ped player = Game.Player.Character;

                if (player == null || !player.Exists() || player.IsDead)
                {
                    Screen.ShowSubtitle(
                        "Pocket Arsenal unavailable: player is not ready.",
                        3000);
                    return;
                }

                if (!_arsenalEnabled)
                    EnableArsenal(player);
                else
                    DisableArsenal(player);
            }
            catch (Exception ex)
            {
                Screen.ShowSubtitle(
                    "Pocket Arsenal error. Check the SHVDN log.",
                    4000);
                // Avoid throwing from a game input callback.
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private void EnableArsenal(Ped player)
        {
            int addedCount = 0;

            foreach (WeaponHash weaponHash in Enum.GetValues(typeof(WeaponHash)))
            {
                if (weaponHash == WeaponHash.Unarmed)
                    continue;

                if (!player.Weapons.IsWeaponValid(weaponHash))
                    continue;

                if (player.Weapons.HasWeapon(weaponHash))
                    continue;

                player.Weapons.Give(
                    weaponHash,
                    AmmoPerWeapon,
                    false,
                    true);

                if (player.Weapons.HasWeapon(weaponHash))
                {
                    _addedByThisModule.Add(weaponHash);
                    addedCount++;
                }
            }

            _arsenalEnabled = true;

            Screen.ShowSubtitle(
                "Pocket Arsenal ON — added " + addedCount + " weapons.",
                4000);
        }

        private void DisableArsenal(Ped player)
        {
            int removedCount = 0;

            foreach (WeaponHash weaponHash in _addedByThisModule.ToArray())
            {
                if (player.Weapons.HasWeapon(weaponHash))
                {
                    player.Weapons.Remove(weaponHash);
                    removedCount++;
                }
            }

            _addedByThisModule.Clear();
            _arsenalEnabled = false;

            Screen.ShowSubtitle(
                "Pocket Arsenal OFF — removed " + removedCount +
                " module-added weapons.",
                4000);
        }

        private void OnAborted(object sender, EventArgs e)
        {
            // Do not remove weapons during abort/reload: the player may
            // have legitimately used or modified the inventory meanwhile.
            // The in-memory ownership set is discarded with this script.
            KeyDown -= OnKeyDown;
            Aborted -= OnAborted;
            _addedByThisModule.Clear();
        }
    }
}
