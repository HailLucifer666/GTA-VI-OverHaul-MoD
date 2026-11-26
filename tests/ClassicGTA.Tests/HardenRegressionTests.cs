using System;
using System.Collections.Generic;
using System.IO;
using ClassicGTA.Contracts;
using ClassicGTA.Core.Activities;
using ClassicGTA.Core.Economy;
using ClassicGTA.Core.Module1;
using ClassicGTA.Core.Module2;
using ClassicGTA.Infrastructure;

namespace ClassicGTA.Tests
{
    internal sealed class PerHandleAliveDispatchAdapter : IEmergencyDispatchAdapter
    {
        public bool VehicleAlive { get; set; }
        public bool ResponderAlive { get; set; }
        public int SpawnedVeh { get; private set; }
        public int SpawnedPed { get; private set; }

        public PerHandleAliveDispatchAdapter()
        {
            VehicleAlive = true;
            ResponderAlive = true;
            SpawnedVeh = 700;
            SpawnedPed = 701;
        }

        public WorldPoint GetPlayerPosition() { return new WorldPoint(0f, 0f, 0f); }
        public bool TryFindRoadSpawnPoint(WorldPoint target, float minDistance, float maxDistance, out WorldPoint spawnPoint)
        {
            spawnPoint = new WorldPoint(80f, 0f, 0f);
            return true;
        }
        public DispatchSpawnResult SpawnUnit(EmergencyServiceType service, WorldPoint spawnPoint)
        {
            return DispatchSpawnResult.Success(SpawnedVeh, SpawnedPed, 702);
        }
        public void TaskDriveTo(int responderPedHandle, int vehicleHandle, WorldPoint destination) { }
        public bool TryGetEntityPosition(int handle, out WorldPoint position) { position = new WorldPoint(80f, 0f, 0f); return true; }
        public bool IsEntityAlive(int handle)
        {
            if (handle == SpawnedVeh) return VehicleAlive;
            if (handle == SpawnedPed) return ResponderAlive;
            return false;
        }
        public void HealPlayerToFull() { }
        public void TaskEngageNearbyHostiles(int responderPedHandle) { }
        public bool TryExtinguishNearbyFires(WorldPoint center, float radius) { return true; }
        public void TaskDepart(int responderPedHandle, int vehicleHandle) { }
    }

    internal static class HardenRegressionTests
    {
        public static IEnumerable<KeyValuePair<string, Action>> GetAll()
        {
            yield return Pair("83_Blackjack_DealerDrawsUntil17OnStand", Blackjack_DealerDrawsUntil17OnStand);
            yield return Pair("84_Blackjack_HidesHoleCardUntilSettled", Blackjack_HidesHoleCardUntilSettled);
            yield return Pair("85_Blackjack_OnlineMidRoundBlocksHitStandAndDouble", Blackjack_OnlineMidRoundBlocksHitStandAndDouble);
            yield return Pair("86_Taxi_MultiTickAtPickup_IssuesEnterTaskOnlyOnce", Taxi_MultiTickAtPickup_IssuesEnterTaskOnlyOnce);
            yield return Pair("87_Dispatch_DeadVehicleWithLivingResponder_Aborts", Dispatch_DeadVehicleWithLivingResponder_Aborts);
            yield return Pair("88_Assets_DifferentSaveSlotFingerprint_Isolated", Assets_DifferentSaveSlotFingerprint_Isolated);
            yield return Pair("89_Assets_PassiveIncome_WorksAcrossSessionRestartAndCapsAt7Days", Assets_PassiveIncome_WorksAcrossSessionRestartAndCapsAt7Days);
            yield return Pair("90_Assets_FirstCollectPersistFailure_DoesNotThrow", Assets_FirstCollectPersistFailure_DoesNotThrow);
            yield return Pair("91_Module1_StripHappeningOnTickNPlus1_StillRestores", Module1_StripHappeningOnTickNPlus1_StillRestores);
            yield return Pair("92_EntityRegistry_NonPositiveHandleReturnsFalseWithoutThrowing", EntityRegistry_NonPositiveHandleReturnsFalseWithoutThrowing);
            yield return Pair("93_EntityRegistry_RefusesOwnedToBorrowedDemotionAndDropAllDoesNotDelete", EntityRegistry_RefusesOwnedToBorrowedDemotionAndDropAllDoesNotDelete);
            yield return Pair("94_EmergencyJobs_ResolvedOnDeadlineTick_SucceedsInsteadOfTimingOut", EmergencyJobs_ResolvedOnDeadlineTick_SucceedsInsteadOfTimingOut);
            yield return Pair("95_OnlineLatch_AbortAfterOnlineNeverDeletesEntities", OnlineLatch_AbortAfterOnlineNeverDeletesEntities);
            yield return Pair("96_CompanionAndPet_BudgetRefusalDeletesUntrackedSpawnAndBlip", CompanionAndPet_BudgetRefusalDeletesUntrackedSpawnAndBlip);
            yield return Pair("97_Module1_ClearsEquipFlagOnExitF8OffAndStop_AndExpiresRetryWindowWhileArmed", Module1_ClearsEquipFlagOnExitF8OffAndStop_AndExpiresRetryWindowWhileArmed);
            yield return Pair("98_Assets_LegacyLedgerWithoutFooterMigratesCleanly", Assets_LegacyLedgerWithoutFooterMigratesCleanly);
        }

