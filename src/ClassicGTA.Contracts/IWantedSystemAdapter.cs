namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Engine-agnostic adapter for Classic Wanted & Heat Reset (replaces GTA VI Criminal Profile friction).
    /// </summary>
    public interface IWantedSystemAdapter
    {
        int GetWantedLevel();
        void SetWantedLevel(int level);
        bool ArePoliceSpottingPlayer();
        void ClearCriminalProfileHeat();
        void SetMaxWantedLevel(int maxLevel);
    }
}
