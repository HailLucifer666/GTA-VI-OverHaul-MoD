using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ClassicGTA.Adapter.GTAV;
using ClassicGTA.Contracts;
using ClassicGTA.Core.Arcade;
using ClassicGTA.Infrastructure;

namespace ClassicGTA.Tests
{
    internal sealed class FakeCarTheftAdapter : ICarTheftAdapter
    {
        public int TargetHandle { get; set; }
        public bool Locked { get; set; }
        public bool UnlockReturns { get; set; }

        public int GetTargetCalls { get; private set; }
        public int UnlockCalls { get; private set; }
        public int DisableAlarmCalls { get; private set; }
        public int PacifyDriverCalls { get; private set; }

        public FakeCarTheftAdapter()
        {
            TargetHandle = 4001;
            Locked = true;
            UnlockReturns = true;
        }

        public int GetTargetVehicleHandle() { GetTargetCalls++; return TargetHandle; }
        public bool IsVehicleLocked(int vehicleHandle) { return Locked; }
        public bool UnlockVehicle(int vehicleHandle)
        {
            UnlockCalls++;
            if (UnlockReturns) Locked = false;
            return UnlockReturns;
        }
        public void DisableAlarm(int vehicleHandle) { DisableAlarmCalls++; }
        public void PacifyDriver(int vehicleHandle) { PacifyDriverCalls++; }
    }

    internal sealed class FakeWantedSystemAdapter : IWantedSystemAdapter
    {
        public int WantedLevel { get; set; }
        public bool PoliceSpotting { get; set; }
        public int MaxWantedLevel { get; private set; }
        public int GetWantedCalls { get; private set; }
        public int SetWantedCalls { get; private set; }
        public int ClearHeatCalls { get; private set; }

        public int GetWantedLevel() { GetWantedCalls++; return WantedLevel; }
        public void SetWantedLevel(int level) { SetWantedCalls++; WantedLevel = level; }
        public bool ArePoliceSpottingPlayer() { return PoliceSpotting; }
        public void ClearCriminalProfileHeat() { ClearHeatCalls++; }
        public void SetMaxWantedLevel(int maxLevel) { MaxWantedLevel = maxLevel; }
    }

    internal sealed class FakeArcadeFreedomAdapter : IArcadeFreedomAdapter
    {
        public int TopUpFuelCalls { get; private set; }
        public int RestoreVitalsCalls { get; private set; }
        public int VignetteCalls { get; private set; }
        public bool LastVignetteSuppressed { get; private set; }

        public void TopUpVehicleFuelAndHealth() { TopUpFuelCalls++; }
        public void RestorePlayerStaminaAndVitals() { RestoreVitalsCalls++; }
        public void SetVignetteSuppressed(bool suppressed) { VignetteCalls++; LastVignetteSuppressed = suppressed; }
    }

    internal sealed class FakeSandboxCheatAdapter : ISandboxCheatAdapter
    {
        public int RefillCalls { get; private set; }
        public int ClearWantedCalls { get; private set; }
        public int NeverWantedCalls { get; private set; }
        public bool LastNeverWanted { get; private set; }
        public int SuperJumpCalls { get; private set; }
        public int RepairCalls { get; private set; }
        public int SpawnCalls { get; private set; }
        public string LastSpawnedModel { get; private set; }
        public int WeatherCalls { get; private set; }
        public string LastWeather { get; private set; }
        public int AdvanceTimeCalls { get; private set; }
        public int NextVehicleHandle { get; set; }

        public FakeSandboxCheatAdapter()
        {
            NextVehicleHandle = 7001;
        }

        public void RefillHealthAndArmor() { RefillCalls++; }
        public void ClearWantedLevel() { ClearWantedCalls++; }
        public void SetNeverWanted(bool enabled) { NeverWantedCalls++; LastNeverWanted = enabled; }
        public void ApplySuperJumpFrame() { SuperJumpCalls++; }
        public bool RepairCurrentVehicle() { RepairCalls++; return true; }
        public int SpawnClassicVehicle(string modelId) { SpawnCalls++; LastSpawnedModel = modelId; return NextVehicleHandle++; }
        public void SetWeather(string weatherId) { WeatherCalls++; LastWeather = weatherId; }
        public void AdvanceTimeHours(int hours) { AdvanceTimeCalls++; }
    }

    internal sealed class FakeTerritoryWarAdapter : ITerritoryWarAdapter
    {
        public int NextEnemyHandle { get; set; }
        public int NextBlipHandle { get; set; }
        public int SpawnEnemyCalls { get; private set; }
        public int CreateBlipCalls { get; private set; }
        public HashSet<int> AliveEnemies { get; private set; }

