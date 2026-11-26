namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Engine-agnostic cleanup adapter for entities, blips, and scripted cameras.
    /// </summary>
    public interface IEntityCleanupAdapter
    {
        void DeleteEntity(int entityHandle);
        void MarkEntityNoLongerNeeded(int entityHandle);
        void DeleteBlip(int blipHandle);
        void RestoreRenderingCamera();
        void DeleteCamera(int cameraHandle);
    }
}
