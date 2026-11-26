# Architecture & Engineering Design

## 1. Design goals

- Keep gameplay rules independent of the game engine.
- Make GTA V and any future target separate adapters.
- Ensure modules can be enabled, disabled, tested and unloaded independently.
- Make all persistent changes explicit and recoverable.
- Prevent entity leaks and expensive frame loops.

## 2. Layer model

```text
User Input / LemonUI
        |
Module Host + Lifecycle
        |
Gameplay Modules
        |
Core Domain + Contracts
        |
Game Adapter Interfaces
        |
GTA V Adapter (SHVDN3)       Future GTA VI Adapter (unavailable)
        |
Game runtime
```

## 3. Suggested projects

### ClassicGTA.Contracts
Interfaces and DTOs only:
- IModModule
- IGameWorld
- IPlayerService
- IWeaponService
- IVehicleService
- IBlipService
- INotificationService
- IClock
- IPersistenceStore
- IInputService

No references to GTA namespaces or native hashes.

### ClassicGTA.Core
Pure domain logic:
- Economy ledger
- Activity state machines
- Property ownership
- Business income schedule
- Skill progression
- Save schema and migrations

No SHVDN dependency.

### ClassicGTA.Infrastructure
- JSON persistence
- INI configuration
- Structured logging
- Backup/restore
- Module registry

### ClassicGTA.GTA5
- SHVDN3 adapter implementations
- GTA V-specific model names, coordinates and native calls
- GTA V compatibility checks

### ClassicGTA.UI
- LemonUI menus
- Notifications and prompts
- Menu registration and disposal

### ClassicGTA.Modules
- Feature modules; each references Contracts and Core, never raw native APIs.

## 4. Module contract (conceptual)

Each module must expose:
- Stable module ID and semantic version.
- Dependencies and optional conflicts.
- Default configuration.
- ValidateConfiguration().
- Start(context).
- Update(context, elapsedTime) or event-driven handlers.
- Stop(reason).
- Dispose().

A module must not construct another module directly. Dependencies are injected through a context/service registry.

## 5. Lifecycle

```text
Discovered -> Validated -> Initialized -> Running
     |            |             |           |
     +----------> Disabled <----+           |
                                  Stopping <-+
                                      |
                                   Disposed
```

On startup, validate dependencies and configuration before Start. If Start fails, call Stop/Dispose and disable only that module. On shutdown or script abort, stop modules in reverse dependency order.

## 6. Tick and scheduling rules

- Never scan all world entities every frame.
- Use a scheduler with explicit intervals (e.g. 100–500 ms for routine checks).
- Use event-driven callbacks where possible.
- Avoid LINQ allocations in hot paths.
- Cap work per update.
- Avoid synchronous file I/O inside Tick.
- Do not block the game thread with network or disk operations.
- Profile before optimizing.

## 7. Entity ownership

Every created entity is registered with an activity/module owner. The registry records handle, entity type, owner and cleanup policy. On cancellation, failure, player death, script abort or reload:
1. Stop issuing tasks.
2. Remove associated blips/markers.
3. Delete only entities the mod owns and is allowed to delete.
4. Mark surviving entities no longer needed where appropriate.
5. Clear references and registry entries.

Never delete mission-owned or player-owned entities merely because a module tracked a reference.

## 8. Persistence

Use a schema such as:
```json
{
  "schemaVersion": 1,
  "profileId": "default",
  "economy": { "cashLedger": [] },
  "ownership": { "properties": [], "vehicles": [], "businesses": [] },
  "progression": { "skills": {} },
  "modules": {}
}
```

Write to a temporary file, flush, then replace the previous file. Keep one or more backups. Validate schema and numeric bounds on load. Never deserialize arbitrary runtime type metadata.

## 9. GTA VI portability strategy

The portable layer contains concepts such as “start taxi job”, “credit payout”, “own property” and “gain driving XP”. The adapter owns actual game operations. GTA VI implementation must be a new adapter built against a legitimate supported PC scripting interface, after:
- PC release is available.
- The interface is publicly available and legally usable.
- APIs are documented or independently verified through permitted means.
- Compatibility and distribution policy are reviewed.

Do not make a “universal native hash” abstraction. Prefer semantic interfaces. Native hashes belong only in a game adapter.
