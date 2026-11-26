using System;
using System.IO;
using System.Windows.Forms;
using ClassicGTA.Contracts;
using ClassicGTA.Core.Activities;
using ClassicGTA.Core.Economy;
using ClassicGTA.Core.Module1;
using ClassicGTA.Core.Module2;
using ClassicGTA.Infrastructure;
using GTA;
using LemonUI;
using LemonUI.Menus;

namespace ClassicGTA.Adapter.GTAV
{
    /// <summary>
    /// Unified SHVDN3 Script Host wiring all verified ClassicGTA.Core engines + LemonUI Submenus.
    ///
    /// Hotkeys:
    ///   F7  : Open/Close Classic GTA Overhaul Hub (LemonUI)
    ///   F8  : Toggle Zone/Interior Weapon Restoration
    ///   F9  : Quick 911 Emergency Dispatch (cycles Ambulance / Fire / Police)
    ///   F10 : Grant Pocket Arsenal (non-destructive, max ammo)
    /// </summary>
    public sealed class ClassicGTAHostScript : Script
    {
        private readonly ObjectPool _uiPool = new ObjectPool();
        private readonly NativeMenu _rootMenu;
        private readonly ModuleScheduler _scheduler;
        private readonly EntityOwnershipRegistry _registry;

        private readonly WeaponAndZoneController _weaponsModule;
        private readonly EmergencyDispatchController _dispatchModule;
        private readonly TaxiMissionController _taxiModule;
        private readonly EmergencySideJobController _sideJobModule;
        private readonly CompanionHangoutController _companionModule;
        private readonly PetCompanionController _petModule;

        private readonly AssetOwnershipEngine _assetsEngine;
        private readonly StockMarketEngine _stockEngine;
        private readonly BlackjackEngine _blackjackEngine;

        public ClassicGTAHostScript()
        {
            string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClassicGTA_Data");
            var notifications = new GtaVNotificationService(Path.Combine(dataDir, "ClassicGTA.log"));
            var context = new GtaVGameContext();
            var cleanup = new GtaVEntityCleanupAdapter();
            var store = new AtomicFileStore(dataDir, notifications);
            var economy = new GtaVEconomyService();

            _registry = new EntityOwnershipRegistry(cleanup, 20);
            _scheduler = new ModuleScheduler(notifications, 250, 1000);

            _weaponsModule = new WeaponAndZoneController(context, new GtaVWeaponService(), notifications, new WeaponAndZoneConfig());
            _dispatchModule = new EmergencyDispatchController(context, new GtaVEmergencyDispatchAdapter(), _registry, notifications, new EmergencyDispatchConfig());
            _taxiModule = new TaxiMissionController(context, new GtaVTaxiAdapter(), economy, _registry, notifications);
            _sideJobModule = new EmergencySideJobController(context, new GtaVEmergencySideJobAdapter(), economy, _registry, notifications);
            _companionModule = new CompanionHangoutController(context, new GtaVCompanionAdapter(), _registry, notifications);
            _petModule = new PetCompanionController(context, new GtaVPetAdapter(), economy, _registry, store, notifications);
            SeedDefaultPets(_petModule);
            _petModule.LoadState();

            _assetsEngine = new AssetOwnershipEngine(context, economy, store, notifications, 7);
            SeedDefaultAssets(_assetsEngine);
            _assetsEngine.LoadState();

            _stockEngine = new StockMarketEngine(context, economy, store, notifications);
            SeedDefaultStocks(_stockEngine);
            _stockEngine.LoadState();

            _blackjackEngine = new BlackjackEngine(context, economy);

            _scheduler.Register(_weaponsModule);
            _scheduler.Register(_dispatchModule);
            _scheduler.Register(_taxiModule);
            _scheduler.Register(_sideJobModule);
            _scheduler.Register(_companionModule);
            _scheduler.Register(_petModule);
            _scheduler.StartAll();

            _rootMenu = BuildMenus(notifications);

            Tick += OnTick;
            KeyDown += OnKeyDown;
            Aborted += OnAborted;

            notifications.Show("~g~Classic GTA Overhaul~s~ ready! ~y~F7~s~ Hub | ~y~F8~s~ Zone | ~y~F9~s~ 911 | ~y~F10~s~ Arsenal");
        }

        private void OnTick(object sender, EventArgs e)
        {
            _uiPool.Process();
            _scheduler.Tick(Game.GameTime);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F7)
            {
                _rootMenu.Visible = !_rootMenu.Visible;
                return;
            }

            GameKey mapped = MapKey(e.KeyCode);
            if (mapped != GameKey.None)
            {
                _scheduler.DispatchKey(mapped);
            }
        }

