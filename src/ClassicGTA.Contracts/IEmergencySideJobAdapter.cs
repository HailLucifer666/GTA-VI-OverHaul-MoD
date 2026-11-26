namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Behavioral adapter for Vigilante, Paramedic, and Firefighter side-jobs (#9).
    /// </summary>
    public interface IEmergencySideJobAdapter
    {
        bool IsPlayerInEmergencyVehicle(out EmergencyServiceType jobType, out int vehicleHandle);
        WorldPoint GetPlayerPosition();
        bool TrySpawnWaveTarget(
            EmergencyServiceType jobType,
            int level,
            out int targetHandle,
            out WorldPoint location,
            out int blipHandle);
        bool IsTargetResolved(EmergencyServiceType jobType, int targetHandle, WorldPoint playerPosition);
        bool IsEntityAlive(int handle);
    }
}