        public FakeTerritoryWarAdapter()
        {
            NextEnemyHandle = 8001;
            NextBlipHandle = 9501;
            AliveEnemies = new HashSet<int>();
        }

        public int SpawnWaveEnemy(string zoneId, int waveNumber)
        {
            SpawnEnemyCalls++;
            int h = NextEnemyHandle++;
            AliveEnemies.Add(h);
            return h;
        }

        public bool IsEntityAlive(int entityHandle)
        {
            return AliveEnemies.Contains(entityHandle);
        }

        public int CreateZoneBlip(string zoneId)
        {
            CreateBlipCalls++;
            return NextBlipHandle++;
        }
    }

    internal static class Phase7ArcadeTests
    {
        public static IEnumerable<KeyValuePair<string, Action>> GetAll()
        {
            yield return Pair("117_CarTheft_UnlocksDisablesAlarmPacifiesDriver_AndAvoidsDuplicateSpam", CarTheft_UnlocksDisablesAlarmPacifiesDriver_AndAvoidsDuplicateSpam);
            yield return Pair("118_CarTheft_InOnlineSession_PerformsZeroAdapterCalls", CarTheft_InOnlineSession_PerformsZeroAdapterCalls);
            yield return Pair("119_CarTheft_DuringCutsceneOrPause_PerformsZeroAdapterCalls", CarTheft_DuringCutsceneOrPause_PerformsZeroAdapterCalls);
            yield return Pair("120_Wanted_LosCooldownClearsStarsAndHeat_AndSpottedResetsTimer", Wanted_LosCooldownClearsStarsAndHeat_AndSpottedResetsTimer);
            yield return Pair("121_Wanted_PausedOrCutscenePausesTimerWithoutReset_AndWrapAroundSafe", Wanted_PausedOrCutscenePausesTimerWithoutReset_AndWrapAroundSafe);
            yield return Pair("122_Wanted_PayNSprayBribeDeductsCashAndClearsStarsAndHeat", Wanted_PayNSprayBribeDeductsCashAndClearsStarsAndHeat);
            yield return Pair("123_Wanted_PayNSprayBribe_InsufficientFundsRejectedStarsUnchanged", Wanted_PayNSprayBribe_InsufficientFundsRejectedStarsUnchanged);
            yield return Pair("124_Wanted_InOnlineSession_PerformsZeroAdapterCalls", Wanted_InOnlineSession_PerformsZeroAdapterCalls);
            yield return Pair("125_ArcadeFreedom_RespectsIndependentToggles_AndStopRestoresVignette", ArcadeFreedom_RespectsIndependentToggles_AndStopRestoresVignette);
            yield return Pair("126_ArcadeFreedom_InOnlineSession_PerformsZeroAdapterCalls", ArcadeFreedom_InOnlineSession_PerformsZeroAdapterCalls);
            yield return Pair("127_ArcadeFreedom_DuringCutsceneOrPause_PerformsZeroAdapterCalls", ArcadeFreedom_DuringCutsceneOrPause_PerformsZeroAdapterCalls);
            yield return Pair("128_Cheats_StoryModeActionsWork_AndEmptyOrUnknownModelWeatherRejected", Cheats_StoryModeActionsWork_AndEmptyOrUnknownModelWeatherRejected);
            yield return Pair("129_Cheats_InOnlineSession_HardBlocksEveryCheatActionAndTick", Cheats_InOnlineSession_HardBlocksEveryCheatActionAndTick);
            yield return Pair("130_Cheats_RegistryBudgetFull_DeletesUntrackedVehicleAndReturnsFalse", Cheats_RegistryBudgetFull_DeletesUntrackedVehicleAndReturnsFalse);
            yield return Pair("131_Cheats_SuperJumpFastTick_RespectsCutsceneAndPause", Cheats_SuperJumpFastTick_RespectsCutsceneAndPause);
            yield return Pair("132_TurfWar_ThreeWaveCapturePersistsAndReloads_AndDuplicateRejected", TurfWar_ThreeWaveCapturePersistsAndReloads_AndDuplicateRejected);
            yield return Pair("133_TurfWar_CorruptPrimaryAndBak1Rejected_GoodBak2Recovered", TurfWar_CorruptPrimaryAndBak1Rejected_GoodBak2Recovered);
            yield return Pair("134_TurfWar_OnlineTransitionAbortsWithoutDeleting_AndPersistFailureRollsBack", TurfWar_OnlineTransitionAbortsWithoutDeleting_AndPersistFailureRollsBack);
            yield return Pair("135_TurfWar_PassiveIncomeCapsAt7Days_AndRejectsBackwardClock", TurfWar_PassiveIncomeCapsAt7Days_AndRejectsBackwardClock);
            yield return Pair("136_GateD_AllRegisteredSchedulerModules_RespectIsOnlineSession", GateD_AllRegisteredSchedulerModules_RespectIsOnlineSession);
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

        // 117
        private static void CarTheft_UnlocksDisablesAlarmPacifiesDriver_AndAvoidsDuplicateSpam()
        {
            var ctx = new FakeGameContext();
            var adapter = new FakeCarTheftAdapter();
            var notify = new FakeNotificationService();
            var mod = new ClassicCarTheftController(ctx, adapter, notify);
            mod.Start();

            mod.SlowTick(1000);
            Assert(adapter.UnlockCalls == 1 && adapter.DisableAlarmCalls == 1 && adapter.PacifyDriverCalls == 1,
                "Expected unlock + disable alarm + pacify driver on first tick.");
            Assert(mod.VehiclesUnlockedCount == 1, "Expected VehiclesUnlockedCount == 1.");

            // Repeated ticks on same handle must not spam natives even if lock toggled
            adapter.Locked = true;
            mod.SlowTick(1250);
            mod.SlowTick(1500);
            Assert(adapter.UnlockCalls == 1, "Must not spam UnlockVehicle on the same vehicle handle.");
        }

        // 118 (Probe P13)
        private static void CarTheft_InOnlineSession_PerformsZeroAdapterCalls()
        {
            var ctx = new FakeGameContext { IsOnlineSession = true };
            var adapter = new FakeCarTheftAdapter();
            var mod = new ClassicCarTheftController(ctx, adapter, new FakeNotificationService());
            mod.Start();

            mod.SlowTick(1000);
            Assert(adapter.GetTargetCalls == 0 && adapter.UnlockCalls == 0,
                "CarTheft must perform zero adapter calls in Online session.");
        }

        // 119
        private static void CarTheft_DuringCutsceneOrPause_PerformsZeroAdapterCalls()
        {
            var ctx = new FakeGameContext { IsInCutscene = true };
            var adapter = new FakeCarTheftAdapter();
            var mod = new ClassicCarTheftController(ctx, adapter, new FakeNotificationService());
            mod.Start();

            mod.SlowTick(1000);
            ctx.IsInCutscene = false;
            ctx.IsPaused = true;
            mod.SlowTick(1250);

            Assert(adapter.GetTargetCalls == 0 && adapter.UnlockCalls == 0,
                "CarTheft must not act during cutscene or pause.");
        }

        // 120
        private static void Wanted_LosCooldownClearsStarsAndHeat_AndSpottedResetsTimer()
        {
            var ctx = new FakeGameContext();
            var adapter = new FakeWantedSystemAdapter { WantedLevel = 3, PoliceSpotting = false };
            var econ = new FakeEconomyService(1000);
            var mod = new ClassicWantedController(ctx, adapter, econ, new FakeNotificationService());
            mod.CooldownSeconds = 20;
            mod.Start();

            mod.SlowTick(1000);  // Anchor hidden timer
            mod.SlowTick(11000); // 10s hidden
            Assert(adapter.WantedLevel == 3 && mod.AccumulatedHiddenMs == 10000, "Should still be wanted at 10s.");

            // Police spot player -> resets hidden timer
            adapter.PoliceSpotting = true;
            mod.SlowTick(12000);
            Assert(mod.AccumulatedHiddenMs == 0, "Police spotting player must reset hidden timer.");

            // Hide again for 20s -> clears stars + Criminal Profile Heat
            adapter.PoliceSpotting = false;
            mod.SlowTick(13000); // Anchor
            mod.SlowTick(33000); // +20s
            Assert(adapter.WantedLevel == 0 && adapter.ClearHeatCalls == 1,
                "Expected wanted cleared and Criminal Profile Heat cleared after 20s out of sight.");
        }

        // 121
        private static void Wanted_PausedOrCutscenePausesTimerWithoutReset_AndWrapAroundSafe()
        {
            var ctx = new FakeGameContext();
            var adapter = new FakeWantedSystemAdapter { WantedLevel = 2, PoliceSpotting = false };
            var mod = new ClassicWantedController(ctx, adapter, new FakeEconomyService(0), new FakeNotificationService());
            mod.CooldownSeconds = 20;
            mod.Start();

            mod.SlowTick(50000); // Anchor
            mod.SlowTick(60000); // +10s hidden
            Assert(mod.AccumulatedHiddenMs == 10000, "Expected 10s accumulated.");

            // Pause for 30s -> must NOT advance or reset accumulated hidden time
            ctx.IsPaused = true;
            mod.SlowTick(90000);
            Assert(mod.AccumulatedHiddenMs == 10000 && adapter.WantedLevel == 2,
                "Paused must preserve accumulated hidden time without resetting or clearing.");

            // Unpause + GameTimeMs wraps backward to 100ms -> must re-anchor cleanly without freezing
            ctx.IsPaused = false;
            mod.SlowTick(100);   // Re-anchor
            mod.SlowTick(10100); // +10s -> total 20s -> cleared!
            Assert(adapter.WantedLevel == 0 && adapter.ClearHeatCalls == 1,
                "Expected wrap-around safe completion after pause.");
        }

        // 122
        private static void Wanted_PayNSprayBribeDeductsCashAndClearsStarsAndHeat()
        {
            var ctx = new FakeGameContext();
            var adapter = new FakeWantedSystemAdapter { WantedLevel = 4 };
            var econ = new FakeEconomyService(1200);
            var mod = new ClassicWantedController(ctx, adapter, econ, new FakeNotificationService());
            mod.Start();

            Assert(mod.TryPayNSprayBribe(), "Pay 'n' Spray bribe should succeed.");
            Assert(econ.Cash == 700, "Expected $500 deducted from Mod Wallet.");
            Assert(adapter.WantedLevel == 0 && adapter.ClearHeatCalls == 1, "Bribe must clear wanted stars and Heat.");
        }

        // 123 (Probe P14)
        private static void Wanted_PayNSprayBribe_InsufficientFundsRejectedStarsUnchanged()
        {
            var ctx = new FakeGameContext();
            var adapter = new FakeWantedSystemAdapter { WantedLevel = 3 };
            var econ = new FakeEconomyService(200); // < $500 default bribe
            var mod = new ClassicWantedController(ctx, adapter, econ, new FakeNotificationService());
            mod.Start();

            Assert(!mod.TryPayNSprayBribe(), "Bribe must be rejected when cash < BribeCost.");
            Assert(adapter.WantedLevel == 3 && adapter.ClearHeatCalls == 0 && econ.Cash == 200,
                "Rejected bribe must leave stars, Heat, and cash untouched.");
        }

        // 124
        private static void Wanted_InOnlineSession_PerformsZeroAdapterCalls()
        {
            var ctx = new FakeGameContext { IsOnlineSession = true };
            var adapter = new FakeWantedSystemAdapter { WantedLevel = 3 };
            var econ = new FakeEconomyService(5000);
            var mod = new ClassicWantedController(ctx, adapter, econ, new FakeNotificationService());
            mod.Start();
            mod.SlowTick(1000);
            Assert(!mod.TryPayNSprayBribe(), "Bribe must be blocked in Online.");
            Assert(adapter.GetWantedCalls == 0 && adapter.SetWantedCalls == 0 && adapter.ClearHeatCalls == 0 && adapter.MaxWantedLevel == 0,
                "Wanted controller must perform zero adapter calls in Online.");
        }

        // 125
        private static void ArcadeFreedom_RespectsIndependentToggles_AndStopRestoresVignette()
        {
            var ctx = new FakeGameContext { IsInVehicle = true };
            var adapter = new FakeArcadeFreedomAdapter();
            var mod = new ArcadeFreedomController(ctx, adapter, new FakeNotificationService());
            mod.NoRefuellingEnabled = true;
            mod.NoSurvivalChoresEnabled = false;
            mod.NoVignetteEnabled = true;
            mod.Start();

            mod.SlowTick(1000);
            Assert(adapter.TopUpFuelCalls == 1 && adapter.RestoreVitalsCalls == 0,
                "Should top up fuel only when NoSurvivalChoresEnabled is false.");
            Assert(adapter.VignetteCalls == 1 && adapter.LastVignetteSuppressed,
                "Should suppress vignette when NoVignetteEnabled is true.");

            mod.Stop();
            Assert(adapter.VignetteCalls == 2 && !adapter.LastVignetteSuppressed,
                "Stop() must restore vignette cleanly.");
        }

        // 126 (Probe P15)
        private static void ArcadeFreedom_InOnlineSession_PerformsZeroAdapterCalls()
        {
            var ctx = new FakeGameContext { IsOnlineSession = true, IsInVehicle = true };
            var adapter = new FakeArcadeFreedomAdapter();
            var mod = new ArcadeFreedomController(ctx, adapter, new FakeNotificationService());
            mod.NoVignetteEnabled = true;
            mod.Start();

            mod.SlowTick(1000);
            mod.Stop();
            Assert(adapter.TopUpFuelCalls == 0 && adapter.RestoreVitalsCalls == 0 && adapter.VignetteCalls == 0,
                "ArcadeFreedom must perform zero adapter calls in Online.");
        }

        // 127
        private static void ArcadeFreedom_DuringCutsceneOrPause_PerformsZeroAdapterCalls()
        {
            var ctx = new FakeGameContext { IsInVehicle = true, IsInCutscene = true };
            var adapter = new FakeArcadeFreedomAdapter();
            var mod = new ArcadeFreedomController(ctx, adapter, new FakeNotificationService());
            mod.Start();

            mod.SlowTick(1000);
            ctx.IsInCutscene = false;
            ctx.IsPaused = true;
            mod.SlowTick(1250);
            Assert(adapter.TopUpFuelCalls == 0 && adapter.RestoreVitalsCalls == 0,
                "ArcadeFreedom must not act during cutscene or pause.");
        }

        // 128
        private static void Cheats_StoryModeActionsWork_AndEmptyOrUnknownModelWeatherRejected()
        {
            var ctx = new FakeGameContext();
            var adapter = new FakeSandboxCheatAdapter();
            var cleanup = new FakeCleanupAdapter();
            var reg = new EntityOwnershipRegistry(cleanup);
            var mod = new ClassicCheatEngine(ctx, adapter, reg, new FakeNotificationService());
            mod.Start();

            Assert(mod.RefillHealthAndArmor() && adapter.RefillCalls == 1, "RefillHealthAndArmor failed.");
            Assert(mod.ClearWanted() && adapter.ClearWantedCalls == 1, "ClearWanted failed.");
            Assert(mod.SetNeverWanted(true) && adapter.LastNeverWanted, "SetNeverWanted failed.");
            Assert(mod.RepairVehicle() && adapter.RepairCalls == 1, "RepairVehicle failed.");
            Assert(mod.TrySpawnClassicVehicle("  CHEETAH2 ") && adapter.LastSpawnedModel == "cheetah2" && reg.TrackedEntityCount == 1,
                "Allowlisted classic vehicle spawn failed.");
            Assert(mod.SetWeather(" thunder ") && adapter.LastWeather == "THUNDER", "Allowlisted weather failed.");
            Assert(mod.AdvanceTime(6) && adapter.AdvanceTimeCalls == 1, "AdvanceTime failed.");

            // Hostile / empty / unknown inputs rejected without adapter calls
            Assert(!mod.TrySpawnClassicVehicle("") && !mod.TrySpawnClassicVehicle("hydra_unknown") && adapter.SpawnCalls == 1,
                "Empty or unknown vehicle model must be rejected without calling adapter.");
            Assert(!mod.SetWeather("") && !mod.SetWeather("APOCALYPSE") && adapter.WeatherCalls == 1,
                "Empty or unknown weather ID must be rejected without calling adapter.");
            Assert(!mod.AdvanceTime(0) && !mod.AdvanceTime(25) && adapter.AdvanceTimeCalls == 1,
                "Out-of-range AdvanceTime hours must be rejected.");
        }

        // 129 (Probe P16)
        private static void Cheats_InOnlineSession_HardBlocksEveryCheatActionAndTick()
        {
            var ctx = new FakeGameContext { IsOnlineSession = true };
            var adapter = new FakeSandboxCheatAdapter();
            var reg = new EntityOwnershipRegistry(new FakeCleanupAdapter());
            var mod = new ClassicCheatEngine(ctx, adapter, reg, new FakeNotificationService());
            mod.SuperJumpEnabled = true;
            mod.Start();

            mod.FastTick();
            mod.SlowTick(1000);
            Assert(!mod.RefillHealthAndArmor(), "Refill must fail in Online.");
            Assert(!mod.ClearWanted(), "ClearWanted must fail in Online.");
            Assert(!mod.SetNeverWanted(true), "SetNeverWanted must fail in Online.");
            Assert(!mod.RepairVehicle(), "RepairVehicle must fail in Online.");
            Assert(!mod.TrySpawnClassicVehicle("cheetah2"), "Spawn must fail in Online.");
            Assert(!mod.SetWeather("CLEAR"), "SetWeather must fail in Online.");
            Assert(!mod.AdvanceTime(6), "AdvanceTime must fail in Online.");

            Assert(adapter.RefillCalls == 0 && adapter.ClearWantedCalls == 0 && adapter.NeverWantedCalls == 0 &&
                   adapter.SuperJumpCalls == 0 && adapter.RepairCalls == 0 && adapter.SpawnCalls == 0 &&
                   adapter.WeatherCalls == 0 && adapter.AdvanceTimeCalls == 0,
                "Zero cheat adapter calls permitted in Online.");
        }

        // 130
        private static void Cheats_RegistryBudgetFull_DeletesUntrackedVehicleAndReturnsFalse()
        {
            var ctx = new FakeGameContext();
            var adapter = new FakeSandboxCheatAdapter();
            var cleanup = new FakeCleanupAdapter();
            var tightReg = new EntityOwnershipRegistry(cleanup, 1);
            tightReg.TryTrackEntity(5555, "other", EntityOwnership.Owned);

            var mod = new ClassicCheatEngine(ctx, adapter, tightReg, new FakeNotificationService());
            mod.Start();

            Assert(!mod.TrySpawnClassicVehicle("cheetah2"), "Full entity budget must refuse spawn.");
            Assert(cleanup.DeletedEntities.Contains(7001), "Refused spawn must delete untracked vehicle handle.");
        }

        // 131
        private static void Cheats_SuperJumpFastTick_RespectsCutsceneAndPause()
        {
            var ctx = new FakeGameContext();
            var adapter = new FakeSandboxCheatAdapter();
            var reg = new EntityOwnershipRegistry(new FakeCleanupAdapter());
            var mod = new ClassicCheatEngine(ctx, adapter, reg, new FakeNotificationService());
            mod.SuperJumpEnabled = true;
            mod.Start();

            mod.FastTick();
            Assert(adapter.SuperJumpCalls == 1, "Expected SuperJump frame in normal Story Mode.");

            ctx.IsInCutscene = true;
            mod.FastTick();
            ctx.IsInCutscene = false;
            ctx.IsPaused = true;
            mod.FastTick();
            Assert(adapter.SuperJumpCalls == 1, "SuperJump must not fire during cutscene or pause.");
        }

        // 132
        private static void TurfWar_ThreeWaveCapturePersistsAndReloads_AndDuplicateRejected()
        {
            string dir = CreateTempDir();
            try
            {
                var ctx = new FakeGameContext();
                var adapter = new FakeTerritoryWarAdapter();
                var econ = new FakeEconomyService(1000);
                var cleanup = new FakeCleanupAdapter();
                var reg = new EntityOwnershipRegistry(cleanup);
                var notify = new FakeNotificationService();
                var store = new AtomicFileStore(dir, notify);

                var turf = new TerritoryWarController(ctx, adapter, econ, reg, store, notify, "slot_turf");
                turf.Start();

                Assert(turf.TryStartTakeover("turf.davis", 1000) == TerritoryStartStatus.Started, "Start takeover failed.");
                Assert(turf.CurrentWave == 1 && adapter.AliveEnemies.Count == 3, "Wave 1 should spawn 3 enemies.");

                // Clear Wave 1
                adapter.AliveEnemies.Clear();
                turf.SlowTick(2000);
                Assert(turf.CurrentWave == 2 && adapter.AliveEnemies.Count == 3, "Wave 2 should spawn 3 enemies.");

                // Clear Wave 2
                adapter.AliveEnemies.Clear();
                turf.SlowTick(3000);
                Assert(turf.CurrentWave == 3 && adapter.AliveEnemies.Count == 3, "Wave 3 should spawn 3 enemies.");

                // Clear Wave 3 -> Captured + persisted + $5,000 bonus
                adapter.AliveEnemies.Clear();
                turf.SlowTick(4000);
                Assert(turf.State == TerritoryWarState.Idle && turf.IsZoneCaptured("turf.davis"), "Davis should be captured.");
                Assert(econ.Cash == 6000, "Expected $5,000 capture bonus (total $6,000), got $" + econ.Cash);

                // Duplicate takeover rejected
                Assert(turf.TryStartTakeover("turf.davis", 5000) == TerritoryStartStatus.AlreadyOwned,
                    "Duplicate capture of owned zone must return AlreadyOwned.");

                // Reload across session
                var turf2 = new TerritoryWarController(ctx, adapter, econ, reg, store, notify, "slot_turf");
                Assert(turf2.LoadState() && turf2.IsZoneCaptured("turf.davis") && turf2.CapturedZoneCount == 1,
                    "Captured zone must survive reload across sessions.");
            }
            finally { DeleteDir(dir); }
        }

        // 133
        private static void TurfWar_CorruptPrimaryAndBak1Rejected_GoodBak2Recovered()
        {
            string dir = CreateTempDir();
            try
            {
                var ctx = new FakeGameContext();
                var adapter = new FakeTerritoryWarAdapter();
                var econ = new FakeEconomyService(0);
                var reg = new EntityOwnershipRegistry(new FakeCleanupAdapter());
                var notify = new FakeNotificationService();
                var store = new AtomicFileStore(dir, notify);

                var turf = new TerritoryWarController(ctx, adapter, econ, reg, store, notify, "slot_bak");
                turf.Start();
                Assert(turf.TryStartTakeover("turf.del_perro", 1000) == TerritoryStartStatus.Started, "Start failed.");
                for (int w = 0; w < 3; w++)
                {
                    adapter.AliveEnemies.Clear();
                    turf.SlowTick(2000 + w * 1000);
                }
                Assert(turf.IsZoneCaptured("turf.del_perro"), "Del Perro should be captured.");

                string basePath = Path.Combine(dir, turf.SaveFileName);
                string good = File.ReadAllText(basePath, Encoding.UTF8);
                File.WriteAllText(basePath + ".bak2", good, new UTF8Encoding(false));
                File.WriteAllText(basePath + ".bak1", "TURF_SCHEMA=1\nTRUNCATED_LINE\n", new UTF8Encoding(false));
                File.WriteAllText(basePath, "TURF_SCHEMA=1\n", new UTF8Encoding(false));

                var turf2 = new TerritoryWarController(ctx, adapter, econ, reg, store, notify, "slot_bak");
                Assert(turf2.LoadState() && turf2.IsZoneCaptured("turf.del_perro"),
                    "Must reject corrupt primary & truncated .bak1 and recover good .bak2.");
            }
            finally { DeleteDir(dir); }
        }

        // 134 (Probe P17)
        private static void TurfWar_OnlineTransitionAbortsWithoutDeleting_AndPersistFailureRollsBack()
        {
            string dir = CreateTempDir();
            try
            {
                var ctx = new FakeGameContext();
                var adapter = new FakeTerritoryWarAdapter();
                var econ = new FakeEconomyService(0);
                var cleanup = new FakeCleanupAdapter();
                var reg = new EntityOwnershipRegistry(cleanup);
                var notify = new FakeNotificationService();
                var toggleStore = new ToggleFailingPersistenceStore(new AtomicFileStore(dir, notify));

                // Part A: Online transition during active wave aborts without deleting entities
                var turfOnline = new TerritoryWarController(ctx, adapter, econ, reg, toggleStore, notify, "slot_onl");
                turfOnline.Start();
                Assert(turfOnline.TryStartTakeover("turf.downtown", 1000) == TerritoryStartStatus.Started, "Start failed.");
                int deletedBeforeOnline = cleanup.DeletedEntities.Count;
                ctx.IsOnlineSession = true;
                turfOnline.SlowTick(2000);
                Assert(turfOnline.State == TerritoryWarState.Idle && cleanup.DeletedEntities.Count == deletedBeforeOnline,
                    "Online transition must abort Turf War cleanly without deleting entities.");

                // Part B: Persist failure on capture rolls back ownership and does NOT credit bonus
                ctx.IsOnlineSession = false;
                var reg2 = new EntityOwnershipRegistry(cleanup);
                var turfPersist = new TerritoryWarController(ctx, adapter, econ, reg2, toggleStore, notify, "slot_rb");
                turfPersist.Start();
                Assert(turfPersist.TryStartTakeover("turf.docks", 1000) == TerritoryStartStatus.Started, "Start docks failed.");
                adapter.AliveEnemies.Clear();
                turfPersist.SlowTick(2000); // Wave 1 -> 2
                adapter.AliveEnemies.Clear();
                turfPersist.SlowTick(3000); // Wave 2 -> 3

                toggleStore.FailSaves = true;
                adapter.AliveEnemies.Clear();
                turfPersist.SlowTick(4000); // Wave 3 cleared, persist throws!
                Assert(!turfPersist.IsZoneCaptured("turf.docks"),
                    "Persist failure must roll back zone ownership.");
                Assert(econ.Cash == 0, "Persist failure must not credit capture bonus.");
            }
            finally { DeleteDir(dir); }
        }

        // 135
        private static void TurfWar_PassiveIncomeCapsAt7Days_AndRejectsBackwardClock()
        {
            string dir = CreateTempDir();
            try
            {
                var ctx = new FakeGameContext();
                var adapter = new FakeTerritoryWarAdapter();
                var econ = new FakeEconomyService(0);
                var reg = new EntityOwnershipRegistry(new FakeCleanupAdapter());
                var notify = new FakeNotificationService();
                var store = new AtomicFileStore(dir, notify);

                var turf = new TerritoryWarController(ctx, adapter, econ, reg, store, notify, "slot_inc");
                turf.Start();
                Assert(turf.TryStartTakeover("turf.davis", 1000) == TerritoryStartStatus.Started, "Start failed.");
                for (int w = 0; w < 3; w++)
                {
                    adapter.AliveEnemies.Clear();
                    turf.SlowTick(2000 + w * 1000);
                }
                int cashAfterCapture = econ.Cash; // $5,000

                // Day 10: baseline stamped, $0 paid
                TurfIncomeResult baseline = turf.CollectTurfIncome(10);
                Assert(baseline.Status == TurfIncomeStatus.NothingDue && baseline.AmountPaid == 0, "Baseline day must return NothingDue.");

                // Backward clock Day 8 -> rejected
                TurfIncomeResult backward = turf.CollectTurfIncome(8);
                Assert(backward.Status == TurfIncomeStatus.ClockMovedBackward && backward.AmountPaid == 0, "Backward clock must be rejected.");

                // Day 30 (+20 days) -> caps at 7 days * $2,500 = $17,500
                TurfIncomeResult capped = turf.CollectTurfIncome(30);
                Assert(capped.Status == TurfIncomeStatus.Paid && capped.ElapsedDays == 7 && capped.AmountPaid == 17500,
                    "20-day absence must cap at 7 days ($17,500), got $" + capped.AmountPaid);
                Assert(econ.Cash == cashAfterCapture + 17500, "Wallet cash must reflect capped turf income.");
            }
            finally { DeleteDir(dir); }
        }

        // 136 (Gate D Derived Guard)
        private static void GateD_AllRegisteredSchedulerModules_RespectIsOnlineSession()
        {
            string dir = CreateTempDir();
            try
            {
                var ctx = new FakeGameContext { IsOnlineSession = true, IsInVehicle = true };
                var notify = new FakeNotificationService();
                var weapons = new FakeWeaponService();
                var carTheft = new FakeCarTheftAdapter();
                var wanted = new FakeWantedSystemAdapter { WantedLevel = 3 };
                var freedom = new FakeArcadeFreedomAdapter();
                var cheats = new FakeSandboxCheatAdapter();
                var turf = new FakeTerritoryWarAdapter();
                var cleanup = new FakeCleanupAdapter();
                var store = new AtomicFileStore(dir, notify);

                var host = new ClassicGTAHostController(
                    ctx,
                    weapons,
                    new PerHandleAliveDispatchAdapter(),
                    new FakeTaxiAdapter(),
                    new FakeSideJobAdapter(),
                    new FakeCompanionAdapter(),
                    new FakePetAdapter(),
                    new FakeSubmersibleAdapter(),
                    cleanup,
                    store,
                    notify,
                    slotId: "slot_gated",
                    carTheftAdapter: carTheft,
                    wantedAdapter: wanted,
                    arcadeFreedomAdapter: freedom,
                    cheatAdapter: cheats,
                    territoryWarAdapter: turf);

                IList<IModModule> registered = host.Scheduler.GetRegisteredModules();
                Assert(registered.Count >= 11, "Expected at least 11 registered modules on host.Scheduler, found " + registered.Count);

                host.CheatEngine.SuperJumpEnabled = true;
                host.ArcadeFreedomModule.NoVignetteEnabled = true;

                // Tick every registered module directly AND through Scheduler while Online
                host.TickScheduler(1000);
                host.TickScheduler(2000);
                for (int i = 0; i < registered.Count; i++)
                {
                    registered[i].FastTick();
                    registered[i].SlowTick(3000);
                    Assert(!host.Scheduler.IsFaulted(registered[i].ModuleId),
                        "Module faulted during Online tick: " + registered[i].ModuleId);
                }

                Assert(weapons.EnableCombatControlsCalls == 0 && weapons.CanEquipAllCalls == 0, "Weapons acted in Online.");
                Assert(carTheft.GetTargetCalls == 0 && carTheft.UnlockCalls == 0, "CarTheft acted in Online.");
                Assert(wanted.GetWantedCalls == 0 && wanted.SetWantedCalls == 0 && wanted.ClearHeatCalls == 0, "Wanted acted in Online.");
                Assert(freedom.TopUpFuelCalls == 0 && freedom.RestoreVitalsCalls == 0 && freedom.VignetteCalls == 0, "ArcadeFreedom acted in Online.");
                Assert(cheats.SuperJumpCalls == 0 && cheats.NeverWantedCalls == 0, "Cheats acted in Online.");
                Assert(turf.SpawnEnemyCalls == 0 && turf.CreateBlipCalls == 0, "TerritoryWar acted in Online.");
            }
            finally { DeleteDir(dir); }
        }
    }
}
