# Test Plan & Release Gates

## Test pyramid

1. Unit tests: pure business rules and state machines.
2. Adapter tests: wrapper behavior with mocked contracts.
3. In-game integration tests: GTA V Story Mode.
4. Manual regression: mission, interior, death, reload and save behavior.
5. Stability/performance: long sessions and entity counts.

## Module test checklist

### General
- [ ] Module starts with valid config.
- [ ] Invalid config produces safe defaults and a warning.
- [ ] Module can be disabled.
- [ ] Start failure does not crash other modules.
- [ ] Stop is idempotent.
- [ ] Dispose releases subscriptions and resources.
- [ ] Script abort triggers cleanup.
- [ ] No unbounded work in Tick.
- [ ] No synchronous disk/network I/O in Tick.

### Pocket Arsenal
- [ ] Start with no weapons.
- [ ] Start with several owned weapons.
- [ ] Toggle on.
- [ ] Toggle off.
- [ ] Original weapons remain.
- [ ] Added weapons are removed.
- [ ] Invalid weapon enum values are skipped.
- [ ] Player death / respawn.
- [ ] F10 while pause/menu is open.
- [ ] Reload script and test again.

### Activity modules
- [ ] Start activity.
- [ ] Complete activity.
- [ ] Fail activity.
- [ ] Cancel activity.
- [ ] Player dies.
- [ ] Player changes vehicle.
- [ ] Mission starts while activity active.
- [ ] Entity cleanup runs.
- [ ] Blips and prompts are removed.
- [ ] Payout happens exactly once.

### Persistence
- [ ] First-run save.
- [ ] Existing save load.
- [ ] Malformed JSON.
- [ ] Unsupported schema version.
- [ ] Interrupted write.
- [ ] Backup restore.
- [ ] Migration from previous schema.
- [ ] Numeric bounds and negative-value validation.

## Performance budgets (initial targets)

- Routine module polling: 100–500 ms unless a verified reason requires faster.
- No full-world scans every frame.
- Avoid allocations and LINQ in hot paths.
- Track spawned entity count per module.
- No unbounded log growth.
- Profile on target hardware before publishing.

## Release gate

No release with:
- Critical/high crash bug.
- Save corruption risk.
- Entity leak.
- Multiplayer/online code path.
- Unverified native hash.
- Missing dependency notice.
- Unresolved license/IP concern.
