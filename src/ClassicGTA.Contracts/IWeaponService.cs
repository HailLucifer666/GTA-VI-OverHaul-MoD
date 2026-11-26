using System.Collections.Generic;

namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Behavioral weapon and combat-control service implemented by engine adapters
    /// (future GTA VI Adapter or test fakes).
    /// </summary>
    public interface IWeaponService
    {
        uint UnarmedWeaponId { get; }
        uint CurrentWeaponId { get; }

        int GetPlayerInteriorId();
        bool HasWeapon(uint weaponId);
        bool SelectWeapon(uint weaponId);
        void EnableCombatControlsThisFrame();
        void SetDisablePlayerFiring(bool disable);
        void SetCanEquipAllWeapons(bool allow);
        IEnumerable<uint> GetAllValidWeaponIds();
        bool GiveWeaponWithMaxAmmo(uint weaponId);
    }
}
