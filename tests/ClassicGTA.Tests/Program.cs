using System;
using System.Collections.Generic;
using ClassicGTA.Contracts;
using ClassicGTA.Core.Module1;

namespace ClassicGTA.Tests
{
    /// <summary>
    /// Zero-dependency test runner for ClassicGTA.Core.
    /// Exits 0 if all tests pass, 1 if any test fails.
    /// </summary>
    internal static class Program
    {
        private static int Main()
        {
            var tests = new List<KeyValuePair<string, Action>>
            {
                Test("01_FastTick_WhenEnabledAndReady_EnablesControls", FastTick_WhenEnabledAndReady_EnablesControls),
                Test("02_FastTick_DuringCutscene_DoesNotTouchControls", FastTick_DuringCutscene_DoesNotTouchControls),
                Test("03_FastTick_WhenPaused_DoesNotTouchControls", FastTick_WhenPaused_DoesNotTouchControls),
                Test("04_FastTick_InOnlineSession_NeverActs", FastTick_InOnlineSession_NeverActs),
                Test("05_FastTick_WhenDisabled_DoesNotTouchControls", FastTick_WhenDisabled_DoesNotTouchControls),
                Test("06_SlowTick_ForcedUnarmedInInterior_RestoresOwnedWeaponAndLogs", SlowTick_ForcedUnarmedInInterior_RestoresOwnedWeaponAndLogs),
                Test("07_SlowTick_LastWeaponRemovedByMission_DoesNotRestoreOrReGrant", SlowTick_LastWeaponRemovedByMission_DoesNotRestoreOrReGrant),
                Test("08_SlowTick_RespectsThrottleInterval", SlowTick_RespectsThrottleInterval),
                Test("09_SlowTick_GameTimeWrapAround_DoesNotFreeze", SlowTick_GameTimeWrapAround_DoesNotFreeze),
                Test("10_SlowTick_ProtagonistSwitch_ResetsTrackedWeapon", SlowTick_ProtagonistSwitch_ResetsTrackedWeapon),
                Test("11_PocketArsenal_DeduplicatesSkipsUnarmedAndPreservesCurrentWeapon", PocketArsenal_DeduplicatesSkipsUnarmedAndPreservesCurrentWeapon),
                Test("12_PocketArsenal_IsIdempotentAndDisposeNeverRemovesWeapons", PocketArsenal_IsIdempotentAndDisposeNeverRemovesWeapons)
            };

            tests.AddRange(InfrastructureTests.GetAll());
            tests.AddRange(Module2Tests.GetAll());
            tests.AddRange(EconomyTests.GetAll());
            tests.AddRange(ActivitiesTests.GetAll());
            tests.AddRange(Phase5BTests.GetAll());
            tests.AddRange(Phase5CTests.GetAll());
            tests.AddRange(HardenRegressionTests.GetAll());

            // T-10 Fix: Enforce minimum test count so silently dropped tests fail the gate.
            const int expectedMinTests = 94;
            if (tests.Count < expectedMinTests)
            {
                Console.WriteLine("[FAIL] Test-count guard tripped: expected at least " + expectedMinTests + ", found " + tests.Count);
                return 1;
            }

            int passed = 0;
            int failed = 0;

            foreach (var entry in tests)
            {
                try
                {
                    entry.Value();
                    Console.WriteLine("[PASS] " + entry.Key);
                    passed++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[FAIL] " + entry.Key + " -> " + ex.Message);
                    failed++;
                }
            }

            Console.WriteLine();
            Console.WriteLine("Summary: " + passed + " passed, " + failed + " failed.");
            return failed == 0 ? 0 : 1;
        }