        private static KeyValuePair<string, Action> Pair(string name, Action body)
        {
            return new KeyValuePair<string, Action>(name, body);
        }

        private static void Assert(bool cond, string msg)
        {
            if (!cond) throw new InvalidOperationException(msg);
        }

        private static string CreateTempDir()
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestScratch", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        private static void DeleteDir(string dir)
        {
            if (Directory.Exists(dir))
            {
                try { Directory.Delete(dir, true); } catch { }
            }
        }

        private static Func<int> Deck(params int[] cards)
        {
            int idx = 0;
            return () => cards[idx++];
        }

        // T-07 Fix: Proves dealer draws until >= 17 on Stand.
        private static void Blackjack_DealerDrawsUntil17OnStand()
        {
            var econ = new FakeEconomyService(1000);
            // Player: 10+8=18; Dealer: 6+5=11 -> draws 9 -> 20 -> DealerWin
            var bj = new BlackjackEngine(new FakeGameContext(), econ, Deck(10, 6, 8, 5, 9));
            bj.StartHand(100);
            BlackjackOutcome outcome = bj.Stand();
            Assert(outcome == BlackjackOutcome.DealerWin && bj.DealerTotal == 20,
                "Dealer MUST draw from 11 to 20 and win, got " + outcome + " total=" + bj.DealerTotal);
        }

        // C-6 Fix: DealerVisibleTotal exposes only upcard while round is active.
        private static void Blackjack_HidesHoleCardUntilSettled()
        {
            var bj = new BlackjackEngine(new FakeGameContext(), new FakeEconomyService(1000), Deck(10, 7, 8, 10));
            bj.StartHand(100);
            Assert(bj.IsRoundActive, "Round should be active.");
            Assert(bj.DealerVisibleTotal == 7, "Hole card must be hidden (expected visible 7, got " + bj.DealerVisibleTotal + ").");
            bj.Stand();
            Assert(bj.DealerVisibleTotal == 17, "Full dealer total (17) should be visible after Stand.");
        }

        // V-18 & V-20 Fix: Mid-round transition to Online voids cleanly on Hit/Double/Stand without freezing or settling cash.
        private static void Blackjack_OnlineMidRoundBlocksHitStandAndDouble()
        {
            var ctx = new FakeGameContext();
            var econ = new FakeEconomyService(1000);
            var bj1 = new BlackjackEngine(ctx, econ, Deck(5, 10, 6, 7, 9));
            bj1.StartHand(100); // Cash = 900
            ctx.IsOnlineSession = true;
            bj1.Hit();
            Assert(!bj1.IsRoundActive && econ.Cash == 900, "Hit in Online must void round cleanly without mutating Online cash.");

            ctx.IsOnlineSession = false;
            var bj2 = new BlackjackEngine(ctx, econ, Deck(5, 10, 6, 7, 9));
            bj2.StartHand(100); // Cash = 800
            ctx.IsOnlineSession = true;
            bj2.Stand();
            Assert(!bj2.IsRoundActive && econ.Cash == 800, "Stand in Online must void round cleanly without settling.");

            ctx.IsOnlineSession = false;
            var bj3 = new BlackjackEngine(ctx, econ, Deck(5, 10, 6, 7, 9));
            bj3.StartHand(100); // Cash = 700
            ctx.IsOnlineSession = true;
            bj3.DoubleDown();
            Assert(!bj3.IsRoundActive && econ.Cash == 700, "DoubleDown in Online must void round cleanly without spending.");
        }

