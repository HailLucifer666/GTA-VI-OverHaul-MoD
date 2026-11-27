namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Engine-agnostic adapter for San Andreas-style Gang Territory / Turf Wars.
    /// </summary>
    public interface ITerritoryWarAdapter
    {
        int SpawnWaveEnemy(string zoneId, int waveNumber);
        bool IsEntityAlive(int entityHandle);
        int CreateZoneBlip(string zoneId);
    }
}