        private static KeyValuePair<string, Action> Test(string name, Action body)
        {
            return new KeyValuePair<string, Action>(name, body);
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static WeaponAndZoneController CreateController(
            out FakeGameContext ctx,
            out FakeWeaponService weapons,
            out FakeNotificationService notify)
        {
            ctx = new FakeGameContext();
            weapons = new FakeWeaponService();
            notify = new FakeNotificationService();
            var controller = new WeaponAndZoneController(ctx, weapons, notify, new WeaponAndZoneConfig());
            controller.Start();
            return controller;
        }

        private static void FastTick_WhenEnabledAndReady_EnablesControls()
        {
            FakeGameContext ctx; FakeWeaponService weapons; FakeNotificationService notify;
            var mod = CreateController(out ctx, out weapons, out notify);
            mod.FastTick();
            Assert(weapons.EnableCombatControlsCalls == 1, "Expected EnableCombatControlsThisFrame once.");
            Assert(weapons.DisableFiringFalseCalls == 1, "Expected SetDisablePlayerFiring(false) once.");
        }

        private static void FastTick_DuringCutscene_DoesNotTouchControls()
        {
            FakeGameContext ctx; FakeWeaponService weapons; FakeNotificationService notify;
            var mod = CreateController(out ctx, out weapons, out notify);
            ctx.IsInCutscene = true;
            mod.FastTick();
            Assert(weapons.EnableCombatControlsCalls == 0, "Controls must not be enabled during cutscenes.");
        }

        private static void FastTick_WhenPaused_DoesNotTouchControls()
        {
            FakeGameContext ctx; FakeWeaponService weapons; FakeNotificationService notify;
            var mod = CreateController(out ctx, out weapons, out notify);
            ctx.IsPaused = true;
            mod.FastTick();
            Assert(weapons.EnableCombatControlsCalls == 0, "Controls must not be enabled while paused.");
        }

        private static void FastTick_InOnlineSession_NeverActs()
        {
            FakeGameContext ctx; FakeWeaponService weapons; FakeNotificationService notify;
            var mod = CreateController(out ctx, out weapons, out notify);
            ctx.IsOnlineSession = true;
            mod.FastTick();
            mod.SlowTick(2000);
            int granted = mod.GrantPocketArsenal();
            Assert(weapons.EnableCombatControlsCalls == 0, "Must never enable controls in Online.");
            Assert(weapons.CanEquipAllCalls == 0, "Must never touch weapon flags in Online.");
            Assert(granted == 0, "Must never grant weapons in Online.");
        }

        private static void FastTick_WhenDisabled_DoesNotTouchControls()
        {
            FakeGameContext ctx; FakeWeaponService weapons; FakeNotificationService notify;
            var mod = CreateController(out ctx, out weapons, out notify);
            mod.OnKeyPressed(GameKey.F8);
            mod.FastTick();
            Assert(weapons.EnableCombatControlsCalls == 0, "Controls must not be enabled after F8 toggle off.");
        }

        private static void SlowTick_ForcedUnarmedInInterior_RestoresOwnedWeaponAndLogs()
        {
            FakeGameContext ctx; FakeWeaponService weapons; FakeNotificationService notify;
            var mod = CreateController(out ctx, out weapons, out notify);
            weapons.OwnedWeapons.Add(FakeWeaponService.Carbine);
            weapons.CurrentWeaponId = FakeWeaponService.Carbine;
            weapons.InteriorId = 0;

            mod.SlowTick(1000);

            // Cross into interior and get forcibly stripped to Unarmed
            weapons.InteriorId = 42;
            weapons.CurrentWeaponId = FakeWeaponService.Unarmed;
            mod.SlowTick(1250);

            Assert(weapons.CurrentWeaponId == FakeWeaponService.Carbine, "Expected Carbine to be restored.");
            Assert(weapons.SelectWeaponCalls == 1, "Expected exactly 1 SelectWeapon call.");
            Assert(notify.Logs.Exists(l => l.Contains("[RESTORE]") && l.Contains("interior=42")),
                "Expected [RESTORE] telemetry log line.");
        }

        private static void SlowTick_LastWeaponRemovedByMission_DoesNotRestoreOrReGrant()
        {
            FakeGameContext ctx; FakeWeaponService weapons; FakeNotificationService notify;
            var mod = CreateController(out ctx, out weapons, out notify);
            weapons.OwnedWeapons.Add(FakeWeaponService.Pistol);
            weapons.CurrentWeaponId = FakeWeaponService.Pistol;
            mod.SlowTick(1000);

            // Mission script confiscates the weapon completely
            weapons.OwnedWeapons.Remove(FakeWeaponService.Pistol);
            weapons.CurrentWeaponId = FakeWeaponService.Unarmed;
            weapons.InteriorId = 99;
            mod.SlowTick(1250);

            Assert(weapons.SelectWeaponCalls == 0, "Must not select a confiscated weapon.");
            Assert(weapons.GiveWeaponCalls == 0, "Must never re-grant a confiscated weapon.");
        }

        private static void SlowTick_RespectsThrottleInterval()
        {
            FakeGameContext ctx; FakeWeaponService weapons; FakeNotificationService notify;
            var mod = CreateController(out ctx, out weapons, out notify);
            weapons.InteriorId = 42;

            mod.SlowTick(1000);
            mod.SlowTick(1100); // 100ms < 200ms threshold
            mod.SlowTick(1199); // 199ms < 200ms threshold
            mod.SlowTick(1200); // 200ms >= 200ms threshold

            Assert(weapons.CanEquipAllCalls == 2, "Expected exactly 2 throttled slow ticks, got " + weapons.CanEquipAllCalls);
        }

        private static void SlowTick_GameTimeWrapAround_DoesNotFreeze()
        {
            FakeGameContext ctx; FakeWeaponService weapons; FakeNotificationService notify;
            var mod = CreateController(out ctx, out weapons, out notify);
            weapons.InteriorId = 42;

            mod.SlowTick(50000);
            // Save load resets GameTime back to 100ms
            mod.SlowTick(100);

            Assert(weapons.CanEquipAllCalls == 2, "SlowTick froze when GameTime reset on save load.");
        }

        private static void SlowTick_ProtagonistSwitch_ResetsTrackedWeapon()
        {
            FakeGameContext ctx; FakeWeaponService weapons; FakeNotificationService notify;
            var mod = CreateController(out ctx, out weapons, out notify);

            // Protagonist A holds Carbine outdoors
            ctx.PlayerHandle = 1001;
            weapons.OwnedWeapons.Add(FakeWeaponService.Carbine);
            weapons.CurrentWeaponId = FakeWeaponService.Carbine;
            mod.SlowTick(1000);

            // Switch to Protagonist B (different handle) who is Unarmed inside an interior
            ctx.PlayerHandle = 2002;
            weapons.CurrentWeaponId = FakeWeaponService.Unarmed;
            weapons.InteriorId = 7;
            mod.SlowTick(1300);

            Assert(weapons.SelectWeaponCalls == 0, "Tracked weapon leaked across protagonist switch.");
        }

        private static void PocketArsenal_DeduplicatesSkipsUnarmedAndPreservesCurrentWeapon()
        {
            FakeGameContext ctx; FakeWeaponService weapons; FakeNotificationService notify;
            var mod = CreateController(out ctx, out weapons, out notify);
            weapons.OwnedWeapons.Add(FakeWeaponService.Pistol);
            weapons.CurrentWeaponId = FakeWeaponService.Pistol;

            // Catalog has: Unarmed, Pistol, Pistol (duplicate), Carbine, Shotgun => 3 unique valid weapons
            mod.OnKeyPressed(GameKey.F10);

            Assert(weapons.GiveWeaponCalls == 3, "Expected 3 deduplicated weapon grants, got " + weapons.GiveWeaponCalls);
            Assert(!weapons.OwnedWeapons.Contains(FakeWeaponService.Unarmed), "Must never grant Unarmed.");
            Assert(weapons.CurrentWeaponId == FakeWeaponService.Pistol, "Must preserve currently held weapon.");
        }

        private static void PocketArsenal_IsIdempotentAndDisposeNeverRemovesWeapons()
        {
            FakeGameContext ctx; FakeWeaponService weapons; FakeNotificationService notify;
            var mod = CreateController(out ctx, out weapons, out notify);

            int first = mod.GrantPocketArsenal();
            int second = mod.GrantPocketArsenal();
            mod.Dispose();

            Assert(first == 3 && second == 3, "Pocket Arsenal must be idempotent.");
            Assert(weapons.OwnedWeapons.Count == 3, "Dispose must never strip player weapons.");
        }
    }
}
