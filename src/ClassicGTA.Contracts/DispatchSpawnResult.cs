namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Explicit multi-way result for emergency unit spawning (never collapses errors to bare bool).
    /// </summary>
    public sealed class DispatchSpawnResult
    {
        public bool Succeeded { get; private set; }
        public int VehicleHandle { get; private set; }
        public int ResponderPedHandle { get; private set; }
        public int BlipHandle { get; private set; }
        public string FailureReason { get; private set; }

        public static DispatchSpawnResult Success(int vehicleHandle, int responderPedHandle, int blipHandle)
        {
            return new DispatchSpawnResult
            {
                Succeeded = true,
                VehicleHandle = vehicleHandle,
                ResponderPedHandle = responderPedHandle,
                BlipHandle = blipHandle,
                FailureReason = null
            };
        }

        public static DispatchSpawnResult Fail(string reason)
        {
            return new DispatchSpawnResult
            {
                Succeeded = false,
                FailureReason = string.IsNullOrWhiteSpace(reason) ? "Unknown spawn failure" : reason
            };
        }
    }
}
