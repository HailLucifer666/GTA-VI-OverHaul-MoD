# ADR-0001: Game Adapter Boundary

**Status:** Accepted  
**Date:** 2026-11-26

## Context
GTA V and GTA VI may differ in runtime, engine APIs, entity model identifiers, map coordinates, native hashes and modding restrictions. Reusing GTA V native calls in GTA VI code would create false portability and high maintenance risk.

## Decision
All game-specific operations are isolated behind semantic interfaces. Gameplay modules depend on interfaces, not SHVDN or native hashes. The GTA V adapter implements those interfaces. A future GTA VI adapter is not created until a legitimate supported PC scripting interface exists.

## Consequences
Positive:
- Core economy and job logic can be tested without the game.
- Native calls remain isolated.
- Future port is a deliberate adapter implementation, not a search-and-replace exercise.

Negative:
- More upfront interface design.
- Some features may require game-specific capabilities and cannot be fully abstracted.
- GTA VI porting still requires significant rework and validation.

## Rejected alternatives
- Copy GTA V hashes into a shared `GameNatives.cs`.
- Guess GTA VI natives from leaks.
- Build a universal native wrapper that hides incompatible game behavior.
