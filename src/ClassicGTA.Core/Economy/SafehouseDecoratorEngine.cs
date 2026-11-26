using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ClassicGTA.Contracts;

namespace ClassicGTA.Core.Economy
{
    public sealed class PlacedFurniture
    {
        public string PlacementId { get; private set; }
        public string SafehouseId { get; private set; }
        public string PropId { get; private set; }
        public WorldPoint Position { get; private set; }
        public float HeadingDegrees { get; private set; }

        public PlacedFurniture(string placementId, string safehouseId, string propId, WorldPoint position, float headingDegrees)
        {
            PlacementId = placementId;
            SafehouseId = safehouseId;
            PropId = propId;
            Position = position;
            HeadingDegrees = headingDegrees;
        }
    }

    /// <summary>
    /// Home Decorating &amp; Furnishing Engine (Cut Feature #7).
    /// Enforces safehouse ownership, interior radius bounds, per-safehouse prop cap,
    /// refund-on-persist-failure, and InvariantCulture atomic transform persistence.
    /// </summary>
    public sealed class SafehouseDecoratorEngine
    {
        private sealed class SafehouseBounds
        {
            public WorldPoint Center;
            public float RadiusMeters;
        }

        private const string SaveFileName = "ClassicGTA_Furniture.dat";
        private const string SchemaHeader = "FURNITURE_SCHEMA=1";

        private readonly IGameContext _context;
        private readonly IEconomyService _economy;
        private readonly IPersistenceStore _store;
        private readonly Func<string, bool> _isSafehouseOwned;
        private readonly int _maxPropsPerSafehouse;

        private readonly Dictionary<string, int> _propPrices =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, SafehouseBounds> _bounds =
            new Dictionary<string, SafehouseBounds>(StringComparer.OrdinalIgnoreCase);

        private readonly List<PlacedFurniture> _placed = new List<PlacedFurniture>();

        public int TotalPlacedCount { get { return _placed.Count; } }

        public SafehouseDecoratorEngine(
            IGameContext context,
            IEconomyService economy,
            IPersistenceStore store,
            Func<string, bool> isSafehouseOwned,
            int maxPropsPerSafehouse = 25)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (economy == null) throw new ArgumentNullException("economy");
            if (store == null) throw new ArgumentNullException("store");
            if (isSafehouseOwned == null) throw new ArgumentNullException("isSafehouseOwned");
            if (maxPropsPerSafehouse < 1) throw new ArgumentOutOfRangeException("maxPropsPerSafehouse");

            _context = context;
            _economy = economy;
            _store = store;
            _isSafehouseOwned = isSafehouseOwned;
            _maxPropsPerSafehouse = maxPropsPerSafehouse;
        }

        public void RegisterProp(string propId, int price)
        {
            if (string.IsNullOrWhiteSpace(propId)) throw new ArgumentException("PropId required.", "propId");
            if (price < 0) throw new ArgumentOutOfRangeException("price");
            _propPrices[propId.Trim()] = price;
        }

        public void RegisterSafehouseBounds(string safehouseId, WorldPoint center, float radiusMeters)
        {
            if (string.IsNullOrWhiteSpace(safehouseId)) throw new ArgumentException("SafehouseId required.", "safehouseId");
            if (radiusMeters <= 0f) throw new ArgumentOutOfRangeException("radiusMeters");
            _bounds[safehouseId.Trim()] = new SafehouseBounds { Center = center, RadiusMeters = radiusMeters };
        }

