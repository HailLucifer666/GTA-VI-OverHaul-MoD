namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Behavioral adapter for Classic Taxi side-hustle missions (#8).
    /// </summary>
    public interface ITaxiAdapter
    {
        bool IsPlayerInTaxi(out int vehicleHandle);
        WorldPoint GetPlayerPosition();
        bool TrySpawnFare(
            WorldPoint nearPlayer,
            out int farePedHandle,
            out WorldPoint pickupPosition,
            out WorldPoint dropoffPosition,
            out int blipHandle);
        bool IsFareInVehicle(int farePedHandle, int vehicleHandle);
        bool IsEntityAlive(int handle);
        void TaskFareEnterVehicle(int farePedHandle, int vehicleHandle);
        void TaskFareLeaveVehicle(int farePedHandle, int vehicleHandle);
        void SetRouteBlip(int blipHandle, WorldPoint destination);
    }
}
