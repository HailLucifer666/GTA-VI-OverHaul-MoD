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
| **Phase 2** | `ClassicGTA.Infrastructure` — 3-Tier Fault-Isolated Scheduler (`0ms/250ms/1000ms`), Three-Way Entity Ownership Registry (`Owned/Borrowed/Detached` + separate `Blip` tracker), Atomic Persistence (`temp -> 3x rotating backup -> replace` JSON + INI config) | **NEXT** | Pending | Pending |
| **Phase 3** | **Module 2 (`EmergencyDispatchController`)** — Cut Feature `#25` (911 Police, Ambulance heal-on-arrival, Fire Truck) + arrival/timeout/cleanup FSM | **PENDING** | Pending | Pending |
| **Phase 4** | **Economy, Properties, Businesses, Vehicles & Stocks** — Cut Features `#1, #2, #3, #4, #5` (idempotent transactions, `OfflineCapDays = 7`, stock ticker + contract shocks) | **PENDING** | Pending | Pending |
| **Phase 5** | **Side-Hustles, RPG Stats & Card Games** — Cut Features `#8, #9, #10, #15, #17 [SUB], #20` (Taxi 5-state FSM, Vigilante/Paramedic/Firefighter, Rampage timer/medals, RPG XP curves, Blackjack/Poker) | **PENDING** | Pending | Pending |
| **Phase 6** | **GTA VI (PC) Adapter & Live Integration** (Gated by 6-Gate Policy after GTA VI PC + verified Script Hook VI launch) | **BLOCKED (Awaiting GTA VI PC)** | Pending | Pending |

---

## 3. Exact Resume Command for Next Session

To start **Phase 2** in any new session or CLI:
1. Run `.\build.ps1` and confirm Phase 1 is still **12/12 PASS**.
2. Build `src\ClassicGTA.Infrastructure` (Fault-Isolated 3-Tier Scheduler, Three-Way Entity/Blip Registry, Atomic JSON Persistence with 3× rotating backups).
3. Add Phase 2 tests to `tests\ClassicGTA.Tests`, run at least 3 sabotage probes (crash isolation, borrowed-entity protection, corrupt-temp-file rollback), restore to GREEN, update this tracker, and push to `origin/main`.
