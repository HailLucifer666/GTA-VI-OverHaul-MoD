namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Ownership registry contract injected into Core modules for leak-free entity/blip/camera cleanup.
    /// </summary>
    public interface IEntityRegistry
    {
        bool TryTrackEntity(int handle, string ownerModuleId, EntityOwnership ownership);
        void TrackBlip(int blipHandle, string ownerModuleId);
        void TrackCamera(int cameraHandle, string ownerModuleId);
        void CleanupOwner(string ownerModuleId, bool releaseOwnedInsteadOfDelete = false);
        void DropOwnerWithoutDeleting(string ownerModuleId);
        void DropAllWithoutDeleting();
        void LatchOnlineSession();
        void DeleteUntrackedEntity(int handle);
        void DeleteUntrackedBlip(int blipHandle);
    }
}
