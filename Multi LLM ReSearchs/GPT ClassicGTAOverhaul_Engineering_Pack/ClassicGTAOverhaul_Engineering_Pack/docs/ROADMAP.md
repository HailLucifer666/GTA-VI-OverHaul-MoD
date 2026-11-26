# Delivery Roadmap

The schedule is effort-based, not a promise of calendar delivery. One feature should not start until its dependencies and acceptance tests are ready.

## Phase 0 — Project control and verification
Deliverables:
- Confirm GTA V edition/build and SHVDN compatibility.
- Verify each of the 26 claimed GTA VI omissions against reliable evidence or mark “unverified”.
- Read Rockstar's current Terms, Creator Platform License and Mod Guidelines.
- Create Git repository, issue labels, versioning and branch protection.
Exit gate: no unresolved platform, legal or dependency blocker.

## Phase 1 — Build foundation
Deliverables:
- Visual Studio project.
- Core / Contracts / GTA5 separation.
- Logging, config, module lifecycle and error boundaries.
- Unit-test project.
Exit gate: clean build and lifecycle tests.

## Phase 2 — MVP inventory
Deliverables:
- Pocket Arsenal toggle.
- Configurable key.
- Non-destructive add/remove behavior.
- Manual test record.
Exit gate: MVP acceptance criteria pass.

## Phase 3 — World interaction framework
Deliverables:
- Interaction registry.
- Marker/prompt abstraction.
- GPS/blip abstraction.
- Entity ownership registry.
- Activity cancellation framework.
Exit gate: spawn-and-cleanup test passes.

## Phase 4 — Economy framework
Deliverables:
- Transaction ledger.
- Wallet service.
- Game-time clock abstraction.
- Passive income scheduler.
- Save schema and migration.
Exit gate: duplicate payouts prevented; interrupted saves recover.

## Phase 5 — Repeatable jobs
Suggested order:
1. Taxi fares.
2. Vigilante pursuits.
3. Ambulance response.
4. Firefighter response.
5. Rampage challenges.
Exit gate: all jobs have cancel, fail, payout and cleanup paths.

## Phase 6 — Ownership and progression
Suggested order:
1. Player skill progression.
2. Vehicle purchase registry.
3. Property ownership.
4. Business ownership/passive income.
5. Stock simulation.
6. Mission replay research spike.
Exit gate: save migration and balance tests pass.

## Phase 7 — Activities and social systems
Suggested order:
1. Darts / bowling.
2. Cinema / amusement interactions.
3. Hangouts.
4. Pets / companions.
5. Tennis / casino / skateboarding prototypes.
Exit gate: mechanics use original code and permitted assets; no unsupported animation hacks.

## Phase 8 — Advanced mechanics
- First-person camera options.
- Prone movement.
- Dual-wielding.
- Submarine controls.
These require feasibility spikes and should not block core releases.

## Phase 9 — Release hardening
- Compatibility matrix.
- 2-hour stability test.
- Save corruption/recovery test.
- Performance profile.
- Clean installation test.
- Dependency and license notices.
- User documentation and rollback guide.

## Phase 10 — GTA VI readiness
A separate go/no-go decision:
- Retail PC release available.
- Supported modding method exists.
- Legal and policy review completed.
- APIs verified.
- Adapter prototype built without leaked materials or unauthorized executable reverse engineering.
Until then, continue improving portable domain logic and GTA V prototype only.
