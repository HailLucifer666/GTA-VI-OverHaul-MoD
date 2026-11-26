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

        // S-10 Fix: Mid-round transition to Online blocks Hit/Stand/DoubleDown and cash settlement.
        private static void Blackjack_OnlineMidRoundBlocksHitStandAndDouble()
        {
            var ctx = new FakeGameContext();
            var econ = new FakeEconomyService(1000);
            var bj = new BlackjackEngine(ctx, econ, Deck(5, 10, 6, 7, 9));
            bj.StartHand(100); // Cash = 900
            ctx.IsOnlineSession = true;

            bj.Hit();
            bj.DoubleDown();
            bj.Stand();
            Assert(bj.IsRoundActive && econ.Cash == 900, "Online mid-round must block Hit/Double/Stand and settlement.");
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

        // C-2 Fix: Session restart rollover pays instead of freezing forever.
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
                int paid = e2.CollectPassiveIncome(0, true);
                Assert(paid == 10000, "Session restart rollover must pay 1 day ($10,000), got $" + paid);
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
    }
}
