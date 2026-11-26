using System;
using System.Collections.Generic;
using ClassicGTA.Contracts;
using ClassicGTA.Core.Activities;
using ClassicGTA.Infrastructure;

namespace ClassicGTA.Tests
{
    internal sealed class FakeSideJobAdapter : IEmergencySideJobAdapter
    {
        public bool InEmergencyVehicle { get; set; }
        public EmergencyServiceType VehicleType { get; set; }
        public int VehicleHandle { get; set; }
        public bool TargetResolved { get; set; }
        private int _nextHandle = 1200;

        public FakeSideJobAdapter()
        {
            InEmergencyVehicle = true;
            VehicleType = EmergencyServiceType.Police;
            VehicleHandle = 1100;
        }

        public bool IsPlayerInEmergencyVehicle(out EmergencyServiceType jobType, out int vehicleHandle)
        {
            jobType = VehicleType;
            vehicleHandle = InEmergencyVehicle ? VehicleHandle : 0;
            return InEmergencyVehicle;
        }

        public WorldPoint GetPlayerPosition() { return new WorldPoint(0f, 0f, 0f); }

        public bool TrySpawnWaveTarget(
            EmergencyServiceType jobType,
            int level,
            out int targetHandle,
            out WorldPoint location,
            out int blipHandle)
        {
            targetHandle = _nextHandle++;
            blipHandle = _nextHandle++;
            location = new WorldPoint(100f, 0f, 0f);
            return true;
        }

        public bool IsTargetResolved(EmergencyServiceType jobType, int targetHandle, WorldPoint playerPosition)
        {
            return TargetResolved;
        }

        public bool IsEntityAlive(int handle) { return true; }
    }

    internal sealed class FakeFirstPersonAdapter : IFirstPersonCameraAdapter
    {
        private int _nextCam = 300;
        public float DeltaYaw { get; set; }
        public float DeltaPitch { get; set; }
        public int CreateCalls { get; private set; }
        public bool LastCreatedInVehicle { get; private set; }
        public float LastPitch { get; private set; }
        public float LastYaw { get; private set; }
        public int SyncHeadingCalls { get; private set; }

        public int CreateAttachedCamera(bool inVehicle)
        {
            CreateCalls++;
            LastCreatedInVehicle = inVehicle;
            return _nextCam++;
        }

        public void ActivateCamera(int cameraHandle) { }

        public void ReadLookDelta(out float deltaYawDegrees, out float deltaPitchDegrees)
        {
            deltaYawDegrees = DeltaYaw;
            deltaPitchDegrees = DeltaPitch;
        }

        public void UpdateCameraLook(int cameraHandle, float pitchDegrees, float yawDegrees, bool inVehicle)
        {
            LastPitch = pitchDegrees;
            LastYaw = yawDegrees;
        }

        public void SyncPlayerHeading(float yawDegrees) { SyncHeadingCalls++; }
    }

    internal sealed class FakeCompanionAdapter : ICompanionAdapter
    {
        public bool CompanionAlive { get; set; }
        public int FollowCalls { get; private set; }
        public int CombatCalls { get; private set; }
        public int DismissCalls { get; private set; }
        private int _nextHandle = 1500;

        public FakeCompanionAdapter() { CompanionAlive = true; }
        public WorldPoint GetPlayerPosition() { return new WorldPoint(0f, 0f, 0f); }

        public bool TrySpawnCompanion(string companionId, WorldPoint nearPlayer, out int pedHandle, out int blipHandle)
        {
            pedHandle = _nextHandle++;
            blipHandle = _nextHandle++;
            return true;
        }

        public bool IsEntityAlive(int handle) { return CompanionAlive; }
        public void TaskFollowPlayer(int pedHandle) { FollowCalls++; }
        public void TaskAssistInCombat(int pedHandle) { CombatCalls++; }
        public void TaskDismiss(int pedHandle) { DismissCalls++; }
    }

