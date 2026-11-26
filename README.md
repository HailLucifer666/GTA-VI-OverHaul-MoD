# Classic GTA Overhaul Mod

A modular, open-source, offline Story Mode C# overhaul mod designed to restore 26 classic Grand Theft Auto sandbox mechanics for **GTA VI (PC)**, built with strict engine-agnostic separation so all core gameplay systems compile and pass automated tests today.

## Quick Start (Zero `C:\` Writes)

```powershell
.\build.ps1
```

- Compiles `ClassicGTA.Contracts`, `ClassicGTA.Core`, and `ClassicGTA.Tests` (`Release | x64`, `TreatWarningsAsErrors=true`) using Visual Studio MSBuild.
- Runs the zero-dependency automated test suite (`ClassicGTA.Tests.exe`).
- Keeps all NuGet packages strictly inside `.\.packages\` via `nuget.config`.

## Repository Layout

- `PROGRESS_TRACKER.md` — Cross-session resume tracker (phase status, test counts, `/kee` sabotage probes).
- `docs/` — PRD, MVP, Tech Stack, Architecture, `/kee` Audit Report, and Unified Master Plan.
- `src/ClassicGTA.Contracts/` — Behavioral interfaces (`IModModule`, `IGameContext`, `IWeaponService`, `INotificationService`).
- `src/ClassicGTA.Core/` — Pure C# gameplay modules and state machines (zero game DLL dependencies).
- `tests/ClassicGTA.Tests/` — Automated state-matrix test runner.
- `.zcode/`, `.claude/`, `AGENTS.md`, `TEAM-USAGE.md` — Local Software Dev Studio team installation.

## Legal & Safety

- **Offline Story Mode Only:** `IsOnlineSession` hard-blocks all mod behavior.
- **100% Free & Open Source (\$0):** No paywalls, no real-money gambling, no third-party IP, no cross-title asset ripping, and zero interaction with leaked builds.
