using System;
using ClassicGTA.Contracts;

namespace ClassicGTA.Core.Activities
{
    public enum TaxiState
    {
        Idle = 0,
        Searching = 1,
        Pickup = 2,
        EnRoute = 3,
        Payout = 4
    }

    /// <summary>
    /// Classic Taxi Side-Hustle 5-State FSM (Cut Feature #8).
    /// Protects player's taxi via EntityOwnership.Borrowed and guarantees single-settlement payouts.
    /// </summary>
    public sealed class TaxiMissionController : IModModule
    {
        private const float ArrivalRadiusMeters = 12f;
        private const int BaseFareDollars = 50;
        private const int DollarsPerTenMeters = 2;

        private readonly IGameContext _context;
        private readonly ITaxiAdapter _adapter;
        private readonly IEconomyService _economy;
        private readonly IEntityRegistry _registry;
        private readonly INotificationService _notifications;

        private bool _isRunning;
        private bool _isEnabled = true;
        private TaxiState _state = TaxiState.Idle;
        private int _taxiVehicleHandle;
        private int _farePedHandle;
        private int _blipHandle;
        private WorldPoint _pickupPos;
        private WorldPoint _dropoffPos;
        private float _tripDistanceMeters;
        private int _tripStartMs;
        private int _parTimeMs;
        private bool _fareSettled;
        private bool _enterTaskIssued;

        public string ModuleId { get { return "classic.activity.taxi"; } }
        public string DisplayName { get { return "Classic Taxi Side-Hustle"; } }
        public TaxiState CurrentState { get { return _state; } }
        public int LastPayout { get; private set; }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }

        public TaxiMissionController(
            IGameContext context,
            ITaxiAdapter adapter,
            IEconomyService economy,
            IEntityRegistry registry,
            INotificationService notifications)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (adapter == null) throw new ArgumentNullException("adapter");
            if (economy == null) throw new ArgumentNullException("economy");
            if (registry == null) throw new ArgumentNullException("registry");
            if (notifications == null) throw new ArgumentNullException("notifications");

