using System;
using System.Collections.Generic;
using ClassicGTA.Contracts;

namespace ClassicGTA.Infrastructure
{
    /// <summary>
    /// Tracks Entities (Owned/Borrowed/Detached), Blips (separate from entities),
    /// and Scripted Cameras (restoring rendering camera before deletion).
    /// Prevents entity leaks and accidental deletion of player-owned vehicles.
    /// </summary>
    public sealed class EntityOwnershipRegistry : IEntityRegistry
    {
        private sealed class EntityRecord
        {
            public int Handle;
            public string OwnerModuleId;
            public EntityOwnership Ownership;
        }

        private readonly IEntityCleanupAdapter _adapter;
        private readonly int _maxOwnedEntities;
        private readonly Dictionary<int, EntityRecord> _entities = new Dictionary<int, EntityRecord>();
        private readonly Dictionary<int, string> _blips = new Dictionary<int, string>();
        private readonly Dictionary<int, string> _cameras = new Dictionary<int, string>();

        public int TrackedEntityCount { get { return _entities.Count; } }
        public int TrackedBlipCount { get { return _blips.Count; } }
        public int TrackedCameraCount { get { return _cameras.Count; } }

        public int OwnedEntityCount
        {
            get
            {
                int count = 0;
                foreach (var kv in _entities)
                {
                    if (kv.Value.Ownership == EntityOwnership.Owned) count++;
                }
                return count;
            }
        }

        public EntityOwnershipRegistry(IEntityCleanupAdapter adapter, int maxOwnedEntities = 20)
        {
            if (adapter == null) throw new ArgumentNullException("adapter");
            if (maxOwnedEntities < 1) throw new ArgumentOutOfRangeException("maxOwnedEntities");

            _adapter = adapter;
            _maxOwnedEntities = maxOwnedEntities;
        }

        public bool TryTrackEntity(int handle, string ownerModuleId, EntityOwnership ownership)
        {
            if (handle <= 0) return false;
            if (string.IsNullOrWhiteSpace(ownerModuleId)) return false;

            EntityRecord existing;
            if (_entities.TryGetValue(handle, out existing))
            {
                // Refuse to demote/overwrite existing ownership class (prevents Owned->Borrowed or Borrowed->Owned bugs).
                return existing.Ownership == ownership &&
                       string.Equals(existing.OwnerModuleId, ownerModuleId, StringComparison.OrdinalIgnoreCase);
            }

            if (ownership == EntityOwnership.Owned && OwnedEntityCount >= _maxOwnedEntities)
            {
                return false;
            }

            _entities[handle] = new EntityRecord
            {
                Handle = handle,
                OwnerModuleId = ownerModuleId,
                Ownership = ownership
            };
            return true;
        }

        public void DropAllWithoutDeleting()
        {
            // Used on SP->Online transition: drop stale pool handles without ever deleting recycled Online entities.
            _entities.Clear();
            _blips.Clear();
            _cameras.Clear();
        }

        public void DeleteUntrackedEntity(int handle)
        {
            if (handle > 0) _adapter.DeleteEntity(handle);
        }

        public void DeleteUntrackedBlip(int blipHandle)
        {
            if (blipHandle > 0) _adapter.DeleteBlip(blipHandle);
        }

        public void TrackBlip(int blipHandle, string ownerModuleId)
        {
            if (blipHandle <= 0) throw new ArgumentOutOfRangeException("blipHandle");
            if (string.IsNullOrWhiteSpace(ownerModuleId)) throw new ArgumentException("Owner required.", "ownerModuleId");
            _blips[blipHandle] = ownerModuleId;
        }

        public void TrackCamera(int cameraHandle, string ownerModuleId)
        {
            if (cameraHandle <= 0) throw new ArgumentOutOfRangeException("cameraHandle");
            if (string.IsNullOrWhiteSpace(ownerModuleId)) throw new ArgumentException("Owner required.", "ownerModuleId");
            _cameras[cameraHandle] = ownerModuleId;
        }

        public void CleanupOwner(string ownerModuleId, bool releaseOwnedInsteadOfDelete = false)
        {
            if (string.IsNullOrWhiteSpace(ownerModuleId)) return;

            CleanupCameras(ownerModuleId);
            CleanupBlips(ownerModuleId);
            CleanupEntities(ownerModuleId, releaseOwnedInsteadOfDelete);
        }

        public void CleanupAll(bool releaseOwnedInsteadOfDelete = false)
        {
            CleanupCameras(null);
            CleanupBlips(null);
            CleanupEntities(null, releaseOwnedInsteadOfDelete);
        }

        private void CleanupCameras(string ownerFilter)
        {
            var toRemove = new List<int>();
            foreach (var kv in _cameras)
            {
                if (ownerFilter == null || string.Equals(kv.Value, ownerFilter, StringComparison.OrdinalIgnoreCase))
                {
                    toRemove.Add(kv.Key);
                }
            }

            if (toRemove.Count > 0)
            {
                // CRITICAL: Always restore game rendering camera BEFORE deleting scripted cameras.
                _adapter.RestoreRenderingCamera();
                for (int i = 0; i < toRemove.Count; i++)
                {
                    _adapter.DeleteCamera(toRemove[i]);
                    _cameras.Remove(toRemove[i]);
                }
            }
        }

        private void CleanupBlips(string ownerFilter)
        {
            var toRemove = new List<int>();
            foreach (var kv in _blips)
            {
                if (ownerFilter == null || string.Equals(kv.Value, ownerFilter, StringComparison.OrdinalIgnoreCase))
                {
                    toRemove.Add(kv.Key);
                }
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                _adapter.DeleteBlip(toRemove[i]);
                _blips.Remove(toRemove[i]);
            }
        }

        private void CleanupEntities(string ownerFilter, bool releaseOwnedInsteadOfDelete)
        {
            var toRemove = new List<EntityRecord>();
            foreach (var kv in _entities)
            {
                if (ownerFilter == null || string.Equals(kv.Value.OwnerModuleId, ownerFilter, StringComparison.OrdinalIgnoreCase))
                {
                    toRemove.Add(kv.Value);
                }
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                EntityRecord rec = toRemove[i];
                if (rec.Ownership == EntityOwnership.Owned)
                {
                    if (releaseOwnedInsteadOfDelete)
                    {
                        _adapter.MarkEntityNoLongerNeeded(rec.Handle);
                    }
                    else
                    {
                        _adapter.DeleteEntity(rec.Handle);
                    }
                }
                else if (rec.Ownership == EntityOwnership.Detached)
                {
                    _adapter.MarkEntityNoLongerNeeded(rec.Handle);
                }
                // EntityOwnership.Borrowed: NEVER delete or release player/world entities.

                _entities.Remove(rec.Handle);
            }
        }
    }
}
