namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Engine-agnostic adapter for Classic Instant Car Theft (bypasses GTA VI lockpick/alarm/QTE friction).
    /// </summary>
    public interface ICarTheftAdapter
    {
        int GetTargetVehicleHandle();
        bool IsVehicleLocked(int vehicleHandle);
        bool UnlockVehicle(int vehicleHandle);
        void DisableAlarm(int vehicleHandle);
        void PacifyDriver(int vehicleHandle);
    }
}