        // T-08 Fix: Multi-tick at pickup issues TaskFareEnterVehicle exactly once.
        private static void Taxi_MultiTickAtPickup_IssuesEnterTaskOnlyOnce()
        {
            var adapter = new FakeTaxiAdapter();
            var reg = new EntityOwnershipRegistry(new FakeCleanupAdapter());
            var taxi = new TaxiMissionController(new FakeGameContext(), adapter, new FakeEconomyService(0), reg, new FakeNotificationService());
            taxi.Start();
            taxi.StartShift();
            taxi.SlowTick(1000); // Pickup

            adapter.PlayerPosition = adapter.PickupPos;
            taxi.SlowTick(2000);
            taxi.SlowTick(2250);
            taxi.SlowTick(2500);
            Assert(adapter.EnterTasks == 1, "Expected EnterVehicle task issued once, got " + adapter.EnterTasks);
        }

        // T-09 Fix: Destroyed vehicle with living responder still aborts dispatch.
        private static void Dispatch_DeadVehicleWithLivingResponder_Aborts()
        {
            var adapter = new PerHandleAliveDispatchAdapter();
            var reg = new EntityOwnershipRegistry(new FakeCleanupAdapter());
            var mod = new EmergencyDispatchController(new FakeGameContext(), adapter, reg, new FakeNotificationService(), new EmergencyDispatchConfig());
            mod.Start();
            mod.RequestDispatch(EmergencyServiceType.Police, 1000);

            adapter.VehicleAlive = false;
            adapter.ResponderAlive = true;
            mod.SlowTick(2000);
            Assert(mod.CurrentState == DispatchState.Idle, "Dead dispatch vehicle must abort even if responder is alive.");
        }

        // C-1 Fix: Different save-slot fingerprints cannot see each other's ownership.
        private static void Assets_DifferentSaveSlotFingerprint_Isolated()
        {
            string dir = CreateTempDir();
            try
            {
                var notify = new FakeNotificationService();
                var store = new AtomicFileStore(dir, notify);
                var e1 = new AssetOwnershipEngine(new FakeGameContext(), new FakeEconomyService(100000), store, notify, 7, "slot_A");
                e1.RegisterAsset(new AssetDefinition("safehouse.del_perro", "Del Perro", AssetCategory.Safehouse, 50000));
                Assert(e1.TryPurchase("safehouse.del_perro").Succeeded, "Slot A purchase should succeed.");

                var e2 = new AssetOwnershipEngine(new FakeGameContext(), new FakeEconomyService(0), store, notify, 7, "slot_B");
                e2.RegisterAsset(new AssetDefinition("safehouse.del_perro", "Del Perro", AssetCategory.Safehouse, 50000));
                Assert(!e2.LoadState() && !e2.IsOwned("safehouse.del_perro"), "Slot B must NOT inherit Slot A's ownership.");
            }
            finally { DeleteDir(dir); }
        }

        // C-2 & V-10 Fix: Session restart rollover pays 1 day, and large forward jumps clamp at 7-day cap.
        private static void Assets_PassiveIncome_WorksAcrossSessionRestartAndCapsAt7Days()
        {
            string dir = CreateTempDir();
            try
            {
                var notify = new FakeNotificationService();
                var store = new AtomicFileStore(dir, notify);
                var econ = new FakeEconomyService(200000);
                var e1 = new AssetOwnershipEngine(new FakeGameContext(), econ, store, notify, 7, "slot_1");
                e1.RegisterAsset(new AssetDefinition("business.club", "Club", AssetCategory.Business, 100000, 10000));
                e1.TryPurchase("business.club");
                e1.CollectPassiveIncome(10, true); // baseline day 10

                // Simulate fresh session restart where session counter resets to 0
                var e2 = new AssetOwnershipEngine(new FakeGameContext(), econ, store, notify, 7, "slot_1");
                e2.RegisterAsset(new AssetDefinition("business.club", "Club", AssetCategory.Business, 100000, 10000));
                Assert(e2.LoadState(), "Reload should succeed.");
                int paidRestart = e2.CollectPassiveIncome(0, true);
                Assert(paidRestart == 10000, "Session restart rollover must pay 1 day ($10,000), got $" + paidRestart);

                // Jump 20 days forward -> must cap at 7 days ($70,000)
                int paidCapped = e2.CollectPassiveIncome(20, true);
                Assert(paidCapped == 70000, "Forward jump of 20 days must cap at 7 days ($70,000), got $" + paidCapped);
            }
            finally { DeleteDir(dir); }
        }

        // R-1 Fix: First CollectPassiveIncome baseline with failing disk must NOT throw.
        private static void Assets_FirstCollectPersistFailure_DoesNotThrow()
        {
            var e = new AssetOwnershipEngine(new FakeGameContext(), new FakeEconomyService(0), new FailingPersistenceStore(), new FakeNotificationService());
            int paid = e.CollectPassiveIncome(5, true);
            Assert(paid == 0, "Expected 0 without throwing on first-call persist failure.");
        }

