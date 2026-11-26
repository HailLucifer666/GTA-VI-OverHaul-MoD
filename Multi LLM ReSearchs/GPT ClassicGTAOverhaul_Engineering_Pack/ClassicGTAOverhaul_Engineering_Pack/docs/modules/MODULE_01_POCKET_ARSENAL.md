# Module 01 — Pocket Arsenal

**ID:** `inventory.pocket-arsenal`  
**Version:** 0.1.0  
**Status:** GTA V prototype source supplied; local build/runtime verification required.

## Purpose
Provide an optional expanded inventory toggle in GTA V Story Mode.

## Keybind
F10 (hardcoded in first prototype; move to INI after configuration service exists).

## Behavior
- Toggle ON: enumerate SHVDN3 `GTA.WeaponHash` values; skip Unarmed and invalid weapon hashes; add valid weapons the player does not already own.
- Toggle OFF: remove only weapon hashes this module added.
- Do not remove weapons the player owned before enabling the module.
- Display a notification with the number of weapons added/removed.
- Does not create entities.
- Does not claim to unlock all interior or mission weapon restrictions.

## Why zone unlocking is separate
Interior and safe-zone restrictions can be implemented through different control, mission and game-script paths. A generic “force weapon + enable fire” loop is not safe. A dedicated zone module needs a verified GTA V test matrix, mission guards and a reversible policy before it can be enabled.

## Manual tests
See `TEST_PLAN.md`, Pocket Arsenal section.

## Known limitations
- Weapon enum contains special/utility entries; runtime `IsWeaponValid` filtering is used.
- Ammo is set to a large finite amount, not infinite.
- The module's in-memory ownership tracking resets on script reload. If reloaded while enabled, previously added weapons may remain. For the MVP, disable before reload or restart Story Mode. A later persistence-safe design should track only module ownership in a controlled session and avoid removing unknown inventory after reload.
- This source has not been compiled against your local GTA V / SHVDN installation.
