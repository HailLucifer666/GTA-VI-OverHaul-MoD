# Team HARDEN — code → verified

First: read `_shared.md` in this folder and apply its conventions (DEV_ROOT
resolution, scaffold, dispatch rules, quality floor, wrap-up) throughout.

**Chair:** code-reviewer · **Members:** security-auditor, performance-optimizer,
test-auditor
**Purpose:** find what is actually wrong, by class, with evidence — then triage.
This team finds; it only applies data-only fixes. Code fixes are queued as
stories for /implement.

## Mode selection (from $BRIEF)

- Brief names a diff/PR to review ("review my PR", "check this branch") → **REVIEW MODE**.
- Brief mentions preflight / "is my PR ready" → **PREFLIGHT MODE**.
- Otherwise (whole repo, "audit this", "is this any good") → **AUDIT MODE**.

## Step 1 — Chair: map before judging (code-reviewer, alone)

Brief: adopt role, DEV_ROOT, read `~/.claude/rules-library/mythos/GATES.md`.
Map the codebase in the kee audit order: root structure → entry points →
package files → config → core data models → auth → API surface → the important
20% of business logic → test coverage → CI. Output CODEBASE MAP +
ARCHITECTURE SUMMARY (2–4 sentences) + KEY DEPENDENCIES + IMMEDIATE FLAGS into
`production/reviews/harden-<date>-map.md`. REVIEW/PREFLIGHT MODE: skip the map
if the diff is small; read the diff against its base instead.

Write the history before the defect list: the constraints that produced the
code, then what is wrong. Never report "nothing found" when no target was
resolved.

## Step 2 — Parallel hunts (each confined to its lens)

- **code-reviewer** → defect-class hunt (kee classes by name: TOCTOU,
  un-awaited writes, friendly test data, guard containing its own bug,
  denylists where a rule belongs, existence checks standing in for correlation,
  errors that hide evidence, false greens) + diff-level review in REVIEW MODE.
  Findings: severity CRITICAL/IMPORTANT/ADVISORY/NIT, CONFIRMED or PLAUSIBLE,
  file:line, ending with a Fix: line.
- **security-auditor** → mythos secret-and-leak sweep as its own pass
  (`~/.zcode/skills/mythos-ship/SKILL.md` Stage 2) + auth boundary checks +
  injection surface. Security findings are never deferred for scoring low.
- **performance-optimizer** → measure first: real numbers or it is not a
  finding. Name the budget each recommendation serves and its precondition.
- **test-auditor** → probe the existing tests: mutation-probe the important
  guards (break the exact statement, watch red, restore, record), hunt false
  greens (class 14), check fixtures reach the branch production reaches,
  spec-vs-reality cross-check against `docs/PRD.md`/`specs/` when present.

Each writes a section into `production/reviews/harden-<date>.md` (chair
creates the skeleton first, one section per lens).

## Step 3 — Chair: synthesis + triage

Read `<studio>\.claude\docs\director-gates\cr-hardening-gate.md` and return
its verdict (CR-HARDENING-GATE: CLEAN / CONCERNS / FAIL) on top of the
consolidated report. Triage every finding into CLOSED / QUICK WIN / BACKLOG /
ACCEPT with a one-line reason each. Sequence remediation where order matters
(fix authorization before rotating keys). Name what to leave alone. Point at
the working sibling implementation when one exists. Data-only fixes: apply
now, list them. Code fixes: queued stories under `production/backlog.md`.

PREFLIGHT MODE: additionally run the kee preflight checklist
(`~/.claude/rules-library/mythos/PREFLIGHT.md`) against the diff and produce
the FAIL/CHECK table with evidence beside every row.

## Step 4 — Wrap-up

Report: verdict, the triaged findings summary (count by severity), report
path, what was applied vs queued, and next step: `/implement` for the queued
stories, or `/release` when clean.