        // R-4 Fix: Delayed weapon strip on tick N+1 after entering interior still restores.
        private static void Module1_StripHappeningOnTickNPlus1_StillRestores()
        {
            var ctx = new FakeGameContext();
            var weapons = new FakeWeaponService();
            var notify = new FakeNotificationService();
            var mod = new WeaponAndZoneController(ctx, weapons, notify, new WeaponAndZoneConfig());
            mod.Start();

            weapons.OwnedWeapons.Add(FakeWeaponService.Carbine);
            weapons.CurrentWeaponId = FakeWeaponService.Carbine;
            weapons.InteriorId = 0;
            mod.SlowTick(1000);

            // Tick N: entered interior, still holding Carbine
            weapons.InteriorId = 42;
            mod.SlowTick(1250);

            // Tick N+1: game script strips weapon to Unarmed
            weapons.CurrentWeaponId = FakeWeaponService.Unarmed;
            mod.SlowTick(1500);

            Assert(weapons.CurrentWeaponId == FakeWeaponService.Carbine, "Delayed strip on tick N+1 MUST still restore Carbine.");
        }

        // H-03 Fix: Non-positive handle returns false instead of throwing.
        private static void EntityRegistry_NonPositiveHandleReturnsFalseWithoutThrowing()
        {
            var reg = new EntityOwnershipRegistry(new FakeCleanupAdapter());
            Assert(!reg.TryTrackEntity(0, "mod", EntityOwnership.Owned), "Handle 0 must return false.");
            Assert(!reg.TryTrackEntity(-5, "mod", EntityOwnership.Owned), "Negative handle must return false.");
        }

        // S-06 & S-11 Fix: Refuses Owned->Borrowed demotion; DropAllWithoutDeleting never calls DeleteEntity.
        private static void EntityRegistry_RefusesOwnedToBorrowedDemotionAndDropAllDoesNotDelete()
        {
            var cleanup = new FakeCleanupAdapter();
            var reg = new EntityOwnershipRegistry(cleanup);

            Assert(reg.TryTrackEntity(500, "dispatch", EntityOwnership.Owned), "Track Owned.");
            Assert(!reg.TryTrackEntity(500, "jobs", EntityOwnership.Borrowed), "Must refuse Owned->Borrowed demotion.");

            reg.DropAllWithoutDeleting();
            Assert(reg.TrackedEntityCount == 0 && cleanup.DeletedEntities.Count == 0,
                "DropAllWithoutDeleting must clear records with ZERO DeleteEntity calls.");
        }

        // R-5 Fix: Target resolved on exact deadline tick succeeds instead of failing timeout.
        private static void EmergencyJobs_ResolvedOnDeadlineTick_SucceedsInsteadOfTimingOut()
        {
            var adapter = new FakeSideJobAdapter();
            var econ = new FakeEconomyService(0);
            var reg = new EntityOwnershipRegistry(new FakeCleanupAdapter());
            var job = new EmergencySideJobController(new FakeGameContext(), adapter, econ, reg, new FakeNotificationService());
            job.Start();
            job.StartJob(1000);

            adapter.TargetResolved = true;
            // Exact deadline tick (65000ms elapsed)
            job.SlowTick(66000);
            Assert(job.IsJobActive && job.CurrentLevel == 2 && econ.Cash == 50,
                "Wave resolved on deadline tick must advance to Level 2 instead of timing out.");
        }

        // V-13 Fix: Latching Online BEFORE scheduler Dispose guarantees ZERO DeleteEntity calls on abort.
        private static void OnlineLatch_AbortAfterOnlineNeverDeletesEntities()
        {
            var ctx = new FakeGameContext();
            var cleanup = new FakeCleanupAdapter();
            var reg = new EntityOwnershipRegistry(cleanup);
            var notify = new FakeNotificationService();
            var scheduler = new ModuleScheduler(notify);
            var companion = new CompanionHangoutController(ctx, new FakeCompanionAdapter(), reg, notify);
            scheduler.Register(companion);
            scheduler.StartAll();
            companion.CallCompanion("buddy.lamar");

            ctx.IsOnlineSession = true;
            reg.LatchOnlineSession();
            scheduler.Dispose();

            Assert(cleanup.DeletedEntities.Count == 0 && cleanup.DeletedBlips.Count == 0,
                "Online abort MUST NOT call DeleteEntity or DeleteBlip.");
            Assert(reg.TrackedEntityCount == 0 && reg.TrackedBlipCount == 0, "Registry must be empty.");
        }