            _context = context;
            _adapter = adapter;
            _economy = economy;
            _registry = registry;
            _notifications = notifications;
        }

        public void Start()
        {
            _isRunning = true;
            _state = TaxiState.Idle;
        }

        public void FastTick() { }

        public bool StartShift()
        {
            if (!_isRunning || !_isEnabled || _context.IsOnlineSession || !_context.IsPlayerReady)
            {
                return false;
            }
            if (_state != TaxiState.Idle) return false;

            int vehHandle;
            if (!_adapter.IsPlayerInTaxi(out vehHandle))
            {
                _notifications.Show("Taxi: Get into a cab to start fares.");
                return false;
            }

            _taxiVehicleHandle = vehHandle;
            // CRITICAL: Player's taxi is Borrowed — NEVER delete it on abort or cleanup.
            _registry.TryTrackEntity(_taxiVehicleHandle, ModuleId, EntityOwnership.Borrowed);
            _state = TaxiState.Searching;
            _notifications.Show("Taxi Shift Started: Searching for nearby fares...");
            return true;
        }

        public void SlowTick(int gameTimeMs)
        {
            if (!_isRunning || !_isEnabled || _state == TaxiState.Idle) return;
            if (_context.IsOnlineSession)
            {
                // V-04 Fix: Drop only this owner's handles without deleting recycled Online entities.
                _registry.DropOwnerWithoutDeleting(ModuleId);
                _taxiVehicleHandle = 0; _farePedHandle = 0; _blipHandle = 0;
                _state = TaxiState.Idle;
                return;
            }
            if (!_context.IsPlayerReady || _context.IsPaused || _context.IsInCutscene) return;

            int currentTaxi;
            if (!_adapter.IsPlayerInTaxi(out currentTaxi) || currentTaxi != _taxiVehicleHandle)
            {
                AbortShift("LeftTaxi");
                return;
            }

            if (_state == TaxiState.Searching)
            {
                HandleSearching();
            }
            else if (_state == TaxiState.Pickup)
            {
                HandlePickup(gameTimeMs);
            }
            else if (_state == TaxiState.EnRoute)
            {
                HandleEnRoute(gameTimeMs);
            }
            else if (_state == TaxiState.Payout)
            {
                CompleteAndReset();
            }
        }

        public void OnKeyPressed(GameKey key) { }

        public void AbortShift(string reason)
        {
            _registry.CleanupOwner(ModuleId, false);
            _taxiVehicleHandle = 0;
            _farePedHandle = 0;
            _blipHandle = 0;
            _enterTaskIssued = false;
            _fareSettled = false;
            _state = TaxiState.Idle;
            _notifications.Log("[Taxi] ABORT reason=" + reason);
        }

        public void Stop()
        {
            _isRunning = false;
            if (_state != TaxiState.Idle)
            {
                AbortShift("Stopped");
            }
        }

        public void Dispose() { Stop(); }

        private void HandleSearching()
        {
            WorldPoint playerPos = _adapter.GetPlayerPosition();
            int ped; WorldPoint pickup; WorldPoint dropoff; int blip;
            if (!_adapter.TrySpawnFare(playerPos, out ped, out pickup, out dropoff, out blip))
            {
                return;
            }

            if (!_registry.TryTrackEntity(ped, ModuleId, EntityOwnership.Owned))
            {
                // H-02 Fix: Immediately delete untracked fare ped & blip on budget refusal.
                _registry.DeleteUntrackedEntity(ped);
                _registry.DeleteUntrackedBlip(blip);
                AbortShift("EntityBudgetExceeded");
                return;
            }
            if (blip > 0)
            {
                _registry.TrackBlip(blip, ModuleId);
            }

            _farePedHandle = ped;
            _pickupPos = pickup;
            _dropoffPos = dropoff;
            _blipHandle = blip;
            _enterTaskIssued = false;
            _fareSettled = false;
            _adapter.SetRouteBlip(_blipHandle, _pickupPos);
            _state = TaxiState.Pickup;
            _notifications.Show("Taxi: Pick up the fare marked on your GPS.");
        }

        private void HandlePickup(int gameTimeMs)
        {
            if (!_adapter.IsEntityAlive(_farePedHandle))
            {
                AbortShift("FareDied");
                return;
            }

            if (_adapter.IsFareInVehicle(_farePedHandle, _taxiVehicleHandle))
            {
                _tripDistanceMeters = _pickupPos.DistanceTo(_dropoffPos);
                _tripStartMs = gameTimeMs;
                // Par time: ~15 m/s average city driving + 15s grace
                _parTimeMs = Math.Max(15000, (int)((_tripDistanceMeters / 15f) * 1000f) + 15000);
                _adapter.SetRouteBlip(_blipHandle, _dropoffPos);
                _state = TaxiState.EnRoute;
                _notifications.Show("Taxi: Drive the passenger to their destination.");
                return;
            }

            if (!_enterTaskIssued && _adapter.GetPlayerPosition().DistanceTo(_pickupPos) <= ArrivalRadiusMeters)
            {
                _enterTaskIssued = true;
                _adapter.TaskFareEnterVehicle(_farePedHandle, _taxiVehicleHandle);
            }
        }

        private void HandleEnRoute(int gameTimeMs)
        {
            if (!_adapter.IsEntityAlive(_farePedHandle))
            {
                AbortShift("FareDied");
                return;
            }

            if (_adapter.GetPlayerPosition().DistanceTo(_dropoffPos) <= ArrivalRadiusMeters)
            {
                _adapter.TaskFareLeaveVehicle(_farePedHandle, _taxiVehicleHandle);
                SettlePayout(gameTimeMs);
                _state = TaxiState.Payout;
            }
        }

        private void SettlePayout(int gameTimeMs)
        {
            if (_fareSettled) return;
            _fareSettled = true;

            int distancePay = ((int)_tripDistanceMeters / 10) * DollarsPerTenMeters;
            int fare = BaseFareDollars + distancePay;
            int elapsed = gameTimeMs >= _tripStartMs ? (gameTimeMs - _tripStartMs) : _parTimeMs;
            int tip = elapsed <= _parTimeMs ? (fare / 4) : 0;
            int total = fare + tip;

            LastPayout = total;
            _economy.Credit(total, "TaxiFare");
            _notifications.Show("Taxi Fare Paid: $" + total + (tip > 0 ? " (includes $" + tip + " speed tip!)" : "."));
            _notifications.Log("[Taxi] PAYOUT total=" + total + " tip=" + tip);
        }

        private void CompleteAndReset()
        {
            // Release owned fare ped & delete blip; Borrowed taxi remains untouched.
            int savedTaxiHandle = _taxiVehicleHandle;
            _registry.CleanupOwner(ModuleId, true);
            // V-12 Fix: Re-register borrowed player taxi so protection never lapses on subsequent fares.
            if (savedTaxiHandle > 0)
            {
                _registry.TryTrackEntity(savedTaxiHandle, ModuleId, EntityOwnership.Borrowed);
            }
            _farePedHandle = 0;
            _blipHandle = 0;
            _state = TaxiState.Idle;
        }
    }
}
