using System;
using System.Collections.Generic;
using ClassicGTA.Contracts;
using ClassicGTA.Core.Module2;
using ClassicGTA.Infrastructure;

namespace ClassicGTA.Tests
{
    internal sealed class FakeDispatchAdapter : IEmergencyDispatchAdapter
    {
        public WorldPoint PlayerPosition { get; set; }
        public WorldPoint SpawnPoint { get; set; }
        public WorldPoint UnitPosition { get; set; }
        public bool RoadFound { get; set; }
        public bool UnitAlive { get; set; }
        public bool FireExtinguishSupported { get; set; }

        public int SpawnCalls { get; private set; }
        public int DriveTaskCalls { get; private set; }
        public int HealCalls { get; private set; }
        public int EngageHostilesCalls { get; private set; }
        public int ExtinguishCalls { get; private set; }
        public int DepartCalls { get; private set; }

        private int _nextHandle = 500;

        public FakeDispatchAdapter()
        {
            PlayerPosition = new WorldPoint(0f, 0f, 0f);
            SpawnPoint = new WorldPoint(100f, 0f, 0f);
            UnitPosition = new WorldPoint(100f, 0f, 0f);
            RoadFound = true;
            UnitAlive = true;
            FireExtinguishSupported = true;
        }

        public WorldPoint GetPlayerPosition() { return PlayerPosition; }

        public bool TryFindRoadSpawnPoint(WorldPoint target, float minDistance, float maxDistance, out WorldPoint spawnPoint)
        {
            spawnPoint = SpawnPoint;
            return RoadFound;
        }

        public DispatchSpawnResult SpawnUnit(EmergencyServiceType service, WorldPoint spawnPoint)
        {
            SpawnCalls++;
            int veh = _nextHandle++;
            int ped = _nextHandle++;
            int blip = _nextHandle++;
            return DispatchSpawnResult.Success(veh, ped, blip);
        }

        public void TaskDriveTo(int responderPedHandle, int vehicleHandle, WorldPoint destination)
        {
            DriveTaskCalls++;
        }

        public bool TryGetEntityPosition(int handle, out WorldPoint position)
        {
            position = UnitPosition;
            return true;
        }

        public bool IsEntityAlive(int handle) { return UnitAlive; }
        public void HealPlayerToFull() { HealCalls++; }
        public void TaskEngageNearbyHostiles(int responderPedHandle) { EngageHostilesCalls++; }

        public bool TryExtinguishNearbyFires(WorldPoint center, float radius)
        {
            ExtinguishCalls++;
            return FireExtinguishSupported;
        }

        public void TaskDepart(int responderPedHandle, int vehicleHandle) { DepartCalls++; }
    }

    internal static class Module2Tests
    {
        public static IEnumerable<KeyValuePair<string, Action>> GetAll()
        {
            yield return Pair("25_Dispatch_InOnlineSession_Blocked", Dispatch_InOnlineSession_Blocked);
            yield return Pair("26_Dispatch_Ambulance_DoesNotHealBeforePhysicalArrival", Dispatch_Ambulance_DoesNotHealBeforePhysicalArrival);
            yield return Pair("27_Dispatch_Ambulance_HealsOnlyOnArrivalThenDepartsAndCleansUp", Dispatch_Ambulance_HealsOnlyOnArrivalThenDepartsAndCleansUp);
            yield return Pair("28_Dispatch_Police_EngagesHostilesOnArrival", Dispatch_Police_EngagesHostilesOnArrival);
            yield return Pair("29_Dispatch_Fire_AttemptsExtinguishAndHandlesUnsupportedGracefully", Dispatch_Fire_AttemptsExtinguishAndHandlesUnsupportedGracefully);
            yield return Pair("30_Dispatch_DoesNotSpamDriveTaskUnlessPlayerMovesPastThreshold", Dispatch_DoesNotSpamDriveTaskUnlessPlayerMovesPastThreshold);
            yield return Pair("31_Dispatch_Timeout_CleansUpEntitiesAndBlips", Dispatch_Timeout_CleansUpEntitiesAndBlips);
            yield return Pair("32_Dispatch_ResponderDiesEnRoute_AbortsAndCleansUp", Dispatch_ResponderDiesEnRoute_AbortsAndCleansUp);
            yield return Pair("33_Dispatch_EnforcesSingleActiveDispatchAndCooldown", Dispatch_EnforcesSingleActiveDispatchAndCooldown);
            yield return Pair("34_Dispatch_StopAndDispose_CleansUpOwnedEntitiesAndBlips", Dispatch_StopAndDispose_CleansUpOwnedEntitiesAndBlips);
        }

        private static KeyValuePair<string, Action> Pair(string name, Action body)
        {
            return new KeyValuePair<string, Action>(name, body);
        }

        private static void Assert(bool cond, string msg)
        {
            if (!cond) throw new InvalidOperationException(msg);
        }