        // V-03 Fix: Companion & Pet budget refusal deletes untracked ped and blip immediately.
        private static void CompanionAndPet_BudgetRefusalDeletesUntrackedSpawnAndBlip()
        {
            var cleanup = new FakeCleanupAdapter();
            var reg = new EntityOwnershipRegistry(cleanup, 1);
            reg.TryTrackEntity(1, "other", EntityOwnership.Owned); // Fill cap=1

            var companion = new CompanionHangoutController(new FakeGameContext(), new FakeCompanionAdapter(), reg, new FakeNotificationService());
            companion.Start();
            Assert(!companion.CallCompanion("buddy.lamar"), "Companion should fail when cap is full.");
            Assert(cleanup.DeletedEntities.Count == 1 && cleanup.DeletedBlips.Count == 1,
                "Refused companion spawn must delete untracked ped and blip.");

            var dir = CreateTempDir();
            try
            {
                var notify = new FakeNotificationService();
                var pets = new PetCompanionController(new FakeGameContext(), new FakePetAdapter(), new FakeEconomyService(5000), reg, new AtomicFileStore(dir, notify), notify);
                pets.RegisterBreed("dog.rottweiler", 1000);
                pets.Start();
                pets.TryBuyBreed("dog.rottweiler");

                Assert(!pets.SummonPet("dog.rottweiler"), "Pet summon should fail when cap is full.");
                Assert(cleanup.DeletedEntities.Count == 2 && cleanup.DeletedBlips.Count == 2,
                    "Refused pet spawn must delete untracked pet and blip.");
            }
            finally { DeleteDir(dir); }
        }

        // V-11, V-19, V-20 Fix: Clears equip flag on F8 off & Stop, and expires retry window while armed.
        private static void Module1_ClearsEquipFlagOnExitF8OffAndStop_AndExpiresRetryWindowWhileArmed()
        {
            var ctx = new FakeGameContext();
            var weapons = new FakeWeaponService();
            var mod = new WeaponAndZoneController(ctx, weapons, new FakeNotificationService(), new WeaponAndZoneConfig());
            mod.Start();

            weapons.OwnedWeapons.Add(FakeWeaponService.Carbine);
            weapons.CurrentWeaponId = FakeWeaponService.Carbine;
            weapons.InteriorId = 42;

            // Tick 10 times inside interior while armed -> 8-tick retry window must expire!
            for (int i = 0; i < 10; i++) mod.SlowTick(1000 + i * 250);

            // Player deliberately holsters weapon after window expired -> must NOT be overridden!
            weapons.CurrentWeaponId = FakeWeaponService.Unarmed;
            mod.SlowTick(5000);
            Assert(weapons.SelectWeaponCalls == 0, "Deliberate holster after retry window must not be overridden.");

            // Toggling F8 off inside interior must clear SetCanEquipAllWeapons(false)
            mod.OnKeyPressed(GameKey.F8);
            mod.SlowTick(5500);
            Assert(weapons.CanEquipAllFalseCalls >= 1, "Expected SetCanEquipAllWeapons(false) on F8 toggle off.");

            int beforeStop = weapons.CanEquipAllFalseCalls;
            mod.Stop();
            Assert(weapons.CanEquipAllFalseCalls > beforeStop, "Expected unconditional SetCanEquipAllWeapons(false) on Stop.");
        }

        // V-16 Fix: Legacy SCHEMA=1 file without END_OF_FILE footer loads cleanly instead of being discarded.
        private static void Assets_LegacyLedgerWithoutFooterMigratesCleanly()
        {
            string dir = CreateTempDir();
            try
            {
                var notify = new FakeNotificationService();
                var store = new AtomicFileStore(dir, notify);
                // Write a legacy v0.6.0 ledger (no SLOT=, no END_OF_FILE)
                store.Save("ClassicGTA_Assets.dat", "SCHEMA=1\nLAST_DAY=5\nOWNED=sub.kraken\n");

                var engine = new AssetOwnershipEngine(new FakeGameContext(), new FakeEconomyService(0), store, notify, 7, "default");
                engine.RegisterAsset(new AssetDefinition("sub.kraken", "Kraken", AssetCategory.Submarine, 250000));
                Assert(engine.LoadState() && engine.IsOwned("sub.kraken"),
                    "Legacy SCHEMA=1 ledger without footer MUST load cleanly.");
            }
            finally { DeleteDir(dir); }
        }
    }
}
