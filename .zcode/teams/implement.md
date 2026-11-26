# Team IMPLEMENT — spec → working code

First: read `_shared.md` in this folder and apply its conventions (DEV_ROOT
resolution, scaffold, dispatch rules, quality floor, wrap-up) throughout.

**Chair:** build-head · **Members:** team-uxui, team-database, team-backend,
team-frontend, team-devops, team-qa, team-security, build-pr (final reviewer)
**Purpose:** architecture-confirmed working code with tests, built in waves.
Tests are part of done. The app is launched before the run ends.

## Mode selection (from $BRIEF)

- Brief names a bug ("fix the save race", "it 500s on login") → **FIX MODE**.
- Brief names a feature ("add export to CSV", "build the dashboard") → **FEATURE MODE**.
- Brief says scaffold / new project / build it / nothing specific → **SCAFFOLD MODE**
  (requires `docs/PRD.md` from /spec — if missing, route the user to /spec first
  or proceed with a clearly-labeled thin spec if they insist).

## House rules (every mode, non-negotiable — kee prime rule)

- Tests ship with the code. A Logic story without a passing test is not done.
- Every new test is seen failing for the stated reason before it counts.
- Full suite runs at the end: **all green or the run does not end.**
- The app is actually launched and exercised once. Say what you ran and what
  you saw. A green suite that never ran the app has shipped a broken app.
- Karpathy floor for every builder: think before coding (state the contract),
  simplicity first, surgical changes, goal-driven execution.

## Step 1 — Chair: direction (build-head, alone)

Brief: adopt role, DEV_ROOT, read `docs/PRD.md`, `docs/ARCHITECTURE.md`,
`docs/FEATURES.md`, the relevant `specs/<feature>/spec.md`, and existing code.
Write `docs/plan-<slice>.md`: the real product framing, in/out of scope,
architecture bets with reversal costs, sequencing for this build, riskiest
assumption + how it gets tested first, and the file-ownership map (which agent
writes which paths — no overlap). FIX MODE: instead write a root-cause brief
following `~/.claude/rules-library/mythos/FIX-PROTOCOL.md` (reproduce first,
failing test before fix, blast radius, class not instance).

## Step 2 — Waves (head-pm's pattern, build-head's sequencing)

Each wave: parallel dispatch, wait for all, then next wave. Every brief names
DEV_ROOT, the task, exact destination paths, and the spec section it implements.

**SCAFFOLD / FEATURE MODE:**
- Wave 1: **team-uxui** (design system + component specs → `docs/design/`)
  · **team-database** (schema + migrations → `src/` or `db/`, migration tests)
  · **team-backend** (API + core logic + unit tests)
- Wave 2: **team-frontend** (UI from design specs + component tests)
  · **team-devops** (CI pipeline, env validation, deploy config → `ci/`, config)
- Wave 3: **team-qa** (full test suite across layers, hostile-input cases)
  · **team-security** (audit of everything waves 1–2 wrote)
- Wave 4: **build-pr** — final review of the whole diff, PASS or BLOCK verdict
  (per its own kee review discipline; findings with file:line + Fix: lines)

**FIX MODE (single pass, not waves):** the owning specialist implements the fix
+ regression test (test first, watched red), then **build-pr** reviews the fix
diff. If the reporter's cause differs from the real cause, say so in the report.

## Step 3 — Integration gates

- Run the full suite via Bash at DEV_ROOT. Real output in the report — counts,
  not claims. All green or the run does not end: fix-loop through the owning
  specialist. If a test cannot be written for something, say why in one line.
- Run/start the app once and exercise the changed path. Report what you saw.
- Read `<studio>\.claude\docs\director-gates\bh-build-gate.md` and return its
  verdict (BH-BUILD-GATE: VERIFIED / CONCERNS / FAILED) — suite, launch, scope
  conformance to plan and spec, file-ownership respected.

## Step 4 — Wrap-up

Report: exact command to run the app, what works, real test output, files
written, decisions needed, deviations from spec (there should be none silently),
and the next step: `/harden` before anything ships.
