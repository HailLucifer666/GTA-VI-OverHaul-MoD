# AI Agent Workflow — Build, Verify, Red-Team

## Rules for AI coding agents

1. Read README, PRD, architecture, target module spec and current ADRs before coding.
2. Do not implement outside the requested module.
3. Do not invent SHVDN methods, enum members, hashes, event signatures or LemonUI APIs.
4. Verify every game API against the pinned version's documentation or source.
5. Do not use online/multiplayer APIs.
6. Do not modify native game saves.
7. Do not introduce dependencies without approval.
8. Never leave TODOs, placeholder methods or fake success paths.
9. If a requested feature cannot be implemented safely with verified APIs, stop and explain the blocker; do not fabricate an API.
10. Include tests and manual in-game test steps.

## Suggested agent sequence

### Agent A — Architect
Produce a short implementation plan, interfaces touched and dependency graph. No code yet.

### Agent B — API researcher
Verify every API signature and relevant runtime behavior. Return evidence links and version constraints.

### Agent C — Implementer
Write the smallest complete implementation. Include error handling and lifecycle cleanup.

### Agent D — Red Team
Review independently for crashes, leaks, mission conflicts, save corruption, performance and policy violations.

### Agent E — Integrator
Build, run tests, inspect diff, update docs and prepare release notes.

## Required handoff template

- Module ID/version:
- User story:
- Scope / non-scope:
- APIs verified:
- Files changed:
- Build command/result:
- Unit test command/result:
- In-game test result:
- Red-team findings:
- Known limitations:
- Rollback instructions:

## Master prompt for a coding agent

You are implementing one module in Classic GTA Overhaul. Read the repository docs first. Work only within the requested module. The project is GTA V Story Mode only and uses the pinned SHVDN3/.NET Framework 4.8 toolchain. Keep all GTA-specific calls behind the GTA V adapter where the architecture allows. Verify every API against the selected SHVDN version. Do not invent methods, hashes, enum values or events. Do not touch multiplayer, anti-cheat, leaked builds, native game saves or copyrighted assets. Implement complete behavior, lifecycle cleanup, configuration validation, tests and documentation. If the feature cannot be implemented with verified supported APIs, report the blocker instead of creating a placeholder. Before finalizing, perform Architect, SHVDN Specialist and Red Team reviews and report build/test results truthfully.