    internal static class Phase5BTests
    {
        public static IEnumerable<KeyValuePair<string, Action>> GetAll()
        {
            yield return Pair("59_EmergencyJobs_EscalatesToLevel12AndPaysMasteryBonus", EmergencyJobs_EscalatesToLevel12AndPaysMasteryBonus);
            yield return Pair("60_EmergencyJobs_ExitingVehicleAbortsWithoutDeletingBorrowedVehicle", EmergencyJobs_ExitingVehicleAbortsWithoutDeletingBorrowedVehicle);
            yield return Pair("61_EmergencyJobs_WaveTimeoutAbortsAndCleansOwnedTargets", EmergencyJobs_WaveTimeoutAbortsAndCleansOwnedTargets);
            yield return Pair("62_EmergencyJobs_BlockedInOnlineSession", EmergencyJobs_BlockedInOnlineSession);
            yield return Pair("63_FirstPerson_ToggleClampsPitchToPlusMinus75AndSyncsHeadingOnFoot", FirstPerson_ToggleClampsPitchToPlusMinus75AndSyncsHeadingOnFoot);
            yield return Pair("64_FirstPerson_CutsceneRestoresRenderingCameraBeforeDeletingCamera", FirstPerson_CutsceneRestoresRenderingCameraBeforeDeletingCamera);
            yield return Pair("65_FirstPerson_SwitchesBetweenFootAndVehicleCameraCleanly", FirstPerson_SwitchesBetweenFootAndVehicleCameraCleanly);
            yield return Pair("66_FirstPerson_BlockedInOnlineSession", FirstPerson_BlockedInOnlineSession);
            yield return Pair("67_Companion_CallFollowCombatAssistAndNaturalDismiss", Companion_CallFollowCombatAssistAndNaturalDismiss);
            yield return Pair("68_Companion_SingleActiveCapBlocksSecondCall", Companion_SingleActiveCapBlocksSecondCall);
            yield return Pair("69_Companion_DeathEndsHangoutAndCleansUpBlipAndEntity", Companion_DeathEndsHangoutAndCleansUpBlipAndEntity);
            yield return Pair("70_Companion_OnlineSessionEndsHangoutImmediately", Companion_OnlineSessionEndsHangoutImmediately);
        }

        private static KeyValuePair<string, Action> Pair(string name, Action body)
        {
            return new KeyValuePair<string, Action>(name, body);
        }

        private static void Assert(bool cond, string msg)
        {
            if (!cond) throw new InvalidOperationException(msg);
        }

        private static void EmergencyJobs_EscalatesToLevel12AndPaysMasteryBonus()
        {
            var adapter = new FakeSideJobAdapter();
            var econ = new FakeEconomyService(0);
            var cleanup = new FakeCleanupAdapter();
            var reg = new EntityOwnershipRegistry(cleanup);
            var job = new EmergencySideJobController(new FakeGameContext(), adapter, econ, reg, new FakeNotificationService());
            job.Start();

            Assert(job.StartJob(1000) && job.CurrentLevel == 1, "Expected job start at Level 1.");

            int expectedCash = 0;
            adapter.TargetResolved = true;
            for (int lvl = 1; lvl <= 12; lvl++)
            {
                expectedCash += 50 * lvl * lvl;
                job.SlowTick(1000 + lvl * 2000);
            }
            expectedCash += EmergencySideJobController.MasteryBonusCash;

            Assert(!job.IsJobActive && job.CurrentLevel == 0, "Job should finish after Level 12.");
            Assert(econ.Cash == expectedCash, "Expected total $" + expectedCash + ", got $" + econ.Cash);
            Assert(!cleanup.DeletedEntities.Contains(1100), "Borrowed emergency vehicle must never be deleted.");
        }

        private static void EmergencyJobs_ExitingVehicleAbortsWithoutDeletingBorrowedVehicle()
        {
            var adapter = new FakeSideJobAdapter();
            var cleanup = new FakeCleanupAdapter();
            var reg = new EntityOwnershipRegistry(cleanup);
            var job = new EmergencySideJobController(new FakeGameContext(), adapter, new FakeEconomyService(0), reg, new FakeNotificationService());
            job.Start();
            job.StartJob(1000);

            adapter.InEmergencyVehicle = false;
            job.SlowTick(2000);

            Assert(!job.IsJobActive, "Exiting vehicle must abort job.");
            Assert(!cleanup.DeletedEntities.Contains(1100) && !cleanup.ReleasedEntities.Contains(1100),
                "Borrowed emergency vehicle (1100) MUST NEVER be deleted or released.");
            Assert(reg.TrackedEntityCount == 0 && reg.TrackedBlipCount == 0, "Owned wave entities/blips must be cleaned up.");
        }