        public PurchaseResult TryPlaceFurniture(
            string safehouseId,
            string propId,
            WorldPoint position,
            float headingDegrees,
            out string placementId)
        {
            placementId = null;

            if (_context.IsOnlineSession)
            {
                return new PurchaseResult(PurchaseStatus.BlockedInOnline, "Blocked in Online session.");
            }

            SafehouseBounds area;
            if (string.IsNullOrWhiteSpace(safehouseId) || !_bounds.TryGetValue(safehouseId.Trim(), out area))
            {
                return new PurchaseResult(PurchaseStatus.UnknownAsset, "Unknown safehouse.");
            }

            if (!_isSafehouseOwned(safehouseId.Trim()))
            {
                return new PurchaseResult(PurchaseStatus.InvalidQuantity, "You do not own this safehouse.");
            }

            if (position.DistanceTo(area.Center) > area.RadiusMeters)
            {
                return new PurchaseResult(PurchaseStatus.InvalidQuantity, "Placement is outside safehouse bounds.");
            }

            int price;
            if (string.IsNullOrWhiteSpace(propId) || !_propPrices.TryGetValue(propId.Trim(), out price))
            {
                return new PurchaseResult(PurchaseStatus.UnknownAsset, "Unknown furniture prop.");
            }

            if (CountForSafehouse(safehouseId.Trim()) >= _maxPropsPerSafehouse)
            {
                return new PurchaseResult(PurchaseStatus.InvalidQuantity, "Safehouse furniture limit reached.");
            }

            if (_economy.GetCash() < price || !_economy.TrySpend(price, "Furniture:" + propId))
            {
                return new PurchaseResult(PurchaseStatus.InsufficientFunds, "Insufficient cash.");
            }

            string newId = Guid.NewGuid().ToString("N");
            var item = new PlacedFurniture(newId, safehouseId.Trim(), propId.Trim(), position, headingDegrees);
            _placed.Add(item);

            try
            {
                PersistState();
            }
            catch (Exception)
            {
                _placed.Remove(item);
                _economy.Credit(price, "RefundFurniture:" + propId);
                return new PurchaseResult(PurchaseStatus.PersistenceFailedAndRefunded, "Save failed; cash refunded.");
            }

            placementId = newId;
            return new PurchaseResult(PurchaseStatus.Succeeded, "Placed " + propId + ".");
        }

        public bool RemoveFurniture(string placementId)
        {
            if (string.IsNullOrWhiteSpace(placementId)) return false;
            int idx = _placed.FindIndex(p => string.Equals(p.PlacementId, placementId, StringComparison.Ordinal));
            if (idx < 0) return false;

            PlacedFurniture removed = _placed[idx];
            _placed.RemoveAt(idx);
            try
            {
                PersistState();
                return true;
            }
            catch
            {
                _placed.Insert(idx, removed);
                return false;
            }
        }

        public List<PlacedFurniture> GetPlacedForSafehouse(string safehouseId)
        {
            var result = new List<PlacedFurniture>();
            for (int i = 0; i < _placed.Count; i++)
            {
                if (string.Equals(_placed[i].SafehouseId, safehouseId, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(_placed[i]);
                }
            }
            return result;
        }

        public bool LoadState()
        {
            string raw;
            if (!_store.TryLoad(SaveFileName, s => !string.IsNullOrWhiteSpace(s) && s.StartsWith(SchemaHeader, StringComparison.Ordinal), out raw))
            {
                return false;
            }

            _placed.Clear();
            string[] lines = raw.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] parts = lines[i].Trim().Split('|');
                if (parts.Length != 7) continue;

                float x, y, z, h;
                if (float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out x) &&
                    float.TryParse(parts[4], NumberStyles.Float, CultureInfo.InvariantCulture, out y) &&
                    float.TryParse(parts[5], NumberStyles.Float, CultureInfo.InvariantCulture, out z) &&
                    float.TryParse(parts[6], NumberStyles.Float, CultureInfo.InvariantCulture, out h))
                {
                    _placed.Add(new PlacedFurniture(parts[0], parts[1], parts[2], new WorldPoint(x, y, z), h));
                }
            }
            return true;
        }

        private int CountForSafehouse(string safehouseId)
        {
            int count = 0;
            for (int i = 0; i < _placed.Count; i++)
            {
                if (string.Equals(_placed[i].SafehouseId, safehouseId, StringComparison.OrdinalIgnoreCase)) count++;
            }
            return count;
        }

        private void PersistState()
        {
            var sb = new StringBuilder();
            sb.AppendLine(SchemaHeader);
            for (int i = 0; i < _placed.Count; i++)
            {
                PlacedFurniture p = _placed[i];
                sb.Append(p.PlacementId).Append('|')
                  .Append(p.SafehouseId).Append('|')
                  .Append(p.PropId).Append('|')
                  .Append(p.Position.X.ToString("R", CultureInfo.InvariantCulture)).Append('|')
                  .Append(p.Position.Y.ToString("R", CultureInfo.InvariantCulture)).Append('|')
                  .Append(p.Position.Z.ToString("R", CultureInfo.InvariantCulture)).Append('|')
                  .AppendLine(p.HeadingDegrees.ToString("R", CultureInfo.InvariantCulture));
            }
            _store.Save(SaveFileName, sb.ToString());
        }
    }
}
