using System;
using System.IO;
using System.Windows.Forms;
using ClassicGTA.Contracts;
using ClassicGTA.Core.Activities;
using ClassicGTA.Infrastructure;
using GTA;
using LemonUI;
using LemonUI.Menus;

namespace ClassicGTA.Adapter.GTAV
{
    /// <summary>
    /// Thin SHVDN3 + LemonUI Script Host delegating all game-independent orchestration
    /// to ClassicGTAHostController and UiLoopGuard (C-02, I-06).
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
        private readonly NativeCheckboxItem _zoneCheckbox;
        private readonly GtaVNotificationService _notifications;
        private readonly ClassicGTAHostController _host;

        public ClassicGTAHostScript()
        {
            string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClassicGTA_Data");
            _notifications = new GtaVNotificationService(Path.Combine(dataDir, "ClassicGTA.log"));
            var context = new GtaVGameContext(_notifications);
            var store = new AtomicFileStore(dataDir, _notifications);

            _host = new ClassicGTAHostController(
                context,
                new GtaVWeaponService(),
                new GtaVEmergencyDispatchAdapter(),
                new GtaVTaxiAdapter(),
                new GtaVEmergencySideJobAdapter(),
                new GtaVCompanionAdapter(),
                new GtaVPetAdapter(),
                new GtaVSubmersibleAdapter(),
                new GtaVEntityCleanupAdapter(),
                store,
                _notifications);

            _rootMenu = BuildMenus(out _zoneCheckbox);

            Tick += OnTick;
            KeyDown += OnKeyDown;
            Aborted += OnAborted;

            _notifications.Show("~g~Classic GTA Overhaul~s~ ready! Mod Wallet: ~g~$" + _host.Wallet.GetCash() + "~s~ | ~y~F7~s~ Hub");
        }

        private void OnTick(object sender, EventArgs e)
        {
            _host.UiGuard.TryProcess(
                () => _uiPool.Process(),
                () => _uiPool.HideAll(),
                msg => _notifications.Log(msg),
                msg => _notifications.Show(msg));

            _host.TickScheduler(Game.GameTime);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.F7)
                {
                    if (_host.UiGuard.IsUiDisabled)
                    {
                        _host.UiGuard.Reset();
                    }

                    if (_uiPool.AreAnyVisible)
                    {
                        _uiPool.HideAll();
                    }
                    else
                    {
                        if (_zoneCheckbox != null) _zoneCheckbox.Checked = _host.WeaponsModule.ZoneRestoreEnabled;
                        _rootMenu.Visible = true;
                    }
                    return;
                }

