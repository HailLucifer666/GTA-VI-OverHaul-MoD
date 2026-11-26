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
    /// Single SHVDN3 Script Host wiring all verified ClassicGTA.Core modules + LemonUI menu.
    ///
    /// Hotkeys:
    ///   F7  : Open/Close Classic GTA Master Menu (LemonUI)
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

            _assetsEngine = new AssetOwnershipEngine(context, economy, store, notifications, 7);
            SeedDefaultAssets(_assetsEngine);
            _assetsEngine.LoadState();

            _stockEngine = new StockMarketEngine(context, economy, store, notifications);
            SeedDefaultStocks(_stockEngine);
            _stockEngine.LoadState();

            _blackjackEngine = new BlackjackEngine(context, economy);

            _scheduler.Register(_weaponsModule);
            _scheduler.Register(_dispatchModule);
            _scheduler.StartAll();

            _rootMenu = BuildMenus(notifications);
            _uiPool.Add(_rootMenu);

            Tick += OnTick;
            KeyDown += OnKeyDown;
            Aborted += OnAborted;

            notifications.Show("~g~Classic GTA Overhaul~s~ loaded. ~y~F7~s~ Menu | ~y~F8~s~ Zone | ~y~F9~s~ 911 | ~y~F10~s~ Arsenal");
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

            _rootMenu.Visible = false;
            _scheduler.Dispose();
            _registry.CleanupAll(false);
        }

        private NativeMenu BuildMenus(INotificationService notifications)
        {
            var menu = new NativeMenu("CLASSIC GTA", "OVERHAUL HUB", "Offline Story Mode Classic Sandbox Restoration");

            var zoneItem = new NativeCheckboxItem("Zone Weapon Restore (F8)", "Restores combat controls & owned weapons in restricted zones.", true);
            zoneItem.CheckboxChanged += (s, a) => { _weaponsModule.ZoneRestoreEnabled = zoneItem.Checked; };
            menu.Add(zoneItem);

            var arsenalItem = new NativeItem("Grant Pocket Arsenal (F10)", "Grants all valid weapons with max ammo (non-destructive).");
            arsenalItem.Activated += (s, a) => { _weaponsModule.GrantPocketArsenal(); };
            menu.Add(arsenalItem);

            var callAmbulance = new NativeItem("911: Call Ambulance", "Dispatches paramedics to heal you on arrival.");
            callAmbulance.Activated += (s, a) => { _dispatchModule.RequestDispatch(EmergencyServiceType.Ambulance, Game.GameTime); };
            menu.Add(callAmbulance);

            var callPolice = new NativeItem("911: Call Police", "Dispatches officers to engage nearby hostiles.");
            callPolice.Activated += (s, a) => { _dispatchModule.RequestDispatch(EmergencyServiceType.Police, Game.GameTime); };
            menu.Add(callPolice);

            var callFire = new NativeItem("911: Call Fire Department", "Dispatches fire truck to extinguish nearby fires.");
            callFire.Activated += (s, a) => { _dispatchModule.RequestDispatch(EmergencyServiceType.FireDepartment, Game.GameTime); };
            menu.Add(callFire);

            var buyClub = new NativeItem("Buy Nightclub ($120,000)", "Passive-income business paying $10,000/in-game day.");
            buyClub.Activated += (s, a) => { notifications.Show(_assetsEngine.TryPurchase("business.malibu_club").Message); };
            menu.Add(buyClub);

            var bjPlay = new NativeItem("Blackjack: Deal $100 Hand", "Play virtual-cash Blackjack (3:2 natural 21).");
            bjPlay.Activated += (s, a) =>
            {
                if (_blackjackEngine.StartHand(100))
                {
                    notifications.Show("Blackjack dealt! Player=" + _blackjackEngine.PlayerTotal + " Dealer=" + _blackjackEngine.DealerTotal + " (" + _blackjackEngine.Outcome + ")");
                }
            };
            menu.Add(bjPlay);

            var bjStand = new NativeItem("Blackjack: Stand", "Stand on your current Blackjack hand.");
            bjStand.Activated += (s, a) =>
            {
                if (_blackjackEngine.IsRoundActive)
                {
                    BlackjackOutcome res = _blackjackEngine.Stand();
                    notifications.Show("Blackjack: " + res + " (P=" + _blackjackEngine.PlayerTotal + " vs D=" + _blackjackEngine.DealerTotal + ")");
                }
            };
            menu.Add(bjStand);

            return menu;
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
    }
}
