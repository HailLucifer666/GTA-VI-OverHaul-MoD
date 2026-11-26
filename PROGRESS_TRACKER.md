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
5. **Legal & Safety Guardrails:** Offline Story Mode only (`IsOnlineSession` hard-blocks all actions). Zero interaction with leaked builds (`E:\GTA VI BUILD Help\` is permanently off-limits). Zero cross-title asset ripping.

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
| **Phase 6** | **GTA VI (PC) Adapter & Live Integration** (Gated by 6-Gate Policy after GTA VI PC + verified Script Hook VI launch) | **NEXT (Awaiting GTA VI PC)** | Pending (6-Gate Policy) | Pending |

---

## 3. Exact Resume Command for Next Session

**All offline-buildable Core & Infrastructure phases (Phases 0–5) are 100% COMPLETE (58/58 tests PASS, 15/15 sabotage probes confirmed RED → restored GREEN).**

To start **Phase 6** once **GTA VI (PC)** + a verified, legal public Script Hook VI launch (all 6 Migration Gates passed):
1. Run `.\build.ps1` and confirm Phases 1–5 are still **58/58 PASS**.
2. Create `src\ClassicGTA.Adapter.GTAVI\` implementing the verified `ClassicGTA.Contracts` interfaces (`IGameContext`, `IWeaponService`, `IEntityCleanupAdapter`, `IEmergencyDispatchAdapter`, `IEconomyService`, `ITaxiAdapter`, `INotificationService`).
3. Populate externalized Leonida coordinate & asset tables (`ContentId`-keyed JSON/INI) without modifying `ClassicGTA.Core`.
4. Run the in-game Measurement Protocol (`[RESTORE]` logs, 30 consecutive hot-reloads, 30-minute soak test, entity/blip baseline return check) and time-box Tier 3 physics/animation spikes (Prone `#21`, Dual-Wield `#22`, Skateboarding `#16`, Guard Dogs `#26`) to 3 sessions max each.
