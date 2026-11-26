# Contributing to Classic GTA Overhaul

Thanks for wanting to help. This project is built by players, in the open, and it only works if people with different skills show up. You do not need to be a senior C# developer to be useful here.

## The one rule that matters

**A fix without a test is a claim, not a fix.**

Every behaviour change ships with a test that fails without it. If a test is genuinely impossible, say why in the pull request. We would rather read a short honest explanation than merge an unproven fix.

## Ways to help

| Skill | What you can do |
|:--|:--|
| **GTA V Legacy on PC** | Run the Phase 6B protocol in-game and report what breaks. This is the highest-value help right now. |
| **C# gameplay** | State machines, economy rules, persistence, edge cases, tests. |
| **SHVDN / native research** | Spawning, tasks, blips, cameras; proving engine behaviour. |
| **LemonUI / UX** | The in-game hub, menu flow, notifications, readability. |
| **3D / art** | Icons, key art, trailers once footage exists. |
| **QA** | Test plans, regression passes, reproducing bugs. |
| **Docs / video** | Guides, changelogs, clips, answering questions. |

## Development setup

Requirements:

- Windows 10 or 11
- .NET Framework 4.8
- Visual Studio Build Tools (or Visual Studio) with the .NET desktop workload
- PowerShell 5.1 or later

```powershell
git clone <your-fork>
cd "Classic GTA overhaul mod for PC"
.\build.ps1     # restores packages, builds 5 projects, runs the full suite
.\probe.ps1     # 12 sabotage probes: break a guard, confirm the right test goes red, restore
```

`build.ps1` restores the two pinned dependencies into `.packages\` and verifies their SHA-256. Nothing is installed globally.

## Branch and pull request flow

1. **One change per branch.** Branch from `main`, name it after the change (`fix/taxi-pickup-timeout`).
2. **Keep `main` green.** `main` is the last known-good build; it is never committed to directly.
3. **Run both gates before opening the PR:** `.\build.ps1` must be green, and `.\probe.ps1` must finish green after its probes.
4. **Bring your branch up to date with `main` and re-run** — two branches that each pass can still break each other.
5. **Fill in the PR template.** Say what changed, what proves it, and what it does not cover.

## Code rules

- **Target:** C# 7.3, .NET Framework 4.8, x64.
- **`ClassicGTA.Contracts` and `ClassicGTA.Core` must never reference `GTA.*` or `LemonUI.*`.** Game APIs live only in `ClassicGTA.Adapter.GTAV`. The build enforces this; do not work around it.
- **No hand-typed game data lists.** Derive from the engine's own enums and validate at runtime.
- **Warnings are errors.** `TreatWarningsAsErrors=true` is on.
- **Persistence goes through the codec.** New saved state needs a schema, a record count and an end marker.

## Adding a sabotage probe

If you add a guard that protects something dangerous — money, entity ownership, online safety — add a probe to `probe.ps1`:

1. Mutate the exact statement.
2. Assert the named test fails.
3. Restore in `finally` and re-run the full suite.

A guard nobody has watched fail is not yet a guard.

## Reporting a bug

Open an issue with:

- What you did, what you expected, what happened.
- GTA V edition and build (`Legacy` / `Enhanced`, `GTA5.exe` version).
- The newest lines from `ClassicGTA_Data\ClassicGTA.log`.
- Whether ScriptHookV and ScriptHookVDotNet3 are installed, and their versions.

Never attach a GTA game file. Never attach pre-release material.

## Scope and safety

- Story Mode only. The mod hard-blocks itself in multiplayer; do not propose changes that weaken that.
- Vanilla game assets only. No ripped models, no cross-title asset copying, no pre-release builds.
- No real-money mechanics. Casino play uses story-mode cash.

## Community

Use Discussions to introduce yourself, ask questions or claim work. Be decent — see [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).
