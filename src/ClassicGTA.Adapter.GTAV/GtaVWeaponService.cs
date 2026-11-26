using System;
using System.Collections.Generic;
using ClassicGTA.Contracts;
using GTA;
using GTA.Native;

namespace ClassicGTA.Adapter.GTAV
{
    internal sealed class GtaVWeaponService : IWeaponService
    {
        private const ulong HashGetInteriorFromEntity = 0x2107BA504071A6BBUL;
        private const ulong HashDisablePlayerFiring = 0x5E6CC07646BBEAB8UL;
        private const ulong HashSetCanPedEquipAllWeapons = 0xEFF296097FF1E509UL;

        public uint UnarmedWeaponId { get { return (uint)WeaponHash.Unarmed; } }

        public uint CurrentWeaponId
        {
            get
            {
                Ped ped = Game.Player.Character;
                if (ped == null || !ped.Exists()) return UnarmedWeaponId;
                Weapon cur = ped.Weapons.Current;
                return cur != null ? (uint)cur.Hash : UnarmedWeaponId;
            }
        }

        public int GetPlayerInteriorId()
        {
            Ped ped = Game.Player.Character;
            if (ped == null || !ped.Exists()) return 0;
            return Function.Call<int>((Hash)HashGetInteriorFromEntity, ped.Handle);
        }

        public bool HasWeapon(uint weaponId)
        {
            Ped ped = Game.Player.Character;
            return ped != null && ped.Exists() && ped.Weapons.HasWeapon((WeaponHash)weaponId);
        }

        public bool SelectWeapon(uint weaponId)
        {
            Ped ped = Game.Player.Character;
            return ped != null && ped.Exists() && ped.Weapons.Select((WeaponHash)weaponId, true);
        }

        public void EnableCombatControlsThisFrame()
        {
            Game.EnableControlThisFrame(GTA.Control.Attack);
            Game.EnableControlThisFrame(GTA.Control.Attack2);
            Game.EnableControlThisFrame(GTA.Control.Aim);
            Game.EnableControlThisFrame(GTA.Control.AccurateAim);
            Game.EnableControlThisFrame(GTA.Control.SelectWeapon);
            Game.EnableControlThisFrame(GTA.Control.SelectNextWeapon);
            Game.EnableControlThisFrame(GTA.Control.SelectPrevWeapon);
            Game.EnableControlThisFrame(GTA.Control.MeleeAttackLight);
            Game.EnableControlThisFrame(GTA.Control.MeleeAttackHeavy);
            Game.EnableControlThisFrame(GTA.Control.Reload);
        }

        public void SetDisablePlayerFiring(bool disable)
        {
            Function.Call((Hash)HashDisablePlayerFiring, Game.Player, disable);
        }

        public void SetCanEquipAllWeapons(bool allow)
        {
            Ped ped = Game.Player.Character;
            if (ped != null && ped.Exists())
            {
                Function.Call((Hash)HashSetCanPedEquipAllWeapons, ped.Handle, allow);
            }
        }

        public IEnumerable<uint> GetAllValidWeaponIds()
        {
            Ped ped = Game.Player.Character;
            if (ped == null || !ped.Exists()) yield break;

            foreach (WeaponHash hash in Enum.GetValues(typeof(WeaponHash)))
            {
                if (hash == WeaponHash.Unarmed) continue;
                if (ped.Weapons.IsWeaponValid(hash))
                {
                    yield return (uint)hash;
                }
            }
        }

        public bool GiveWeaponWithMaxAmmo(uint weaponId)
        {
            Ped ped = Game.Player.Character;
            if (ped == null || !ped.Exists()) return false;

            WeaponHash hash = (WeaponHash)weaponId;
            Weapon w = ped.Weapons.Give(hash, 0, false, true);
            if (w == null || !w.IsPresent) return false;

            if (w.MaxAmmo > 0)
            {
                w.Ammo = w.MaxAmmo;
            }
            return true;
        }
    }
}
