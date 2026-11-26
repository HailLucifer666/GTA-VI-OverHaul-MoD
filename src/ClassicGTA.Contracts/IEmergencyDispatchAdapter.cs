namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Behavioral 911 dispatch adapter implemented by engine adapters (future GTA VI Adapter) or test fakes.
    /// </summary>
    public interface IEmergencyDispatchAdapter
    {
        WorldPoint GetPlayerPosition();
        bool TryFindRoadSpawnPoint(WorldPoint target, float minDistance, float maxDistance, out WorldPoint spawnPoint);
        DispatchSpawnResult SpawnUnit(EmergencyServiceType service, WorldPoint spawnPoint);
        void TaskDriveTo(int responderPedHandle, int vehicleHandle, WorldPoint destination);
        bool TryGetEntityPosition(int handle, out WorldPoint position);
        bool IsEntityAlive(int handle);
        void HealPlayerToFull();
        void TaskEngageNearbyHostiles(int responderPedHandle);
        bool TryExtinguishNearbyFires(WorldPoint center, float radius);
        void TaskDepart(int responderPedHandle, int vehicleHandle);
    }
}