        private static void EmergencyJobs_WaveTimeoutAbortsAndCleansOwnedTargets()
        {
            var adapter = new FakeSideJobAdapter();
            var cleanup = new FakeCleanupAdapter();
            var reg = new EntityOwnershipRegistry(cleanup);
            var job = new EmergencySideJobController(new FakeGameContext(), adapter, new FakeEconomyService(0), reg, new FakeNotificationService());
            job.Start();
            job.StartJob(1000);

            // Level 1 wave timeout is 65000ms
            job.SlowTick(70000);
            Assert(!job.IsJobActive && reg.TrackedEntityCount == 0 && reg.TrackedBlipCount == 0,
                "Timeout must abort and leave zero leaks.");
        }

        private static void EmergencyJobs_BlockedInOnlineSession()
        {
            var ctx = new FakeGameContext { IsOnlineSession = true };
            var job = new EmergencySideJobController(ctx, new FakeSideJobAdapter(), new FakeEconomyService(0), new EntityOwnershipRegistry(new FakeCleanupAdapter()), new FakeNotificationService());
            job.Start();
            Assert(!job.StartJob(1000), "Must never start emergency side-jobs in Online.");
        }

        private static void FirstPerson_ToggleClampsPitchToPlusMinus75AndSyncsHeadingOnFoot()
        {
            var ctx = new FakeGameContext { IsInVehicle = false };
            var adapter = new FakeFirstPersonAdapter { DeltaPitch = 120f, DeltaYaw = 45f };
            var cleanup = new FakeCleanupAdapter();
            var reg = new EntityOwnershipRegistry(cleanup);
            var fp = new FirstPersonCameraController(ctx, adapter, reg, new FakeNotificationService());
            fp.Start();

            fp.OnKeyPressed(GameKey.F11);
            Assert(fp.IsFirstPersonActive, "F11 should activate First-Person camera.");

            fp.FastTick();
            Assert(fp.CurrentPitchDegrees == 75f, "Pitch must clamp at +75 degrees, got " + fp.CurrentPitchDegrees);
            Assert(adapter.SyncHeadingCalls == 1, "On-foot mode must sync player heading.");

            adapter.DeltaPitch = -300f;
            fp.FastTick();
            Assert(fp.CurrentPitchDegrees == -75f, "Pitch must clamp at -75 degrees, got " + fp.CurrentPitchDegrees);
        }

        private static void FirstPerson_CutsceneRestoresRenderingCameraBeforeDeletingCamera()
        {
            var ctx = new FakeGameContext();
            var adapter = new FakeFirstPersonAdapter();
            var cleanup = new FakeCleanupAdapter();
            var reg = new EntityOwnershipRegistry(cleanup);
            var fp = new FirstPersonCameraController(ctx, adapter, reg, new FakeNotificationService());
            fp.Start();
            fp.ToggleFirstPerson();

            ctx.IsInCutscene = true;
            fp.FastTick();

            Assert(!fp.IsFirstPersonActive && reg.TrackedCameraCount == 0, "Cutscene must deactivate and untrack camera.");
            int restoreIdx = cleanup.CallLog.IndexOf("RestoreRenderingCamera");
            int deleteIdx = cleanup.CallLog.IndexOf("DeleteCamera:300");
            Assert(restoreIdx >= 0 && deleteIdx > restoreIdx, "RestoreRenderingCamera MUST execute before DeleteCamera.");
        }

