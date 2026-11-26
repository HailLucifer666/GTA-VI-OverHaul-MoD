using System;
using ClassicGTA.Contracts;
using ClassicGTA.Core.Economy;

namespace ClassicGTA.Adapter.GTAV
{
    public interface ISubmersibleAdapter
    {
        bool IsSuitableWaterLocation();
        int SpawnKrakenNearPlayer();
    }

    /// <summary>
    /// Enforces Kraken Submersible delivery invariants (I-05):
    /// - Blocks in Online or unready state.
    /// - Requires ownership of "sub.kraken".
    /// - Rejects dry-land / unsuitable locations via ISubmersibleAdapter.IsSuitableWaterLocation().
    /// - Caps at 1 active mod-owned Kraken (replaces previous owned Kraken cleanly).
    /// - Tracks via IEntityRegistry (respects global 20-entity budget; deletes untracked spawn on refusal).
    /// - Deterministic cleanup on Stop/Abort and safe non-deleting drop on Online transition.
    /// </summary>
    public sealed class KrakenDeliveryController
    {
        public const string OwnerId = "classic.vehicle.kraken";
        public const string KrakenAssetId = "sub.kraken";

        private readonly IGameContext _context;
        private readonly AssetOwnershipEngine _assets;
        private readonly ISubmersibleAdapter _adapter;
        private readonly IEntityRegistry _registry;
        private readonly INotificationService _notifications;

        private int _activeSubHandle;

        public int ActiveSubHandle { get { return _activeSubHandle; } }

        public KrakenDeliveryController(
            IGameContext context,
            AssetOwnershipEngine assets,
            ISubmersibleAdapter adapter,
            IEntityRegistry registry,
            INotificationService notifications)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (assets == null) throw new ArgumentNullException("assets");
            if (adapter == null) throw new ArgumentNullException("adapter");
            if (registry == null) throw new ArgumentNullException("registry");
            if (notifications == null) throw new ArgumentNullException("notifications");

            _context = context;
            _assets = assets;
            _adapter = adapter;
            _registry = registry;
            _notifications = notifications;
        }

        public bool SummonOwnedSubmersible()
        {
            if (_context.IsOnlineSession || !_context.IsPlayerReady)
            {
                return false;
            }

            if (!_assets.IsOwned(KrakenAssetId))
            {
                _notifications.Show("Buy the Kraken Submersible first ($250,000).");
                return false;
            }

            if (!_adapter.IsSuitableWaterLocation())
            {
                _notifications.Show("Move to open water or a dock to deliver the Kraken Submersible.");
                return false;
            }

            // Cap = 1: cleanly remove any previously delivered mod-owned Kraken before spawning a replacement.
            if (_activeSubHandle > 0)
            {
                _registry.CleanupOwner(OwnerId, false);
                _activeSubHandle = 0;
            }

            int handle = _adapter.SpawnKrakenNearPlayer();
            if (handle <= 0)
            {
                _notifications.Show("Could not deliver Kraken here — move to an open water area.");
                return false;
            }

            if (!_registry.TryTrackEntity(handle, OwnerId, EntityOwnership.Owned))
            {
                _registry.DeleteUntrackedEntity(handle);
                _notifications.Log("[Kraken] Entity budget exceeded; aborted delivery.");
                _notifications.Show("Entity budget full — dismiss active units before summoning Kraken.");
                return false;
            }

            _activeSubHandle = handle;
            _notifications.Show("Kraken Submersible delivered!");
            _notifications.Log("[Kraken] DELIVERED handle=" + handle);
            return true;
        }

        public void Cleanup(bool isOnline)
        {
            if (_activeSubHandle <= 0) return;

            if (isOnline)
            {
                _registry.DropOwnerWithoutDeleting(OwnerId);
            }
            else
            {
                _registry.CleanupOwner(OwnerId, false);
            }
            _activeSubHandle = 0;
        }
    }
}
