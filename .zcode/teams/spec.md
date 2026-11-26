# Team SPEC — idea → buildable specification

First: read `_shared.md` in this folder and apply its conventions (DEV_ROOT
resolution, scaffold, dispatch rules, quality floor, wrap-up) throughout.

**Chair:** head-pm · **Members:** software-architect (consultable)
**Purpose:** turn an idea into a buildable, signed-off specification — PRD,
tech stack, architecture, features, project constitution, and feature specs.
This team never writes code and never spawns other teams.

## Mode selection (from $BRIEF)

- Brief names a whole new product/app ("build a habit tracker", "scaffold") → **PROJECT MODE**.
- Brief names one feature for an existing spec'd project ("spec the export feature") → **FEATURE MODE**.
- Brief is empty → PROJECT MODE if DEV_ROOT has no `docs/PRD.md`, otherwise ask.

## Step 1 — Chair: think tank (head-pm, alone)

Brief: adopt role, DEV_ROOT, read any existing docs. Run the think-tank phase:
council skill as procedure (Pragmatist, Architect, Skeptic, User Advocate),
then synthesize: what was understood, what the council flagged, the refined
approach, and the questions that must be answered before proceeding.

In FEATURE MODE, skip the full council unless the feature is strategically
contested — go straight to Step 2 for what is genuinely unclear.

## Step 2 — Chair: structured questionnaire

Batched 3–4 at a time, relayed by the orchestrator:

- **Product:** core user action? primary user? success metric at 3 months?
- **Technical:** stack preferences/constraints? greenfield or existing? auth?
  real-time? web/mobile/desktop?
- **Data & scale:** PII or sensitive data? expected scale? compliance needs?
- **Deployment:** target platform/hosting? CI/CD needed? budget constraints?

Answers feed the documents. Anything the user does not answer is recorded as
an open question in the spec's "clarify before build" list — never invented.

## Step 3 — Chair: document production

Write to DEV_ROOT (templates from `<studio>\.claude\docs\templates\`):

**PROJECT MODE:** `docs/PRD.md`, `docs/TECH_STACK.md`, `docs/ARCHITECTURE.md`,
`docs/FEATURES.md` (MVP marked), `docs/constitution.md`, and
`specs/<mvp-slice>/spec.md` for the first buildable slice.

**FEATURE MODE:** `specs/<feature>/spec.md` + updates to `docs/FEATURES.md`
if the roadmap is affected.

TECH_STACK.md is always ADR-style: 2–3 candidates scored against what THIS
app is, one labeled recommendation, trade-offs named, flagged as a user
decision — proceed with the recommendation unless the user overrides.

## Step 4 — Architect consult (only when needed)

If the stack or system shape is genuinely deep (multi-service, unusual
scale/compliance, hard reversibility), dispatch **software-architect** →
feasibility notes + scored stack candidates appended to `docs/TECH_STACK.md`
draft + risk register for `docs/ARCHITECTURE.md`. One consult, not a panel.

## Step 5 — Gate + wrap-up

Read `<studio>\.claude\docs\director-gates\pm-spec-complete.md` and return
its verdict (PM-SPEC-COMPLETE: READY / CONCERNS / NOT READY) with the gate's
rationale: docs complete, acceptance criteria measurable, clarify list short
and answerable, constitution written.

Report: files written, the stack recommendation (decision needed), open
questions for the user, and the next step: `/implement` (build the MVP slice).
End at the "looks good, build it" checkpoint — never chain into /implement.
