using System;
using ClassicGTA.Contracts;
using ClassicGTA.Core.Activities;
using ClassicGTA.Core.Economy;
using ClassicGTA.Core.Module1;
using ClassicGTA.Core.Module2;
using ClassicGTA.Infrastructure;

namespace ClassicGTA.Adapter.GTAV
{
    public static class DeploymentManifest
    {
        public static readonly string[] RequiredScriptFiles =
        {
            "ClassicGTA.GTAV.dll",
            "ClassicGTA.Contracts.dll",
            "ClassicGTA.Core.dll",
            "ClassicGTA.Infrastructure.dll",
            "LemonUI.SHVDN3.dll"
        };

        public static readonly string[] ForbiddenScriptFiles =
        {
            "ScriptHookVDotNet3.dll"
        };
    }

    /// <summary>
    /// Engine-independent host orchestrator inside ClassicGTA.Adapter.GTAV (C-02).
    /// Contains zero GTA.* or LemonUI.* references so ClassicGTA.Tests executes real shipping
    /// host wiring, slot selection, UTC income math, legacy migration, Kraken caps, UI recovery,
    /// and abort ordering directly against ClassicGTA.GTAV.dll.
    /// </summary>
    public sealed class ClassicGTAHostController
    {
        public const string DefaultSlotId = AssetOwnershipEngine.ProductionDefaultSlot;

        private readonly IGameContext _context;
        private readonly INotificationService _notifications;
        private readonly Func<DateTime> _utcNowProvider;
        private readonly IEconomyService _storyCash;

        public string SlotId { get; private set; }
        public EntityOwnershipRegistry Registry { get; private set; }
        public ModuleScheduler Scheduler { get; private set; }
        public ModWalletEconomyService Wallet { get; private set; }

        public WeaponAndZoneController WeaponsModule { get; private set; }
        public EmergencyDispatchController DispatchModule { get; private set; }
        public TaxiMissionController TaxiModule { get; private set; }
        public EmergencySideJobController SideJobModule { get; private set; }
        public CompanionHangoutController CompanionModule { get; private set; }
        public PetCompanionController PetModule { get; private set; }

        public AssetOwnershipEngine AssetsEngine { get; private set; }
        public StockMarketEngine StockEngine { get; private set; }
        public BlackjackEngine BlackjackEngine { get; private set; }
        public KrakenDeliveryController KrakenController { get; private set; }
        public UiLoopGuard UiGuard { get; private set; }

        public ClassicGTAHostController(
            IGameContext context,
            IWeaponService weaponService,
            IEmergencyDispatchAdapter dispatchAdapter,
            ITaxiAdapter taxiAdapter,
            IEmergencySideJobAdapter sideJobAdapter,
            ICompanionAdapter companionAdapter,
            IPetAdapter petAdapter,
            ISubmersibleAdapter submersibleAdapter,
            IEntityCleanupAdapter cleanupAdapter,
            IPersistenceStore store,
            INotificationService notifications,
            Func<DateTime> utcNowProvider = null,
            string slotId = DefaultSlotId,
            int initialWalletBalance = ModWalletEconomyService.DefaultInitialBalance,
            IEconomyService storyCashEconomy = null)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (weaponService == null) throw new ArgumentNullException("weaponService");
            if (dispatchAdapter == null) throw new ArgumentNullException("dispatchAdapter");
            if (taxiAdapter == null) throw new ArgumentNullException("taxiAdapter");
            if (sideJobAdapter == null) throw new ArgumentNullException("sideJobAdapter");
            if (companionAdapter == null) throw new ArgumentNullException("companionAdapter");
            if (petAdapter == null) throw new ArgumentNullException("petAdapter");
            if (submersibleAdapter == null) throw new ArgumentNullException("submersibleAdapter");
            if (cleanupAdapter == null) throw new ArgumentNullException("cleanupAdapter");
            if (store == null) throw new ArgumentNullException("store");
            if (notifications == null) throw new ArgumentNullException("notifications");

            _context = context;
            _notifications = notifications;
            _utcNowProvider = utcNowProvider ?? (() => DateTime.UtcNow);
            _storyCash = storyCashEconomy;
            SlotId = string.IsNullOrWhiteSpace(slotId) ? DefaultSlotId : slotId.Trim();

            Registry = new EntityOwnershipRegistry(cleanupAdapter, 20);
            Scheduler = new ModuleScheduler(notifications, 250, 1000);
            UiGuard = new UiLoopGuard(3);

            // C-01 Fix: Dedicated per-slot atomic mod wallet keeps cash and assets on the same persistence plane.
            Wallet = new ModWalletEconomyService(store, notifications, SlotId, initialWalletBalance);

            WeaponsModule = new WeaponAndZoneController(context, weaponService, notifications, new WeaponAndZoneConfig());
            DispatchModule = new EmergencyDispatchController(context, dispatchAdapter, Registry, notifications, new EmergencyDispatchConfig());
            TaxiModule = new TaxiMissionController(context, taxiAdapter, Wallet, Registry, notifications);
            SideJobModule = new EmergencySideJobController(context, sideJobAdapter, Wallet, Registry, notifications);
            CompanionModule = new CompanionHangoutController(context, companionAdapter, Registry, notifications);
            PetModule = new PetCompanionController(context, petAdapter, Wallet, Registry, store, notifications);
            SeedDefaultPets(PetModule);

