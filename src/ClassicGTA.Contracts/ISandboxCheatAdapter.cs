namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Engine-agnostic adapter for Classic Sandbox Cheat Hub (Story Mode only, unlocked from start).
    /// </summary>
    public interface ISandboxCheatAdapter
    {
        void RefillHealthAndArmor();
        void ClearWantedLevel();
        void SetNeverWanted(bool enabled);
        void ApplySuperJumpFrame();
        bool RepairCurrentVehicle();
        int SpawnClassicVehicle(string modelId);
        void SetWeather(string weatherId);
        void AdvanceTimeHours(int hours);
    }
}
