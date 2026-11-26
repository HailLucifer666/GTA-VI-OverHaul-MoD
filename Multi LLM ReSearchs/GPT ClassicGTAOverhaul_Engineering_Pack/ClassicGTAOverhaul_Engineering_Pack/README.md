# Classic GTA Overhaul

A modular, offline single-player gameplay restoration project. GTA V is the current prototype and validation target. GTA VI is a future target only after a legitimate PC release and a compatible, publicly available modding interface exist.

## Current status (2026-11-26)

- GTA VI launched on PS5 and Xbox Series X|S on 2026-11-19.
- Rockstar has not announced a PC release in the official material checked for this plan.
- No supported GTA VI PC scripting SDK or Script Hook VI has been established for this project.
- GTA V prototype work can proceed with ScriptHookV + ScriptHookVDotNet3.
- The supplied feature list is a proposed backlog, not a verified statement that every listed feature is absent from GTA VI.

## Non-negotiable boundaries

1. Story Mode / offline single-player only.
2. Never connect modded code to GTA Online or any multiplayer service.
3. Do not bypass BattlEye or other security systems to access online play.
4. Do not use leaked console builds, private executables, stolen SDKs, or unauthorized reverse engineering.
5. Do not redistribute Rockstar game assets, executable files, proprietary data, or third-party assets without permission.
6. Do not assume GTA V native hashes, coordinates, models, UI, or API calls transfer to GTA VI.
7. Re-check Rockstar's current Terms, Creator Platform License and Mod Guidelines before any public release. This is an engineering plan, not legal advice.

## First build

Open `src/ClassicGTA.GTA5/ClassicGTA.GTA5.csproj` in Visual Studio 2022. Set `GTA5_DIR` to your GTA V installation directory, restore NuGet packages, build Release, and verify the output in `GTA V/scripts`.

The initial source file implements the Pocket Arsenal toggle only. A universal safe-zone/interior weapon unlocker is deliberately not claimed: the restriction sources and mission-safe override behavior must be verified in the actual GTA V build before that feature is enabled.

## Documentation map

- `docs/PRD.md`: product requirements and scope.
- `docs/MVP.md`: smallest valuable playable release.
- `docs/ARCHITECTURE.md`: system boundaries and module contracts.
- `docs/TECH_STACK.md`: tools, versions and dependencies.
- `docs/ROADMAP.md`: phased execution plan.
- `docs/FEATURE_BACKLOG.md`: 26-feature backlog and dependencies.
- `docs/TEST_PLAN.md`: test strategy and release gates.
- `docs/RED_TEAM_PROTOCOL.md`: adversarial engineering checklist.
- `docs/SECURITY_AND_LEGAL.md`: safety, compatibility and IP boundaries.
- `docs/AGENT_WORKFLOW.md`: multi-agent implementation protocol.
- `docs/SETUP_GTA5.md`: Visual Studio setup.
- `docs/modules/MODULE_01_POCKET_ARSENAL.md`: first module spec.
- `docs/ADR/0001_ADAPTER_BOUNDARY.md`: architecture decision record.

## Important limitation

The source has not been compiled or executed in your local GTA V installation. Treat the first build as a verification step, not as a claim of tested compatibility. The referenced SHVDN APIs and package versions are documented by their maintainers; game updates can still break runtime compatibility.