            AssetsEngine = new AssetOwnershipEngine(context, Wallet, store, notifications, 7, SlotId);
            SeedDefaultAssets(AssetsEngine);

            StockEngine = new StockMarketEngine(context, Wallet, store, notifications);
            SeedDefaultStocks(StockEngine);

            BlackjackEngine = new BlackjackEngine(context, Wallet);
            KrakenController = new KrakenDeliveryController(context, AssetsEngine, submersibleAdapter, Registry, notifications);

            SafeLoad(() => Wallet.LoadState(), "Wallet");
            SafeLoad(() => PetModule.LoadState(), "Pets");
            SafeLoad(() => AssetsEngine.LoadState(), "Assets");
            SafeLoad(() => StockEngine.LoadState(), "Stocks");

            Scheduler.Register(WeaponsModule);
            Scheduler.Register(DispatchModule);
            Scheduler.Register(TaxiModule);
            Scheduler.Register(SideJobModule);
            Scheduler.Register(CompanionModule);
            Scheduler.Register(PetModule);
            Scheduler.StartAll();
        }

        public int CurrentUtcDay
        {
            get { return PassiveIncomeClock.GetUtcDay(_utcNowProvider()); }
        }

        public PurchaseResult PurchaseAsset(string contentId)
        {
            PurchaseResult result = AssetsEngine.TryPurchase(contentId, CurrentUtcDay);
            _notifications.Show(result.Message);
            return result;
        }

        public bool TryAddStoryCash(int amount)
        {
            if (_context.IsOnlineSession)
            {
                _notifications.Show("~r~Blocked: Story Mode only.");
                return false;
            }
            if (amount <= 0 || _storyCash == null) return false;

            _storyCash.Credit(amount, "UserMenuGrant");
            _notifications.Show("Story Cash ~g~+$" + amount.ToString("N0") + "~s~ (Total: ~g~$" + _storyCash.GetCash().ToString("N0") + "~s~)");
            return true;
        }

        public bool TryAddModWalletCash(int amount)
        {
            if (_context.IsOnlineSession)
            {
                _notifications.Show("~r~Blocked: Story Mode only.");
                return false;
            }
            if (amount <= 0) return false;

            Wallet.Credit(amount, "UserMenuGrant");
            _notifications.Show("Mod Wallet ~g~+$" + amount.ToString("N0") + "~s~ (Balance: ~g~$" + Wallet.GetCash().ToString("N0") + "~s~)");
            return true;
        }

        public int GetStoryCash()
        {
            return _storyCash != null ? _storyCash.GetCash() : 0;
        }

        public PassiveIncomeResult CollectPassiveIncome()
        {
            PassiveIncomeResult result = AssetsEngine.CollectPassiveIncomeDetailed(CurrentUtcDay);
            if (result.Status != PassiveIncomeStatus.Paid)
            {
                // I-06 Fix: Report PersistenceFailed, ClockMovedBackward, NoBusinessesOwned, and NothingDue distinctly.
                string prefix = result.Status == PassiveIncomeStatus.PersistenceFailed ? "~r~" : string.Empty;
                _notifications.Show(prefix + result.Message);
            }
            return result;
        }

        public void TickScheduler(int gameTimeMs)
        {
            try
            {
                if (_context.IsOnlineSession)
                {
                    KrakenController.Cleanup(true);
                }
                Scheduler.Tick(gameTimeMs);
            }
            catch (Exception ex)
            {
                _notifications.Log("[Host] Scheduler error: " + ex.Message);
            }
        }

        public void HandleAbort()
        {
            bool isOnline = _context.IsOnlineSession;
            if (isOnline)
            {
                Registry.LatchOnlineSession();
                KrakenController.Cleanup(true);
                Scheduler.Dispose();
            }
            else
            {
                KrakenController.Cleanup(false);
                Scheduler.Dispose();
                Registry.CleanupAll(false);
            }
        }

        private void SafeLoad(Func<bool> loadAction, string label)
        {
            try
            {
                if (!loadAction())
                {
                    _notifications.Log("[Host] SafeLoad: no existing valid state loaded for " + label + " (fresh start or refused corrupt file).");
                }
            }
            catch (Exception ex)
            {
                _notifications.Log("[Host] SafeLoad failed for " + label + ": " + ex.Message);
            }
        }

        private static void SeedDefaultAssets(AssetOwnershipEngine engine)
        {
            engine.RegisterAsset(new AssetDefinition("safehouse.del_perro", "Del Perro Safehouse", AssetCategory.Safehouse, 50000));
            engine.RegisterAsset(new AssetDefinition("business.malibu_club", "Classic Nightclub", AssetCategory.Business, 120000, 10000));
            engine.RegisterAsset(new AssetDefinition("sub.kraken", "Kraken Submersible", AssetCategory.Submarine, 250000));
        }

        private static void SeedDefaultStocks(StockMarketEngine engine)
        {
            engine.RegisterStock(new StockQuote("LCN1", "Cluckin Bell", 100, 10, 500, 10));
            engine.RegisterStock(new StockQuote("REDW", "Redwood Cigarettes", 120, 15, 600, 12));
            engine.RegisterStock(new StockQuote("DEBO", "Debonaire", 95, 15, 600, 12));
        }

        private static void SeedDefaultPets(PetCompanionController pets)
        {
            pets.RegisterBreed("dog.rottweiler", 1500);
            pets.RegisterBreed("dog.shepherd", 1800);
            pets.RegisterBreed("dog.husky", 2000);
        }
    }
}
