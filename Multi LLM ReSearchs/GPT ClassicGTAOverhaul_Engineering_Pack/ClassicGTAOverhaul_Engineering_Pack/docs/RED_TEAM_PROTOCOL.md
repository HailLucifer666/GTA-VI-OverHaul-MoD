# Multi-Agent Council & Red-Team Protocol

Use three review roles. These are review perspectives, not separate autonomous authorities. A human owner approves merges and releases.

## 1. Architect
Checks:
- Layer boundaries and module ownership.
- Dependency direction.
- Lifecycle, state machines and persistence.
- Portability without pretending native calls are portable.
- Acceptance criteria and failure handling.

Required output:
- Design summary.
- Files changed.
- Interfaces affected.
- Risks and migration notes.

## 2. RAGE / SHVDN Specialist
Checks:
- API exists in the selected SHVDN3 version.
- Correct namespace, overload and event signature.
- Native hash source is verified where applicable.
- Game-thread constraints and tick behavior.
- Entity ownership and cleanup.
- Compatibility with pinned GTA V build.

Required evidence:
- Official repository/API docs or source link.
- Exact method signature.
- Runtime test result or clearly marked untested status.

## 3. Red Team
Must attempt to break the proposal:
- Hallucinated APIs or hashes.
- Null player/entity references.
- Duplicate event subscriptions.
- Reload/abort leaks.
- Payout duplication.
- Save corruption.
- Race conditions and state transition holes.
- Expensive Tick loops.
- Mission conflicts.
- Online-mode exposure.
- Third-party IP and licensing violations.

## Review format

For every implementation:
1. Requirements and acceptance criteria.
2. Architect review.
3. API verification review.
4. Red-team findings classified Critical / High / Medium / Low.
5. Fixes applied.
6. Build result.
7. In-game test result.
8. Remaining limitations.
9. Merge decision.

Never claim “compiled”, “tested”, “safe” or “GTA VI compatible” unless the corresponding action was actually performed.
