# Classic GTA Overhaul Mod

A modular, story-mode-only C# overhaul mod prototyping classic Grand Theft Auto sandbox mechanics in **GTA V Legacy (PC)** (`ScriptHookVDotNet3 3.6.0` + `LemonUI.SHVDN3 2.2.0`), built behind engine-agnostic behavioral interfaces (`ClassicGTA.Contracts` + `ClassicGTA.Core`) for future portability when **GTA VI (PC)** and a verified public scripting interface launch.

> **Honest Status (2026-11-27):**
> - **Offline Unit-Tested Suite:** `98/98` unique automated tests passing (`0 Errors, 0 Warnings`), plus repeatable mutation probes in `.\probe.ps1`.
> - **Feature Delivery Breakdown (26-item backlog):** `3` delivered end-to-end (`#8` Taxi, `#19` Pocket Arsenal/Zone Restore, `#25` 911 Dispatch), `11` reachable in the `F7` hub in some form, `6` core engines built/tested offline but unwired in GTA V (`#6`, `#7`, `#10`, `#17 [SUB]`, `#18`, `#20`), `9` not yet built (`#3`, `#11–#14`, `#16`, `#21–#23`).
> - **Known Open Limitations (tracked for Phase 6B):**
>   - `ClassicGTA.Adapter.GTAV` ships on **compile-only evidence** (zero adapter unit tests; not yet live-tested in GTA V Legacy).
>   - `ClassicGTA_Assets.dat` is currently a **global Story Mode ledger** (not yet isolated per GTA V savegame slot).

## Quick Start (Zero `C:\` Writes)

```powershell
.\build.ps1   # Auto-restores SHVDN3 3.6.0 + LemonUI 2.2.0 into .\.packages\, compiles all 5 projects, runs 94 tests
.\probe.ps1   # Runs scripted /kee mutation probes (confirms RED on sabotage, then restores GREEN)
```

## Player Prerequisites (GTA V Legacy Story Mode Only)
- `ScriptHookV` + `ScriptHookVDotNet3` installed in the **GTA V root folder** (never copy `ScriptHookVDotNet3.dll` into `scripts\`).
- Copy `ClassicGTA.GTAV.dll`, `ClassicGTA.Core.dll`, `ClassicGTA.Contracts.dll`, `ClassicGTA.Infrastructure.dll`, and `LemonUI.SHVDN3.dll` into `GTA V\scripts\`.

## Repository Layout

- `PROGRESS_TRACKER.md` — Cross-session resume tracker (phase status, test counts, probes).
- `build.ps1` / `probe.ps1` — Reproducible build, test, and mutation-probe runners.
- `src/ClassicGTA.Contracts/` — 14 engine-agnostic behavioral interfaces & DTOs.
- `src/ClassicGTA.Core/` — Pure C# gameplay engines (zero `GTA.*` / `LemonUI.*` references).
- `src/ClassicGTA.Infrastructure/` — Fault-isolated scheduler, 3-way entity/blip/camera registry, atomic 3× backup store, throttled INI.
- `src/ClassicGTA.Adapter.GTAV/` — SHVDN3 3.6.0 + LemonUI 2.2.0 host (`F7` hub, `F8`/`F9`/`F10` hotkeys).
- `tests/ClassicGTA.Tests/` — Zero-dependency automated test runner (94 tests + test-count guard).

## Legal & Safety

- **Story Mode Only:** No network or anti-cheat interaction; fail-closed `IsOnlineSession` blocks all actions in multiplayer.
- **Non-Commercial & Native Assets Only:** Uses only vanilla game models and story-mode cash; zero cross-title asset ripping and zero affiliation with Rockstar Games / Take-Two Interactive.
