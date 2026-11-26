# Building and installing

Two audiences live here: players who want the mod running, and developers who want to change it.

---

## Players — install on GTA V Legacy (PC)

> **Status:** the adapter is compile-verified but has never been run in-game. Treat this as a prototype install. If it breaks, the log plus a report is genuinely useful.

### What you need

1. **GTA V Legacy** for PC, Story Mode.
2. **ScriptHookV** — the ASI loader. Install into the GTA V root folder.
3. **ScriptHookVDotNet3** — the .NET script runtime. Install into the GTA V root folder.
4. The five mod DLLs from a build (see below).

### Install

Copy these five files into `GTA V\scripts\`:

```
ClassicGTA.GTAV.dll
ClassicGTA.Core.dll
ClassicGTA.Contracts.dll
ClassicGTA.Infrastructure.dll
LemonUI.SHVDN3.dll
```

> **Never** copy `ScriptHookVDotNet3.dll` into `scripts\`. It belongs in the game root.

### Run

1. Launch GTA V in **Story Mode**. The mod refuses to act in multiplayer by design.
2. Press **F7** to open the hub.
3. Hotkeys: **F8** zone weapon restore · **F9** 911 dispatch · **F10** pocket arsenal.

### Where the mod stores data

`GTA V\scripts\ClassicGTA_Data\` — the mod's own wallet, ledger and log. It never edits your GTA save file.

### Uninstall

Delete the five DLLs above and the `ClassicGTA_Data` folder. Nothing else is touched.

---

## Developers — build from source

### Requirements

- Windows 10 or 11 (x64)
- .NET Framework 4.8
- Visual Studio Build Tools or Visual Studio with the .NET desktop workload (a Roslyn-capable MSBuild)
- PowerShell 5.1 or later

### Build and test

```powershell
.\build.ps1          # Release | x64, warnings as errors, full suite
.\build.ps1 -SkipTests   # compile only
```

First run downloads the pinned `ScriptHookVDotNet3 3.6.0` and `LemonUI.SHVDN3 2.2.0` packages into `.packages\`, verifying their SHA-256. Nothing is installed globally and nothing is written outside the repository.

Output:

```
src\ClassicGTA.Adapter.GTAV\bin\Release\
```

### Mutation probes

```powershell
.\probe.ps1
```

Runs a green baseline, then breaks one guard at a time and confirms the *named* test goes red before restoring the file byte-for-byte and re-running the suite. A guard nobody has watched fail is not yet a guard.

### Deploy straight into the game

Set `GTA5_DIR` to your GTA V folder before building. The adapter's build target copies the four mod DLLs plus `LemonUI.SHVDN3.dll` into `GTA5_DIR\scripts\` when `GTA5.exe` is present, and deliberately excludes `ScriptHookVDotNet3.dll`.

```powershell
$env:GTA5_DIR = "D:\Games\GTAV"
.\build.ps1
```

### Project layout

| Project | Role |
|:--|:--|
| `ClassicGTA.Contracts` | Interfaces and DTOs. No game dependency. |
| `ClassicGTA.Core` | Gameplay engines and state machines. No `GTA.*`, no `LemonUI.*`. |
| `ClassicGTA.Infrastructure` | Scheduler, entity registry, atomic store, persistence codec. |
| `ClassicGTA.Adapter.GTAV` | The only project that references ScriptHookVDotNet3 and LemonUI. |
| `ClassicGTA.Tests` | Zero-dependency runner: unit tests plus adapter-seam tests. |

---

## Troubleshooting

**The menu does not appear.**
Check that `LemonUI.SHVDN3.dll` is in `scripts\`. The script throws at load if it is missing, and ScriptHookVDotNet reports that in its own log.

**The mod loads but every action does nothing.**
Check `ClassicGTA_Data\ClassicGTA.log`. If the session detector cannot read the game's network state, the mod fails closed and stays inert — by design.

**Nothing works in GTA Online.**
Correct. The mod is story-mode only and blocks itself in multiplayer.

**The game build is newer than ScriptHookV supports.**
Update ScriptHookV. The mod cannot fix a script hook that does not load.

**A feature is listed but does nothing.**
Check [`FEATURES.md`](FEATURES.md) — some systems are lab-only, and nothing is live-verified yet.

When reporting, include the last lines of `ClassicGTA.log`, your GTA V edition and `GTA5.exe` version, and the ScriptHookV/SHVDN3 versions.