        private static EmergencyDispatchController Create(
            out FakeGameContext ctx,
            out FakeDispatchAdapter adapter,
            out FakeCleanupAdapter cleanup,
            out EntityOwnershipRegistry registry,
            out FakeNotificationService notify)
        {
            ctx = new FakeGameContext();
            adapter = new FakeDispatchAdapter();
            cleanup = new FakeCleanupAdapter();
            registry = new EntityOwnershipRegistry(cleanup, 20);
            notify = new FakeNotificationService();
            var mod = new EmergencyDispatchController(ctx, adapter, registry, notify, new EmergencyDispatchConfig());
            mod.Start();
            return mod;
        }

        private static void Dispatch_InOnlineSession_Blocked()
        {
            FakeGameContext ctx; FakeDispatchAdapter adapter; FakeCleanupAdapter cleanup; EntityOwnershipRegistry reg; FakeNotificationService notify;
            var mod = Create(out ctx, out adapter, out cleanup, out reg, out notify);
            ctx.IsOnlineSession = true;

            Assert(!mod.RequestDispatch(EmergencyServiceType.Police, 1000), "Must never dispatch in Online.");
            Assert(adapter.SpawnCalls == 0, "Zero units should spawn in Online.");
        }

        private static void Dispatch_Ambulance_DoesNotHealBeforePhysicalArrival()
        {
            FakeGameContext ctx; FakeDispatchAdapter adapter; FakeCleanupAdapter cleanup; EntityOwnershipRegistry reg; FakeNotificationService notify;
            var mod = Create(out ctx, out adapter, out cleanup, out reg, out notify);

            Assert(mod.RequestDispatch(EmergencyServiceType.Ambulance, 1000), "Expected dispatch to succeed.");
            adapter.UnitPosition = new WorldPoint(80f, 0f, 0f); // Still 80m away
            mod.SlowTick(2000);

            Assert(adapter.HealCalls == 0, "Ambulance must NOT heal player before physical arrival.");
            Assert(mod.CurrentState == DispatchState.EnRoute, "Ambulance should remain EnRoute.");
        }

        private static void Dispatch_Ambulance_HealsOnlyOnArrivalThenDepartsAndCleansUp()
        {
            FakeGameContext ctx; FakeDispatchAdapter adapter; FakeCleanupAdapter cleanup; EntityOwnershipRegistry reg; FakeNotificationService notify;
            var mod = Create(out ctx, out adapter, out cleanup, out reg, out notify);

            mod.RequestDispatch(EmergencyServiceType.Ambulance, 1000);
            adapter.UnitPosition = new WorldPoint(5f, 0f, 0f); // Within 15m arrival radius
            mod.SlowTick(5000);

            Assert(adapter.HealCalls == 1, "Ambulance must heal on arrival.");
            Assert(adapter.DepartCalls == 1, "Ambulance must depart after healing.");
            Assert(mod.CurrentState == DispatchState.Departing, "Expected Departing state.");

            mod.SlowTick(5250);
            Assert(mod.CurrentState == DispatchState.Idle, "Expected Idle after cleanup.");
            Assert(reg.TrackedEntityCount == 0 && reg.TrackedBlipCount == 0, "All tracked entities and blips must be released.");
            Assert(cleanup.ReleasedEntities.Count == 2 && cleanup.DeletedBlips.Count == 1,
                "Expected departing unit entities released (not popped out of existence) and blip deleted.");
        }

        private static void Dispatch_Police_EngagesHostilesOnArrival()
        {
            FakeGameContext ctx; FakeDispatchAdapter adapter; FakeCleanupAdapter cleanup; EntityOwnershipRegistry reg; FakeNotificationService notify;
            var mod = Create(out ctx, out adapter, out cleanup, out reg, out notify);

            mod.RequestDispatch(EmergencyServiceType.Police, 1000);
            adapter.UnitPosition = new WorldPoint(10f, 0f, 0f);
            mod.SlowTick(3000);

            Assert(adapter.EngageHostilesCalls == 1, "Police must engage nearby hostiles on arrival.");
        }

        private static void Dispatch_Fire_AttemptsExtinguishAndHandlesUnsupportedGracefully()
        {
            FakeGameContext ctx; FakeDispatchAdapter adapter; FakeCleanupAdapter cleanup; EntityOwnershipRegistry reg; FakeNotificationService notify;
            var mod = Create(out ctx, out adapter, out cleanup, out reg, out notify);
            adapter.FireExtinguishSupported = false;

            mod.RequestDispatch(EmergencyServiceType.FireDepartment, 1000);
            adapter.UnitPosition = new WorldPoint(10f, 0f, 0f);
            mod.SlowTick(3000);

            Assert(adapter.ExtinguishCalls == 1, "Fire Department should attempt fire suppression.");
            Assert(notify.Notifications.Exists(n => n.Contains("unavailable")),
                "Should gracefully notify when auto-extinguish is unsupported.");
        }

