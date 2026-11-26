# MVP Definition

## Objective

Deliver a stable GTA V Story Mode prototype proving that the project can load a C# script, respond to a keybind, manipulate the player's inventory through documented SHVDN3 APIs, display feedback, and be rebuilt/reloaded safely.

## MVP scope (Release 0.1)

### Included
- Visual Studio .NET Framework 4.8 Class Library.
- SHVDN3 reference.
- Pocket Arsenal F10 toggle.
- Add valid weapon enum entries with configurable ammo.
- Track only weapons added by this module.
- On disable, remove only weapons added by this module.
- Keep weapons the player already owned.
- Screen notifications.
- Basic error handling and abort cleanup.
- README and manual test checklist.

### Explicitly excluded
- Safe-zone weapon unlocking.
- Interior weapon restriction override.
- LemonUI menu (not needed for first proof).
- Weapon attachments, tint, ammo persistence.
- GTA VI code.
- All other 25 feature groups.

## User story

As a Story Mode player, I want to press F10 to enable a larger weapon inventory and press F10 again to remove only the weapons this module added, so I can experiment without losing my original loadout.

## Acceptance criteria

1. F10 enables the arsenal.
2. A notification reports how many new weapon entries were granted.
3. Existing owned weapons are not duplicated unnecessarily.
4. F10 disables the arsenal.
5. Only weapons added by this module are removed.
6. Original weapons remain available.
7. If the player is dead or unavailable, the module fails gracefully.
8. The module does not spawn peds, vehicles, props or blips.
9. No online mode interaction exists.
10. The module subscribes to the script abort lifecycle and clears in-memory tracking.

## MVP exit gate

Do not begin Module 02 until:
- The project compiles against the chosen SHVDN3 reference.
- A clean Story Mode test passes.
- Toggle-on/off behavior is verified with a pre-existing mixed loadout.
- A 30-minute session shows no unhandled exceptions.
- The Red Team checklist is signed off.

## Why safe-zone unlock is not in MVP

“Safe-zone weapon lock” is not one universal switch. It can be caused by control restrictions, weapon validity, mission scripts, interior logic, wanted-level behavior or scripted scenario rules. Blindly forcing firing controls or re-equipping a weapon could break missions and NPC behavior. First inventory the exact restriction in a controlled GTA V test, then create a separate opt-in module with explicit zone rules and mission guards.
