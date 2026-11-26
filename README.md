# Classic GTA Overhaul Mod

A modular, story-mode-only C# overhaul mod prototyping classic Grand Theft Auto sandbox mechanics in **GTA V Legacy (PC)** (`ScriptHookVDotNet3 3.6.0` + `LemonUI.SHVDN3 2.2.0`), built behind engine-agnostic behavioral interfaces (`ClassicGTA.Contracts` + `ClassicGTA.Core`) for future portability when **GTA VI (PC)** and a verified public scripting interface launch.

> **Honest Status (2026-11-27 — `v0.6.3-hardened`):**
> - **Automated Test Suite:** `116/116` unique automated tests passing (`0 Errors, 0 Warnings`), compiling and executing directly against `ClassicGTA.GTAV.dll` (Core + Infrastructure + Adapter Seam tests), plus `12/12` repeatable `/kee` sabotage probes in `.\probe.ps1`.
> - **Authoritative Feature Breakdown (26-item backlog):**
>   - **Live GTA V Verified:** `0` (all runtime native behavior, frame time, hot-reload stability, and 30-minute soak remain **Phase 6B pending**).
>   - **Core + Adapter Seam Tested & Menu-Wired (`F7` Hub / Hotkeys):** `11` (`#1` Safehouse Ledger, `#2` Passive Business UTC Income, `#4` Kraken Water Delivery, `#5` LCN Stocks, `#8` Taxi, `#9` Emergency Side-Jobs, `#15` Blackjack, `#19` Pocket Arsenal & Interior Zone Restore, `#24` Companion Hangout, `#25` 911 Dispatch, `#26` Guard Dogs).
>   - **Core Unit-Tested Only (Unwired in GTA V):** `6` (`#6 [SUB]`, `#7`, `#10`, `#17 [SUB]`, `#18`, `#20`).
>   - **Not Yet Built:** `9` (`#3`, `#11–#14`, `#16`, `#21–#23`).
> - **Economy & Persistence Architecture:**
>   - Dedicated per-slot atomic **Mod Wallet** (`ClassicGTA_Wallet_<slot>.dat`) paired with per-slot **Asset Ledger** (`ClassicGTA_Assets_<slot>.dat`) eliminates GTA Story Mode save/load cash-vs-ownership desync by construction, with automatic migration from legacy v0.6.0 `ClassicGTA_Assets.dat`.
>   - All 7 persisted engines use strict `PersistenceCodec` (`SCHEMA` + per-line grammar + `RECORD_COUNT` + `END_OF_FILE`) and validated backup rotation.
> - **Remaining Gate Before Release / Known-Good Tag:**
>   - **Phase 6B (Live GTA V Legacy Session):** Native execution, entity cleanup, 30 hot-reloads, 30-minute soak, and frame-time telemetry must be measured in-game before any release or known-good runtime claim.

## Quick Start (Zero `C:\` Writes)

```powershell
.\build.ps1   # SHA-256 verified E:\-only restore of SHVDN3 3.6.0 + LemonUI 2.2.0, compiles all 5 projects, runs 116/116 tests
.\probe.ps1   # Runs 12 scripted /kee mutation probes (confirms RED on sabotage, then restores GREEN)
```

## Player Prerequisites (GTA V Legacy Story Mode Only)
- `ScriptHookV` + `ScriptHookVDotNet3` installed in the **GTA V root folder** (never copy `ScriptHookVDotNet3.dll` into `scripts\`).
- Copy `ClassicGTA.GTAV.dll`, `ClassicGTA.Core.dll`, `ClassicGTA.Contracts.dll`, `ClassicGTA.Infrastructure.dll`, and `LemonUI.SHVDN3.dll` into `GTA V\scripts\`.

## Repository Layout

- `PROGRESS_TRACKER.md` — Cross-session resume tracker (phase status, test counts, probes).
- `build.ps1` / `probe.ps1` — Reproducible SHA-256-verified build, test, and 12-probe mutation runners.
- `src/ClassicGTA.Contracts/` — Engine-agnostic behavioral interfaces, DTOs, and strict `PersistenceCodec`.
- `src/ClassicGTA.Core/` — Pure C# gameplay engines + `ModWalletEconomyService` (zero `GTA.*` / `LemonUI.*` references).
- `src/ClassicGTA.Infrastructure/` — Fault-isolated scheduler, 3-way entity/blip/camera registry, atomic validated 3× backup store, throttled INI.
- `src/ClassicGTA.Adapter.GTAV/` — SHVDN3 3.6.0 + LemonUI 2.2.0 host (`F7` hub, `F8`/`F9`/`F10` hotkeys) + testable host/adapter seams (`ClassicGTAHostController`, `UiLoopGuard`, `KrakenDeliveryController`, `RoadSpawnValidator`, `FireExtinguishEvaluator`, `PassiveIncomeClock`, `OnlineDetectionGuard`).
- `tests/ClassicGTA.Tests/` — Zero-dependency automated test runner (`116/116` unique tests + test-count & doc-consistency guards).

## Legal & Safety

- **Story Mode Only:** No network or anti-cheat interaction; fail-closed `IsOnlineSession` blocks all actions in multiplayer (logging and notifying once on native failure).
- **Non-Commercial & Native Assets Only:** Uses only vanilla game models; zero cross-title asset ripping and zero affiliation with Rockstar Games / Take-Two Interactive.
