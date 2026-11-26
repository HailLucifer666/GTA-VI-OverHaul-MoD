namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Behavioral adapter for Free-Roam First-Person Camera (#18).
    /// </summary>
    public interface IFirstPersonCameraAdapter
    {
        int CreateAttachedCamera(bool inVehicle);
        void ActivateCamera(int cameraHandle);
        void ReadLookDelta(out float deltaYawDegrees, out float deltaPitchDegrees);
        void UpdateCameraLook(int cameraHandle, float pitchDegrees, float yawDegrees, bool inVehicle);
        void SyncPlayerHeading(float yawDegrees);
    }
}
