using System.Collections.Generic;
using ClassicGTA.Contracts;

namespace ClassicGTA.Tests
{
    internal sealed class FakeGameContext : IGameContext
    {
        public bool IsPlayerReady { get; set; }
        public int PlayerHandle { get; set; }
        public bool IsInCutscene { get; set; }
        public bool IsPaused { get; set; }
        public bool IsLoading { get; set; }
        public bool IsOnlineSession { get; set; }
        public bool IsInVehicle { get; set; }
        public bool IsScreenFadedOut { get; set; }
        public int GameTimeMs { get; set; }

        public FakeGameContext()
        {
            IsPlayerReady = true;
            PlayerHandle = 1001;
            GameTimeMs = 1000;
        }
    }

    internal sealed class FakeWeaponService : IWeaponService
    {
        public const uint Unarmed = 0xA2719263U;
        public const uint Pistol = 0x1B06D571U;
        public const uint Carbine = 0x83BF0278U;
        public const uint Shotgun = 0x1D073A89U;

        public uint UnarmedWeaponId { get { return Unarmed; } }
        public uint CurrentWeaponId { get; set; }
        public int InteriorId { get; set; }
        public HashSet<uint> OwnedWeapons { get; private set; }
        public List<uint> CatalogWeapons { get; private set; }

        public int EnableCombatControlsCalls { get; private set; }
        public int DisableFiringFalseCalls { get; private set; }
        public int CanEquipAllCalls { get; private set; }
        public int CanEquipAllFalseCalls { get; private set; }
        public int SelectWeaponCalls { get; private set; }
        public int GiveWeaponCalls { get; private set; }

        public FakeWeaponService()
        {
            CurrentWeaponId = Unarmed;
            OwnedWeapons = new HashSet<uint>();
            CatalogWeapons = new List<uint> { Unarmed, Pistol, Pistol, Carbine, Shotgun };
        }

        public int GetPlayerInteriorId() { return InteriorId; }
        public bool HasWeapon(uint weaponId) { return OwnedWeapons.Contains(weaponId); }

        public bool SelectWeapon(uint weaponId)
        {
            SelectWeaponCalls++;
            if (!OwnedWeapons.Contains(weaponId)) return false;
            CurrentWeaponId = weaponId;
            return true;
        }

        public void EnableCombatControlsThisFrame() { EnableCombatControlsCalls++; }

        public void SetDisablePlayerFiring(bool disable)
        {
            if (!disable) DisableFiringFalseCalls++;
        }

        public void SetCanEquipAllWeapons(bool allow)
        {
            if (allow) CanEquipAllCalls++;
            else CanEquipAllFalseCalls++;
        }

        public IEnumerable<uint> GetAllValidWeaponIds() { return CatalogWeapons; }

        public bool GiveWeaponWithMaxAmmo(uint weaponId)
        {
            GiveWeaponCalls++;
            OwnedWeapons.Add(weaponId);
            return true;
        }
    }

    internal sealed class FakeNotificationService : INotificationService
    {
        public List<string> Notifications { get; private set; }
        public List<string> Logs { get; private set; }

        public FakeNotificationService()
        {
            Notifications = new List<string>();
            Logs = new List<string>();
        }

        public void Show(string message) { Notifications.Add(message); }
        public void Log(string message) { Logs.Add(message); }
    }
}