        private static void Dispatch_DoesNotSpamDriveTaskUnlessPlayerMovesPastThreshold()
        {
            FakeGameContext ctx; FakeDispatchAdapter adapter; FakeCleanupAdapter cleanup; EntityOwnershipRegistry reg; FakeNotificationService notify;
            var mod = Create(out ctx, out adapter, out cleanup, out reg, out notify);

            mod.RequestDispatch(EmergencyServiceType.Police, 1000);
            Assert(adapter.DriveTaskCalls == 1, "Initial dispatch issues 1 drive task.");

            // Player moves only 5m (< 25m threshold) across multiple ticks
            adapter.PlayerPosition = new WorldPoint(5f, 0f, 0f);
            mod.SlowTick(2000);
            mod.SlowTick(3000);
            mod.SlowTick(4000);
            Assert(adapter.DriveTaskCalls == 1, "Must NOT re-task drive when player barely moved (resets AI pathing).");

            // Player moves 40m (>= 25m threshold)
            adapter.PlayerPosition = new WorldPoint(40f, 0f, 0f);
            mod.SlowTick(5000);
            Assert(adapter.DriveTaskCalls == 2, "Expected re-task once player moved past threshold.");
        }

        private static void Dispatch_Timeout_CleansUpEntitiesAndBlips()
        {
            FakeGameContext ctx; FakeDispatchAdapter adapter; FakeCleanupAdapter cleanup; EntityOwnershipRegistry reg; FakeNotificationService notify;
            var mod = Create(out ctx, out adapter, out cleanup, out reg, out notify);

            mod.RequestDispatch(EmergencyServiceType.Ambulance, 1000);
            mod.SlowTick(62000); // Exceeds 60,000ms timeout

            Assert(mod.CurrentState == DispatchState.Idle, "Expected Idle after timeout.");
            Assert(reg.TrackedEntityCount == 0 && reg.TrackedBlipCount == 0, "Timeout must leak zero entities or blips.");
            Assert(cleanup.DeletedEntities.Count == 2 && cleanup.DeletedBlips.Count == 1,
                "Expected timed-out entities and blip deleted.");
        }

        private static void Dispatch_ResponderDiesEnRoute_AbortsAndCleansUp()
        {
            FakeGameContext ctx; FakeDispatchAdapter adapter; FakeCleanupAdapter cleanup; EntityOwnershipRegistry reg; FakeNotificationService notify;
            var mod = Create(out ctx, out adapter, out cleanup, out reg, out notify);

            mod.RequestDispatch(EmergencyServiceType.Police, 1000);
            adapter.UnitAlive = false;
            mod.SlowTick(2500);

            Assert(mod.CurrentState == DispatchState.Idle, "Expected Idle after responder death.");
            Assert(reg.TrackedEntityCount == 0 && reg.TrackedBlipCount == 0, "Dead responder cleanup must leave zero leaks.");
        }

        private static void Dispatch_EnforcesSingleActiveDispatchAndCooldown()
        {
            FakeGameContext ctx; FakeDispatchAdapter adapter; FakeCleanupAdapter cleanup; EntityOwnershipRegistry reg; FakeNotificationService notify;
            var mod = Create(out ctx, out adapter, out cleanup, out reg, out notify);

            Assert(mod.RequestDispatch(EmergencyServiceType.Ambulance, 1000), "First dispatch should succeed.");
            Assert(!mod.RequestDispatch(EmergencyServiceType.Police, 2000), "Second concurrent dispatch must be blocked.");

            // Complete first dispatch quickly at t=3000 (cooldown lasts until t=11000)
            adapter.UnitPosition = new WorldPoint(2f, 0f, 0f);
            mod.SlowTick(3000);
            mod.SlowTick(3250);

            Assert(!mod.RequestDispatch(EmergencyServiceType.Police, 5000), "Dispatch inside cooldown window must be blocked.");
            Assert(mod.RequestDispatch(EmergencyServiceType.Police, 12000), "Dispatch after cooldown must succeed.");
        }

        private static void Dispatch_StopAndDispose_CleansUpOwnedEntitiesAndBlips()
        {
            FakeGameContext ctx; FakeDispatchAdapter adapter; FakeCleanupAdapter cleanup; EntityOwnershipRegistry reg; FakeNotificationService notify;
            var mod = Create(out ctx, out adapter, out cleanup, out reg, out notify);

            mod.RequestDispatch(EmergencyServiceType.FireDepartment, 1000);
            mod.Dispose();

            Assert(mod.CurrentState == DispatchState.Idle, "Dispose must return state to Idle.");
            Assert(reg.TrackedEntityCount == 0 && reg.TrackedBlipCount == 0, "Dispose must clean up all entities and blips.");
        }
    }
}