        private void OnAborted(object sender, EventArgs e)
        {
            Tick -= OnTick;
            KeyDown -= OnKeyDown;
            Aborted -= OnAborted;

            _uiPool.HideAll();
            _scheduler.Dispose();
            _registry.CleanupAll(false);
        }

        private NativeMenu BuildMenus(INotificationService notifications)
        {
            var root = new NativeMenu("CLASSIC GTA", "OVERHAUL HUB", "Offline Story Mode Classic Sandbox Restoration");
            _uiPool.Add(root);

            // 1. Weapons & Safe-Zones
            var weaponsSub = AddSubmenu(root, "Weapons & Safe-Zones", "Pocket Arsenal and interior weapon restoration.");
            var zoneItem = new NativeCheckboxItem("Zone Weapon Restore (F8)", "Restores combat controls & owned weapons in restricted zones.", true);
            zoneItem.CheckboxChanged += (s, a) => { _weaponsModule.ZoneRestoreEnabled = zoneItem.Checked; };
            weaponsSub.Add(zoneItem);
            AddAction(weaponsSub, "Grant Pocket Arsenal (F10)", "Grants all valid weapons with max ammo.", () => _weaponsModule.GrantPocketArsenal());

            // 2. 911 Emergency Dispatch
            var dispatchSub = AddSubmenu(root, "911 Emergency Dispatch", "Call Police, Ambulance, or Fire Department.");
            AddAction(dispatchSub, "Call Ambulance (Heals on Arrival)", "Dispatches paramedics to your road.", () => _dispatchModule.RequestDispatch(EmergencyServiceType.Ambulance, Game.GameTime));
            AddAction(dispatchSub, "Call Police Backup", "Dispatches officers to engage hostiles.", () => _dispatchModule.RequestDispatch(EmergencyServiceType.Police, Game.GameTime));
            AddAction(dispatchSub, "Call Fire Department", "Dispatches fire truck to extinguish fires.", () => _dispatchModule.RequestDispatch(EmergencyServiceType.FireDepartment, Game.GameTime));

            // 3. Classic Side-Hustles
            var jobsSub = AddSubmenu(root, "Classic Side-Hustles", "Taxi fares and 12-level Vigilante/Paramedic/Firefighter jobs.");
            AddAction(jobsSub, "Taxi: Start Shift (Must Be In Cab)", "Pick up fares for distance + speed tips.", () => _taxiModule.StartShift());
            AddAction(jobsSub, "Taxi: End Shift", "End current taxi shift safely.", () => _taxiModule.AbortShift("UserEnded"));
            AddAction(jobsSub, "Emergency Job: Start Level 1", "Sit in Police, Ambulance, or Fire Truck and start 12-level waves.", () => _sideJobModule.StartJob(Game.GameTime));
            AddAction(jobsSub, "Emergency Job: Abort", "Cancel active emergency side-job.", () => _sideJobModule.AbortJob("UserAborted"));

            // 4. Economy, Properties & Stocks
            var econSub = AddSubmenu(root, "Properties, Businesses & Stocks", "Buy safehouses, passive businesses, vehicles & LCN stocks.");
            AddAction(econSub, "Buy Del Perro Safehouse ($50,000)", "Classic open-market safehouse.", () => notifications.Show(_assetsEngine.TryPurchase("safehouse.del_perro").Message));
            AddAction(econSub, "Buy Classic Nightclub ($120,000)", "Pays $10,000 per elapsed in-game day (7d cap).", () => notifications.Show(_assetsEngine.TryPurchase("business.malibu_club").Message));
            AddAction(econSub, "Buy Kraken Submersible ($250,000)", "Unlock submarine ownership.", () => notifications.Show(_assetsEngine.TryPurchase("sub.kraken").Message));
            AddAction(econSub, "Collect Business Passive Income", "Collects accrued in-game day income.", () =>
            {
                int day = (int)(World.CurrentTimeOfDay.TotalHours / 24.0) + (Game.GameTime / 2880000);
                int paid = _assetsEngine.CollectPassiveIncome(day);
                if (paid == 0) notifications.Show("No new business income due yet.");
            });
            AddAction(econSub, "Stocks: Buy 10 LCN1", "Buy 10 shares of Cluckin Bell.", () => notifications.Show(_stockEngine.TryBuyShares("LCN1", 10).Message));
            AddAction(econSub, "Stocks: Sell 10 LCN1", "Sell 10 shares of Cluckin Bell.", () => notifications.Show(_stockEngine.TrySellShares("LCN1", 10).Message));
            AddAction(econSub, "Stocks: Advance Market Tick", "Simulate market price movement.", () =>
            {
                _stockEngine.AdvanceMarketTick();
                StockQuote q = _stockEngine.GetQuote("LCN1");
                notifications.Show("LCN1 now $" + q.CurrentPrice + " (Owned: " + q.SharesOwned + ")");
            });

            // 5. Casino Blackjack
            var bjSub = AddSubmenu(root, "Casino Blackjack (Virtual Cash)", "Classic 21 with 3:2 Natural Blackjack.");
            AddAction(bjSub, "Deal $100 Hand", "Start a new $100 hand.", () => DealBlackjack(100, notifications));
            AddAction(bjSub, "Deal $500 Hand", "Start a new $500 hand.", () => DealBlackjack(500, notifications));
            AddAction(bjSub, "Hit", "Draw another card.", () =>
            {
                if (_blackjackEngine.IsRoundActive)
                {
                    BlackjackOutcome res = _blackjackEngine.Hit();
                    notifications.Show("Blackjack Hit: Total=" + _blackjackEngine.PlayerTotal + " (" + res + ")");
                }
            });
            AddAction(bjSub, "Double Down", "Double bet and take one card.", () =>
            {
                if (_blackjackEngine.IsRoundActive)
                {
                    BlackjackOutcome res = _blackjackEngine.DoubleDown();
                    notifications.Show("Blackjack Double: " + res + " (P=" + _blackjackEngine.PlayerTotal + " vs D=" + _blackjackEngine.DealerTotal + ")");
                }
            });
            AddAction(bjSub, "Stand", "Hold hand and let dealer play.", () =>
            {
                if (_blackjackEngine.IsRoundActive)
                {
                    BlackjackOutcome res = _blackjackEngine.Stand();
                    notifications.Show("Blackjack: " + res + " (P=" + _blackjackEngine.PlayerTotal + " vs D=" + _blackjackEngine.DealerTotal + ")");
                }
            });

            // 6. Friends & Guard Dogs
            var socialSub = AddSubmenu(root, "Friends & Guard Dogs", "Call hangout buddies and command guard dogs.");
            AddAction(socialSub, "Call Buddy: Lamar", "Spawn Lamar to hang out and back you up.", () => _companionModule.CallCompanion("buddy.lamar"));
            AddAction(socialSub, "Buddy: Combat Assist", "Order companion to engage threats.", () => _companionModule.CommandCombatAssist());
            AddAction(socialSub, "Buddy: Dismiss", "Dismiss companion naturally.", () => _companionModule.DismissCompanion());
            AddAction(socialSub, "Buy Rottweiler ($1,500)", "Purchase a loyal guard dog.", () => notifications.Show(_petModule.TryBuyBreed("dog.rottweiler").Message));
            AddAction(socialSub, "Summon Rottweiler", "Call your owned Rottweiler.", () =>
            {
                if (!_petModule.SummonPet("dog.rottweiler")) notifications.Show("Buy the Rottweiler first or dismiss active dog.");
            });
            AddAction(socialSub, "Dog: Follow", "Order dog to follow.", () => _petModule.GiveCommand(PetCommand.Follow));
            AddAction(socialSub, "Dog: Sit", "Order dog to stay.", () => _petModule.GiveCommand(PetCommand.Sit));
            AddAction(socialSub, "Dog: Guard / Attack Threats", "Order dog to defend you.", () => _petModule.GiveCommand(PetCommand.Guard));
            AddAction(socialSub, "Dog: Send to Kennel", "Dismiss active guard dog.", () => _petModule.DismissPet("Kennelled"));

            return root;
        }

        private void DealBlackjack(int bet, INotificationService notifications)
        {
            if (_blackjackEngine.StartHand(bet))
            {
                notifications.Show("Blackjack $" + bet + ": Player=" + _blackjackEngine.PlayerTotal + " vs Dealer=" + _blackjackEngine.DealerTotal + " (" + _blackjackEngine.Outcome + ")");
            }
            else
            {
                notifications.Show("Cannot deal hand (round active or insufficient cash).");
            }
        }

        private NativeMenu AddSubmenu(NativeMenu parent, string title, string description)
        {
            var sub = new NativeMenu("CLASSIC GTA", title.ToUpperInvariant(), description);
            _uiPool.Add(sub);
            parent.AddSubMenu(sub);
            return sub;
        }

        private static void AddAction(NativeMenu menu, string title, string description, Action onActivate)
        {
            var item = new NativeItem(title, description);
            item.Activated += (s, a) => onActivate();
            menu.Add(item);
        }

        private static GameKey MapKey(Keys key)
        {
            if (key == Keys.F8) return GameKey.F8;
            if (key == Keys.F9) return GameKey.F9;
            if (key == Keys.F10) return GameKey.F10;
            if (key == Keys.F11) return GameKey.F11;
            return GameKey.None;
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