        private static void FirstPerson_SwitchesBetweenFootAndVehicleCameraCleanly()
        {
            var ctx = new FakeGameContext { IsInVehicle = false };
            var adapter = new FakeFirstPersonAdapter();
            var cleanup = new FakeCleanupAdapter();
            var reg = new EntityOwnershipRegistry(cleanup);
            var fp = new FirstPersonCameraController(ctx, adapter, reg, new FakeNotificationService());
            fp.Start();
            fp.ToggleFirstPerson();
            Assert(!adapter.LastCreatedInVehicle, "Expected on-foot camera initially.");

            ctx.IsInVehicle = true;
            fp.FastTick();
            Assert(adapter.CreateCalls == 2 && adapter.LastCreatedInVehicle && reg.TrackedCameraCount == 1,
                "Expected clean switch to vehicle camera with old camera cleaned up.");
            Assert(adapter.SyncHeadingCalls == 0, "Vehicle mode must not overwrite ped heading directly.");
        }

        private static void FirstPerson_BlockedInOnlineSession()
        {
            var ctx = new FakeGameContext { IsOnlineSession = true };
            var fp = new FirstPersonCameraController(ctx, new FakeFirstPersonAdapter(), new EntityOwnershipRegistry(new FakeCleanupAdapter()), new FakeNotificationService());
            fp.Start();
            Assert(!fp.ToggleFirstPerson() && !fp.IsFirstPersonActive, "First-Person camera must be blocked in Online.");
        }

        private static void Companion_CallFollowCombatAssistAndNaturalDismiss()
        {
            var adapter = new FakeCompanionAdapter();
            var cleanup = new FakeCleanupAdapter();
            var reg = new EntityOwnershipRegistry(cleanup);
            var hangout = new CompanionHangoutController(new FakeGameContext(), adapter, reg, new FakeNotificationService());
            hangout.Start();

            Assert(hangout.CallCompanion("buddy.lamar") && hangout.CurrentState == HangoutState.Following, "Expected Following.");
            Assert(adapter.FollowCalls == 1, "Expected follow task.");

            Assert(hangout.CommandCombatAssist() && hangout.CurrentState == HangoutState.CombatAssist, "Expected CombatAssist.");
            Assert(adapter.CombatCalls == 1, "Expected combat assist task.");

            hangout.DismissCompanion();
            Assert(hangout.CurrentState == HangoutState.Idle && adapter.DismissCalls == 1, "Expected natural dismiss.");
            Assert(cleanup.ReleasedEntities.Count == 1 && cleanup.DeletedEntities.Count == 0 && cleanup.DeletedBlips.Count == 1,
                "Dismissed companion should be released (not popped out of existence) and blip deleted.");
        }

        private static void Companion_SingleActiveCapBlocksSecondCall()
        {
            var reg = new EntityOwnershipRegistry(new FakeCleanupAdapter());
            var hangout = new CompanionHangoutController(new FakeGameContext(), new FakeCompanionAdapter(), reg, new FakeNotificationService());
            hangout.Start();

            Assert(hangout.CallCompanion("buddy.a"), "First companion call should succeed.");
            Assert(!hangout.CallCompanion("buddy.b"), "Second concurrent companion must be blocked.");
        }

        private static void Companion_DeathEndsHangoutAndCleansUpBlipAndEntity()
        {
            var adapter = new FakeCompanionAdapter();
            var cleanup = new FakeCleanupAdapter();
            var reg = new EntityOwnershipRegistry(cleanup);
            var hangout = new CompanionHangoutController(new FakeGameContext(), adapter, reg, new FakeNotificationService());
            hangout.Start();
            hangout.CallCompanion("buddy.a");

            adapter.CompanionAlive = false;
            hangout.SlowTick(2000);

            Assert(hangout.CurrentState == HangoutState.Idle && reg.TrackedEntityCount == 0 && reg.TrackedBlipCount == 0,
                "Companion death must end hangout and clean up.");
        }

        private static void Companion_OnlineSessionEndsHangoutImmediately()
        {
            var ctx = new FakeGameContext();
            var reg = new EntityOwnershipRegistry(new FakeCleanupAdapter());
            var hangout = new CompanionHangoutController(ctx, new FakeCompanionAdapter(), reg, new FakeNotificationService());
            hangout.Start();
            hangout.CallCompanion("buddy.a");

            ctx.IsOnlineSession = true;
            hangout.SlowTick(2000);
            Assert(hangout.CurrentState == HangoutState.Idle && reg.TrackedEntityCount == 0, "Online session must end hangout.");
        }
    }
}
