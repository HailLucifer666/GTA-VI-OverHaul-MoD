using System;
using System.Collections.Generic;
using System.Text;
using ClassicGTA.Contracts;

namespace ClassicGTA.Core.Activities
{
    /// <summary>
    /// Purchasable &amp; Walkable Guard Dogs (Cut Feature #26).
    /// Supports breed catalog purchase (refund-safe), summoning owned pets,
    /// Follow/Sit/Fetch/Guard/AttackThreat commands, death handling, and atomic persistence.
    /// </summary>
    public sealed class PetCompanionController : IModModule
    {
        private const string SaveFileName = "ClassicGTA_Pets.dat";
        private const string SchemaHeader = "PETS_SCHEMA=1";

        private readonly IGameContext _context;
        private readonly IPetAdapter _adapter;
        private readonly IEconomyService _economy;
        private readonly IEntityRegistry _registry;
        private readonly IPersistenceStore _store;
        private readonly INotificationService _notifications;

        private readonly Dictionary<string, int> _breedPrices =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> _ownedBreeds =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private bool _isRunning;
        private bool _isEnabled = true;
        private int _activePetHandle;
        private string _activeBreedId;
        private PetCommand _currentCommand = PetCommand.Follow;

        public string ModuleId { get { return "classic.social.pets"; } }
        public string DisplayName { get { return "Purchasable Guard Dogs"; } }
        public bool HasActivePet { get { return _activePetHandle > 0; } }
        public string ActiveBreedId { get { return _activeBreedId; } }
        public PetCommand CurrentCommand { get { return _currentCommand; } }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }

        public PetCompanionController(
            IGameContext context,
            IPetAdapter adapter,
            IEconomyService economy,
            IEntityRegistry registry,
            IPersistenceStore store,
            INotificationService notifications)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (adapter == null) throw new ArgumentNullException("adapter");
            if (economy == null) throw new ArgumentNullException("economy");
            if (registry == null) throw new ArgumentNullException("registry");
            if (store == null) throw new ArgumentNullException("store");
            if (notifications == null) throw new ArgumentNullException("notifications");

            _context = context;
            _adapter = adapter;
            _economy = economy;
            _registry = registry;
            _store = store;
            _notifications = notifications;
        }

        public void RegisterBreed(string breedId, int price)
        {
            if (string.IsNullOrWhiteSpace(breedId)) throw new ArgumentException("BreedId required.", "breedId");
            if (price < 0) throw new ArgumentOutOfRangeException("price");
            _breedPrices[breedId.Trim()] = price;
        }

        public bool IsBreedOwned(string breedId)
        {
            return !string.IsNullOrWhiteSpace(breedId) && _ownedBreeds.Contains(breedId.Trim());
        }

        public void Start() { _isRunning = true; }
        public void FastTick() { }
        public void OnKeyPressed(GameKey key) { }

        public PurchaseResult TryBuyBreed(string breedId)
        {
            if (_context.IsOnlineSession)
            {
                return new PurchaseResult(PurchaseStatus.BlockedInOnline, "Blocked in Online session.");
            }

            int price;
            if (string.IsNullOrWhiteSpace(breedId) || !_breedPrices.TryGetValue(breedId.Trim(), out price))
            {
                return new PurchaseResult(PurchaseStatus.UnknownAsset, "Unknown pet breed: " + breedId);
            }

            string id = breedId.Trim();
            if (_ownedBreeds.Contains(id))
            {
                return new PurchaseResult(PurchaseStatus.AlreadyOwned, "Breed already owned.");
            }

            if (_economy.GetCash() < price || !_economy.TrySpend(price, "BuyPet:" + id))
            {
                return new PurchaseResult(PurchaseStatus.InsufficientFunds, "Insufficient cash.");
            }

            _ownedBreeds.Add(id);
            try
            {
                PersistState();
            }
            catch (Exception ex)
            {
                _ownedBreeds.Remove(id);
                _economy.Credit(price, "RefundPet:" + id);
                _notifications.Log("[Pets] Persist failed, refunded: " + ex.Message);
                return new PurchaseResult(PurchaseStatus.PersistenceFailedAndRefunded, "Save failed; cash refunded.");
            }

            _notifications.Show("Purchased guard dog: " + id + " ($" + price + ").");
            return new PurchaseResult(PurchaseStatus.Succeeded, "Purchased " + id + ".");
        }

        public bool SummonPet(string breedId)
        {
            if (!_isRunning || !_isEnabled || _context.IsOnlineSession || !_context.IsPlayerReady) return false;
            if (_activePetHandle > 0) return false;
            if (!IsBreedOwned(breedId)) return false;

            int petHandle; int blipHandle;
            if (!_adapter.TrySpawnPet(breedId.Trim(), _adapter.GetPlayerPosition(), out petHandle, out blipHandle))
            {
                return false;
            }

            if (!_registry.TryTrackEntity(petHandle, ModuleId, EntityOwnership.Owned))
            {
                _registry.CleanupOwner(ModuleId, false);
                return false;
            }

            if (blipHandle > 0)
            {
                _registry.TrackBlip(blipHandle, ModuleId);
            }

            _activePetHandle = petHandle;
            _activeBreedId = breedId.Trim();
            _currentCommand = PetCommand.Follow;
            _adapter.IssueCommand(_activePetHandle, PetCommand.Follow);
            return true;
        }

        public bool GiveCommand(PetCommand command)
        {
            if (!_isRunning || _activePetHandle <= 0 || _context.IsOnlineSession) return false;
            if (!_adapter.IsEntityAlive(_activePetHandle))
            {
                DismissPet("PetDied");
                return false;
            }

            _currentCommand = command;
            _adapter.IssueCommand(_activePetHandle, command);
            return true;
        }

        public void DismissPet(string reason = "Kennelled")
        {
            if (_activePetHandle <= 0) return;
            _registry.CleanupOwner(ModuleId, false);
            _activePetHandle = 0;
            _activeBreedId = null;
            _notifications.Log("[Pets] DISMISSED reason=" + reason);
        }

        public void SlowTick(int gameTimeMs)
        {
            if (!_isRunning || !_isEnabled || _activePetHandle <= 0) return;
            if (_context.IsOnlineSession)
            {
                // S-06 Fix: Drop handles without calling DeleteEntity across SP->Online transition.
                _registry.DropAllWithoutDeleting();
                _activePetHandle = 0;
                _activeBreedId = null;
                return;
            }
            if (!_adapter.IsEntityAlive(_activePetHandle))
            {
                _notifications.Show("Your guard dog was injured and returned to the kennel.");
                DismissPet("PetDied");
            }
        }

        public bool LoadState()
        {
            string raw;
            if (!_store.TryLoad(SaveFileName, s => !string.IsNullOrWhiteSpace(s) && s.StartsWith(SchemaHeader, StringComparison.Ordinal), out raw))
            {
                return false;
            }

            _ownedBreeds.Clear();
            string[] lines = raw.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < lines.Length; i++)
            {
                string id = lines[i].Trim();
                if (id.Length > 0 && _breedPrices.ContainsKey(id))
                {
                    _ownedBreeds.Add(id);
                }
            }
            return true;
        }

        public void Stop()
        {
            _isRunning = false;
            DismissPet("Stopped");
        }

        public void Dispose() { Stop(); }

        private void PersistState()
        {
            var sb = new StringBuilder();
            sb.AppendLine(SchemaHeader);
            foreach (string id in _ownedBreeds)
            {
                sb.AppendLine(id);
            }
            _store.Save(SaveFileName, sb.ToString());
        }
    }
}
