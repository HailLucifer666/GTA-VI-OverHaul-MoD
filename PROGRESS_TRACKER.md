# Classic GTA Overhaul Mod — Cross-Session Progress Tracker

> **Purpose:** Any new session, AI model (Gemini, Claude, GPT, Qwen, Kimi), or CLI tool can read this single file, run `.\build.ps1`, and immediately resume from the exact next phase with zero lost context.

**Repository:** `https://github.com/HailLucifer666/classic-gta-overhaul-mod` (private)
**Workspace Root:** `E:\Classic GTA overhaul mod for PC`
**Last Updated:** `2026-11-26`
**Dev Team Installed Locally:** `.zcode/`, `.claude/`, `AGENTS.md`, `TEAM-USAGE.md` (from `E:\WIFI Hack\Claude-Code-Dev-Studios`)

---

## 1. Non-Negotiable Engineering Rules (For Any AI / CLI Resuming This Repo)

1. **`E:\` Drive Only:** Never write packages, caches, or builds to `C:\`. `nuget.config` pins `globalPackagesFolder` to `.\.packages`.
2. **Direct-to-GTA-VI Architecture:** Keep 100% of game logic inside `src\ClassicGTA.Contracts` and `src\ClassicGTA.Core` with **zero dependency** on `GTA.*`, `ScriptHookVDotNet3`, or `LemonUI`. Only future `src\ClassicGTA.Adapter.GTAVI` (post–GTA VI PC launch) may reference engine hooks.
3. **One-Command Build & Test:** Run `.\build.ps1` from repo root. Gate: `0 Errors, 0 Warnings` (`TreatWarningsAsErrors=true`) and `0` failed tests.
4. **`/kee` Discipline:** Every phase MUST be **built → tested → sabotage-probed (watch tests go RED) → restored → verified GREEN** before advancing. Record every probe and its date below.
5. **Legal & Safety Guardrails:** Story Mode only (fail-closed `IsOnlineSession` hard-blocks all actions). Zero interaction with any leaked builds. Zero cross-title asset ripping.

---

## 2. Phase Tracker

| Phase | Scope | Status | Build & Test Proof | Sabotage Probes Recorded |
| :---: | :--- | :---: | :--- | :--- |
| **Phase 0** | Repo init, GitHub remote (`HailLucifer666/classic-gta-overhaul-mod`), install `Claude-Code-Dev-Studios` team locally, `nuget.config` (`E:\` only), `/kee` audit + Multi-LLM synthesis | **DONE** | `git remote -v` + `docs/` committed | N/A (setup) |
| **Phase 1** | Solution scaffold (`Contracts`, `Core`, `Tests`) + **Module 1 (`WeaponAndZoneController`)** — Cut Feature `#19` + Safe-Zone/Interior restoration | **DONE** | `.\build.ps1` → **0 Errors, 0 Warnings, 12/12 PASS** (`2026-11-26`) | **3/3 RED confirmed (`2026-11-26`):**<br>• Probe A (`IsOnlineSession` removed) → `04_FastTick_InOnlineSession_NeverActs` FAIL<br>• Probe B (`gameTimeMs < _lastSlowTickMs` disabled) → `09_SlowTick_GameTimeWrapAround_DoesNotFreeze` FAIL<br>• Probe C (protagonist-switch reset removed) → `10_SlowTick_ProtagonistSwitch_ResetsTrackedWeapon` FAIL |
| **Phase 2** | `ClassicGTA.Infrastructure` — 3-Tier Fault-Isolated Scheduler (`0ms/250ms/1000ms`), Three-Way Entity Ownership Registry (`Owned/Borrowed/Detached` + separate `Blip` tracker + camera-restore-before-delete), Atomic Persistence (`temp -> 3x rotating backup -> replace` + throttled `SimpleIniConfig`) | **DONE** | `.\build.ps1` → **0 Errors, 0 Warnings, 24/24 PASS** (`2026-11-26`) | **3/3 RED confirmed (`2026-11-26`):**<br>• Probe D (scheduler isolation bypassed) → `13_Scheduler_IsolatesFaultingModuleAndKeepsSiblingRunning` FAIL<br>• Probe E (borrowed entity deleted) → `17_EntityRegistry_OwnedDeleted_BorrowedNeverTouched_DetachedReleased` FAIL<br>• Probe F (camera restore skipped before delete) → `19_EntityRegistry_RestoresCameraBeforeDeletingCamera` FAIL |
| **Phase 3** | **Module 2 (`EmergencyDispatchController`)** — Cut Feature `#25` (911 Police, Ambulance heal-on-arrival, Fire Truck) + arrival/timeout/cleanup FSM | **DONE** | `.\build.ps1` → **0 Errors, 0 Warnings, 34/34 PASS** (`2026-11-26`) | **3/3 RED confirmed (`2026-11-26`):**<br>• Probe G (remote heal before arrival) → `26_Dispatch_Ambulance_DoesNotHealBeforePhysicalArrival` & `27` FAIL<br>• Probe H (unconditional drive re-task spam) → `30_Dispatch_DoesNotSpamDriveTaskUnlessPlayerMovesPastThreshold` FAIL<br>• Probe I (skipped cleanup on timeout) → `31_Dispatch_Timeout_CleansUpEntitiesAndBlips` FAIL |
| **Phase 4** | **Economy, Properties, Businesses, Vehicles & Stocks** — Cut Features `#1, #2, #3, #4, #5` (idempotent transactions, `OfflineCapDays = 7`, stock ticker + contract shocks) | **DONE** | `.\build.ps1` → **0 Errors, 0 Warnings, 46/46 PASS** (`2026-11-26`) | **3/3 RED confirmed (`2026-11-26`):**<br>• Probe J (missing refund on persist failure) → `38_AssetEngine_PersistenceFailure_RefundsCashAndRollsBackOwnership` FAIL<br>• Probe K (`OfflineCapDays` cap bypassed) → `40_AssetEngine_PassiveIncome_CapsAtOfflineCapDaysAndIsIdempotent` FAIL<br>• Probe L (duplicate purchase double-charge) → `36_AssetEngine_DuplicatePurchase_RejectedWithZeroDoubleCharge` FAIL |
| **Phase 5** | **Side-Hustles, RPG Stats & Card Games** — Cut Features `#8, #10, #15, #17 [SUB], #20` (Taxi 5-state FSM, Rampage timer/medals, RPG XP curves, virtual-cash Blackjack) | **DONE** | `.\build.ps1` → **0 Errors, 0 Warnings, 58/58 PASS** (`2026-11-26`) | **3/3 RED confirmed (`2026-11-26`):**<br>• Probe M (borrowed taxi tracked as Owned) → `48_Taxi_AbortNeverDeletesBorrowedPlayerTaxi_CleansOwnedFareAndBlip` FAIL<br>• Probe N (medal downgrade overwrite) → `50_Rampage_KillThresholdAwardsBronzeSilverGoldAndNeverDowngradesMedal` FAIL<br>• Probe O (soft-Ace downgrade disabled) → `56_Blackjack_SoftAceAdjustmentAndPlayerBust_SettlesCorrectly` FAIL |
| **Phase 5B** | **Emergency Side-Jobs, First-Person Camera & Friend Hangouts** — Cut Features `#9, #18, #24` (12-level Vigilante/Paramedic/Firefighter, `±75°` pitch-clamped FPS camera, Companion AI) | **DONE** | `.\build.ps1` → **0 Errors, 0 Warnings, 70/70 PASS** (`2026-11-26`) | **3/3 RED confirmed (`2026-11-26`):**<br>• Probe P (borrowed emergency vehicle tracked Owned) → `60_EmergencyJobs_ExitingVehicleAbortsWithoutDeletingBorrowedVehicle` FAIL<br>• Probe Q (FPS pitch clamp removed) → `63_FirstPerson_ToggleClampsPitchToPlusMinus75AndSyncsHeadingOnFoot` FAIL<br>• Probe R (dismissed companion deleted instead of released) → `67_Companion_CallFollowCombatAssistAndNaturalDismiss` FAIL |
| **Phase 5C** | **Guard Dogs (`#26`), Home Furnishing Store (`#7`) & ClassicNet Web Hub (`#6 [SUB]`)** — remaining offline-buildable pure-C# modules | **DONE** | `.\build.ps1` → **0 Errors, 0 Warnings, 82/82 PASS** (`2026-11-26`) | **3/3 RED confirmed (`2026-11-26`):**<br>• Probe S (unowned pet breed summoned) → `71_Pet_CannotSummonUnownedBreed_BuyThenSummonAndCommand` FAIL<br>• Probe T (out-of-bounds furniture placement) → `76_Decorator_RejectsOutOfBoundsAndUnownedSafehouse` FAIL<br>• Probe U (missing furniture refund on persist fail) → `77_Decorator_PersistFailureRefundsAndRollsBack` FAIL |
| **Phase 6A** | **`ClassicGTA.Adapter.GTAV` (GTA V Legacy Prototype — Compile-Only)** — Compiled against `ScriptHookVDotNet3 3.6.0` + `LemonUI.SHVDN3 2.2.0` (`E:\.packages\`, `Private=False`); wires 6 `F7` submenus (Weapons/Zone, 911 Dispatch, Taxi, Emergency Jobs, Properties/Businesses/Kraken Summon/Stocks, Blackjack, Friends/Guard Dogs) | **DONE (Compile-Only)** | `.\build.ps1` → **0 Errors, 0 Warnings** (`2026-11-26`); not yet live-tested in-game | Zero `ScriptHookVDotNet3.dll` leakage into `bin\Release\` |
| **Phase 6A-Harden (Pass 1 & 2)** | **Hardening Passes (`v0.6.1` + `v0.6.2`)** — Closed `R-1`, `H-01`, `C-2`, `C-4`, `C-6`, `C-7`, `C-9`, `C-10`, `H-02` (5/5 sites), `H-03`, `R-4/V-11`, `R-5`, `R-14/V-19`, `S-06/V-04/V-13`, `S-08`, `S-10/V-18`, `S-11`, `S-12/V-12`, `V-14`, `V-16`, `V-20/V-21`. **Explicitly still OPEN until Phase 6B:** `C-1` (global Story Mode ledger, not per-savegame isolated) & `C-3` (adapter is compile-only, zero adapter unit tests) | **DONE (Known Open: C-1, C-3)** | `.\build.ps1` → **0 Errors, 0 Warnings, 98/98 PASS** (`2026-11-27`) | **`.\probe.ps1` (green baseline + exact literal `[FAIL]` prefix match: 6/6 RED → 98/98 GREEN)** |
| **Phase 6B** | **Live GTA V Legacy In-Game Deploy & Measurement** — Deploy built DLLs + `LemonUI.SHVDN3.dll` to `GTA V Legacy\scripts\` once download finishes, verify `GTA5.exe` build + ScriptHookV/SHVDN3 runtime, and run Measurement Protocol | **NEXT (Awaiting GTA V Legacy Download)** | Pending (game downloading) | Pending |

---

## 3. Exact Resume Command for Next Session

**ALL offline-buildable Core & Infrastructure phases (Phases 0–5C) are 100% COMPLETE: 82/82 tests PASS, 21/21 sabotage probes (A–U) confirmed RED → restored GREEN, 17 of 26 cut features implemented in pure C#.**

To start **Phase 6** once your game download finishes (in any new session or CLI):
1. Run `.\build.ps1` and confirm Phases 1–5C are still **82/82 PASS**.
2. Check which game + edition finished installing (Legacy vs. Enhanced if GTA V, or GTA VI PC post-launch) and verify exact `GTA*.exe` product version before downloading matching ScriptHook / SHVDN3 binaries into `E:\`.
3. Build the thin Engine Adapter (`src\ClassicGTA.Adapter.*`) wiring our 14 verified `ClassicGTA.Contracts` interfaces (`IGameContext`, `IWeaponService`, `IEntityCleanupAdapter`, `IEmergencyDispatchAdapter`, `IEconomyService`, `ITaxiAdapter`, `IEmergencySideJobAdapter`, `IFirstPersonCameraAdapter`, `ICompanionAdapter`, `IPetAdapter`, `INotificationService`) without modifying `ClassicGTA.Core`.
4. Run the in-game Measurement Protocol (`[RESTORE]` telemetry logs, 30 consecutive hot-reloads, 30-minute soak test, zero entity/blip leaks).
