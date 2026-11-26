# Product Requirements Document — Classic GTA Overhaul

**Version:** 0.1.0  
**Date:** 2026-11-26  
**Status:** Draft for implementation

## 1. Product vision

Restore selected classic Grand Theft Auto sandbox systems in a modular offline single-player mod. The project should make the world feel more systemic: players can earn money through repeatable jobs, own useful assets, progress skills, access more activities, and interact with services beyond the main story.

The project must prioritize reliability, reversibility, performance, player choice and maintainability over feature count.

## 2. Problem statement

Players may want classic sandbox activities and progression loops that are unavailable, restricted, or implemented differently in the target game. A single large script would be difficult to maintain and unsafe to extend. We need a modular framework where features can be enabled independently and ported only through supported game-specific adapters.

## 3. Goals

- Implement a GTA V proof-of-concept using C# and SHVDN3.
- Establish a reusable module lifecycle and configuration pattern.
- Provide persistent player-owned data without corrupting the game's native save.
- Restore selected activities and economy loops through independent modules.
- Keep gameplay logic independent from GTA V native calls.
- Establish automated tests for game-independent logic.
- Prepare a future adapter seam for a possible GTA VI PC implementation.

## 4. Non-goals

- GTA Online, multiplayer, FiveM or competitive functionality.
- Cheating in online play, anti-cheat evasion or security bypasses.
- Reverse engineering leaked console builds or proprietary executables.
- Recreating Rockstar story missions, maps, characters, dialogue or proprietary assets.
- Promising a GTA VI PC port before official PC support and a legitimate modding API exist.
- Shipping copyrighted game assets or third-party assets without permission.
- Implementing all 26 features in the first release.

## 5. Target users

- Players who prefer classic GTA sandbox systems.
- Single-player PC mod users comfortable with installing supported dependencies.
- Mod developers who want independently testable modules.

## 6. Product principles

1. Optional by default: each module can be disabled.
2. Safe failure: if a module fails, the game and other modules should continue.
3. Persistent data is versioned and recoverable.
4. Every spawned entity has an owner and cleanup path.
5. No expensive work in per-frame callbacks.
6. Game-specific code is isolated behind adapter contracts.
7. No unsupported native hashes or guessed API methods.
8. User-facing changes are documented and configurable.

## 7. Functional requirements

### FR-01 Module lifecycle
The host loads, starts, stops and disposes modules independently. A module exposes metadata, configuration validation, start, tick/update, stop and disposal behavior.

### FR-02 Configuration
Users can enable/disable modules, change keybinds and tune gameplay values through a documented INI file. Invalid values fall back safely and are logged.

### FR-03 Persistence
Persistent data uses a versioned JSON schema, atomic writes (temporary file then replace), backups and migration handlers. Save data is kept outside the game's native save files.

### FR-04 Logging
Errors include module ID, operation and exception details. Logs rotate and avoid excessive per-frame output.

### FR-05 Pocket Arsenal
A configurable key toggles an optional expanded weapon inventory in GTA V Story Mode. The module must not remove the player's original weapons when disabled.

### FR-06 Economy
A central ledger records transactions with source, amount, timestamp and reason. Passive income is credited on a controlled game-time schedule, not every frame.

### FR-07 Activities
Jobs use explicit state machines, cancellation paths, payout rules and cleanup routines.

### FR-08 World entities
Every created vehicle, ped, blip or marker is tracked. Entities are released or deleted according to ownership and game API rules when the activity ends or script aborts.

### FR-09 Compatibility
The host records game version, SHVDN version and mod version. Unknown combinations produce a warning and safe disable behavior where appropriate.

## 8. Non-functional requirements

- Performance: no unbounded loops in Tick; routine polling is throttled.
- Stability: exceptions are caught at module boundaries.
- Persistence: interrupted writes must not destroy the last valid save.
- Maintainability: modules should be independently testable.
- Accessibility: keybinds configurable; notifications concise.
- Privacy: no telemetry, network calls or collection of player data.
- Safety: Story Mode only; no online interaction.

## 9. Success metrics

MVP:
- Builds successfully on the documented GTA V / SHVDN setup.
- Pocket Arsenal toggles on/off without deleting original inventory.
- No unhandled exception during a 30-minute test session.
- No persistent entity creation by Module 01.
- Module can be disabled and unloaded without a restart when SHVDN supports reload.
- Configuration defaults work on a clean install.

Framework release:
- At least 80% branch coverage for pure core logic.
- All module lifecycle tests pass.
- Save backup and recovery tests pass.
- No critical/high red-team findings open.

## 10. Assumptions and open questions

- GTA V build and SHVDN version will be selected and pinned by the project owner.
- GTA VI PC release timing and modding support are unknown.
- Every proposed GTA VI feature must be independently verified in the retail game before implementation is prioritized.
- A universal interior weapon unlock cannot be assumed safe; mission restrictions may be intentional.