                GameKey mapped = MapKey(e.KeyCode);
                if (mapped != GameKey.None)
                {
                    _host.Scheduler.DispatchKey(mapped);
                    if (mapped == GameKey.F8 && _zoneCheckbox != null)
                    {
                        _zoneCheckbox.Checked = _host.WeaponsModule.ZoneRestoreEnabled;
                    }
                }
            }
            catch (Exception ex)
            {
                _notifications.Log("[Host] KeyDown error: " + ex.Message);
            }
        }

        private void OnAborted(object sender, EventArgs e)
        {
            Tick -= OnTick;
            KeyDown -= OnKeyDown;
            Aborted -= OnAborted;

            try { _uiPool.HideAll(); } catch { /* non-throwing teardown */ }
            _host.HandleAbort();
        }

        private NativeMenu BuildMenus(out NativeCheckboxItem zoneCheckbox)
        {
            var root = new NativeMenu("CLASSIC GTA", "OVERHAUL HUB", "Story Mode Classic Sandbox Restoration");
            _uiPool.Add(root);

            // 1. Weapons & Safe-Zones
            var weaponsSub = AddSubmenu(root, "Weapons & Safe-Zones", "Pocket Arsenal and interior weapon restoration.");
            zoneCheckbox = new NativeCheckboxItem("Zone Weapon Restore (F8)", "Restores combat controls & owned weapons inside restricted interiors.", _host.WeaponsModule.ZoneRestoreEnabled);
            NativeCheckboxItem localZoneRef = zoneCheckbox;
            zoneCheckbox.CheckboxChanged += (s, a) => { _host.WeaponsModule.ZoneRestoreEnabled = localZoneRef.Checked; };
            weaponsSub.Add(zoneCheckbox);
            AddAction(weaponsSub, "Grant Pocket Arsenal (F10)", "Grants all valid weapons with max ammo (non-destructive).", () => _host.WeaponsModule.GrantPocketArsenal());

            // 2. 911 Emergency Dispatch
            var dispatchSub = AddSubmenu(root, "911 Emergency Dispatch", "Call Police, Ambulance, or Fire Department.");
            AddAction(dispatchSub, "Call Ambulance (Heals on Arrival)", "Dispatches paramedics to your road.", () => _host.DispatchModule.RequestDispatch(EmergencyServiceType.Ambulance, Game.GameTime));
            AddAction(dispatchSub, "Call Police Backup", "Dispatches officers to engage hostiles.", () => _host.DispatchModule.RequestDispatch(EmergencyServiceType.Police, Game.GameTime));
            AddAction(dispatchSub, "Call Fire Department", "Dispatches fire truck to extinguish fires.", () => _host.DispatchModule.RequestDispatch(EmergencyServiceType.FireDepartment, Game.GameTime));

            // 3. Classic Side-Hustles
            var jobsSub = AddSubmenu(root, "Classic Side-Hustles", "Taxi fares and 12-level Vigilante/Paramedic/Firefighter jobs.");
            AddAction(jobsSub, "Taxi: Start Shift (Must Be In Cab)", "Pick up fares for distance + speed tips.", () => _host.TaxiModule.StartShift());
            AddAction(jobsSub, "Taxi: End Shift", "End current taxi shift safely.", () => _host.TaxiModule.AbortShift("UserEnded"));
            AddAction(jobsSub, "Emergency Job: Start Level 1", "Sit in Police, Ambulance, or Fire Truck and start 12-level waves.", () => _host.SideJobModule.StartJob(Game.GameTime));
            AddAction(jobsSub, "Emergency Job: Abort", "Cancel active emergency side-job.", () => _host.SideJobModule.AbortJob("UserAborted"));

            // 4. Economy, Properties & Stocks (Mod Wallet)
            var econSub = AddSubmenu(root, "Properties, Businesses & Stocks", "Dedicated Mod Wallet keeps cash and assets synchronized across saves.");
            AddAction(econSub, "Check Mod Wallet Balance", "Displays your Classic GTA Mod Wallet balance.", () => _notifications.Show("Classic GTA Mod Wallet: ~g~$" + _host.Wallet.GetCash()));
            AddAction(econSub, "Buy Del Perro Safehouse ($50,000) [Ledger]", "Unlocks safehouse ownership record (world interior pending).", () => _host.PurchaseAsset("safehouse.del_perro"));
            AddAction(econSub, "Buy Classic Nightclub ($120,000)", "Pays $10,000 per UTC day (7-day cap).", () => _host.PurchaseAsset("business.malibu_club"));
            AddAction(econSub, "Buy Kraken Submersible ($250,000)", "Unlocks Submarine ownership + Summon Kraken.", () => _host.PurchaseAsset("sub.kraken"));
            AddAction(econSub, "Summon Owned Kraken Submersible", "Delivers 1 tracked Kraken in open water.", () => _host.KrakenController.SummonOwnedSubmersible());
            AddAction(econSub, "Collect Business Passive Income", "Collects accrued UTC daily business income.", () => _host.CollectPassiveIncome());
            AddAction(econSub, "Stocks: Buy 10 LCN1", "Buy 10 shares of Cluckin Bell.", () => _notifications.Show(_host.StockEngine.TryBuyShares("LCN1", 10).Message));
            AddAction(econSub, "Stocks: Sell 10 LCN1", "Sell 10 shares of Cluckin Bell.", () => _notifications.Show(_host.StockEngine.TrySellShares("LCN1", 10).Message));
            AddAction(econSub, "Stocks: Advance Market Tick", "Simulate and persist market price movement.", () =>
            {
                _host.StockEngine.AdvanceMarketTick();
                var q = _host.StockEngine.GetQuote("LCN1");
                if (q != null) _notifications.Show("LCN1 now $" + q.CurrentPrice + " (Owned: " + q.SharesOwned + ")");
            });

            // 5. Casino Blackjack (Mod Wallet)
            var bjSub = AddSubmenu(root, "Casino Blackjack (Mod Wallet)", "Classic 21 with 3:2 Natural Blackjack.");
            AddAction(bjSub, "Deal $100 Hand", "Start a new $100 hand.", () => DealBlackjack(100));
            AddAction(bjSub, "Deal $500 Hand", "Start a new $500 hand.", () => DealBlackjack(500));
            AddAction(bjSub, "Hit", "Draw another card.", () =>
            {
                if (_host.BlackjackEngine.IsRoundActive)
                {
                    BlackjackOutcome res = _host.BlackjackEngine.Hit();
                    _notifications.Show("Blackjack Hit: Player=" + _host.BlackjackEngine.PlayerTotal + " vs Dealer=" + _host.BlackjackEngine.DealerVisibleTotal + " (" + res + ")");
                }
            });
            AddAction(bjSub, "Double Down", "Double bet and take one card.", () =>
            {
                if (_host.BlackjackEngine.IsRoundActive)
                {
                    BlackjackOutcome res = _host.BlackjackEngine.DoubleDown();
                    _notifications.Show("Blackjack Double: " + res + " (P=" + _host.BlackjackEngine.PlayerTotal + " vs D=" + _host.BlackjackEngine.DealerVisibleTotal + ")");
                }
            });
            AddAction(bjSub, "Stand", "Hold hand and let dealer play.", () =>
            {
                if (_host.BlackjackEngine.IsRoundActive)
                {
                    BlackjackOutcome res = _host.BlackjackEngine.Stand();
                    _notifications.Show("Blackjack: " + res + " (P=" + _host.BlackjackEngine.PlayerTotal + " vs D=" + _host.BlackjackEngine.DealerTotal + ")");
                }
            });

            // 6. Friends & Guard Dogs
            var socialSub = AddSubmenu(root, "Friends & Guard Dogs", "Call hangout buddies and command guard dogs.");
            AddAction(socialSub, "Call Buddy: Lamar", "Spawn Lamar to hang out and back you up.", () =>
            {
                if (!_host.CompanionModule.CallCompanion("buddy.lamar")) _notifications.Show("Buddy already active or unavailable.");
            });
            AddAction(socialSub, "Buddy: Combat Assist", "Order companion to engage threats.", () =>
            {
                if (!_host.CompanionModule.CommandCombatAssist()) _notifications.Show("No active buddy to command.");
            });
            AddAction(socialSub, "Buddy: Dismiss", "Dismiss companion naturally.", () => _host.CompanionModule.DismissCompanion());
            AddAction(socialSub, "Buy Rottweiler ($1,500)", "Purchase a loyal guard dog.", () => _notifications.Show(_host.PetModule.TryBuyBreed("dog.rottweiler").Message));
            AddAction(socialSub, "Summon Rottweiler", "Call your owned Rottweiler.", () =>
            {
                if (!_host.PetModule.SummonPet("dog.rottweiler")) _notifications.Show("Buy the Rottweiler first or dismiss active dog.");
            });
            AddAction(socialSub, "Dog: Follow", "Order dog to follow.", () => _host.PetModule.GiveCommand(PetCommand.Follow));
            AddAction(socialSub, "Dog: Sit", "Order dog to stay.", () => _host.PetModule.GiveCommand(PetCommand.Sit));
            AddAction(socialSub, "Dog: Guard / Attack Threats", "Order dog to defend you.", () => _host.PetModule.GiveCommand(PetCommand.Guard));
            AddAction(socialSub, "Dog: Send to Kennel", "Dismiss active guard dog.", () => _host.PetModule.DismissPet("Kennelled"));

            return root;
        }

        private void DealBlackjack(int bet)
        {
            if (_host.BlackjackEngine.StartHand(bet))
            {
                _notifications.Show("Blackjack $" + bet + ": Player=" + _host.BlackjackEngine.PlayerTotal + " vs Dealer=" + _host.BlackjackEngine.DealerVisibleTotal + " (" + _host.BlackjackEngine.Outcome + ")");
            }
            else
            {
                _notifications.Show("Cannot deal hand (round active or insufficient Mod Wallet cash).");
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
    }
}
