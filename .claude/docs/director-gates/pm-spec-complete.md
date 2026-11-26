> Gate definition. The spawning skill passes this file's path to the chair agent; the AGENT reads it — the parent session should not.

# PM-SPEC-COMPLETE — Head PM Specification Completeness Gate

Agent: `head-pm` | Model tier: session (inherit)

**Trigger**: End of every /spec run, before handoff to /implement.

**Context to pass**:
- DEV_ROOT and the list of spec documents written
- The stack recommendation (if PROJECT MODE)
- The "clarify before build" question list from the spec(s)

**Prompt**:
> "Review the specification set for completeness. Are PRD, TECH_STACK,
> ARCHITECTURE, FEATURES, and constitution all present (PROJECT MODE) or the
> feature spec (FEATURE MODE)? Is every user story's acceptance criterion
> measurable — could a tester write a failing test from it? Are hostile-input
> edge cases covered, not only happy paths? Is the clarify list short and
> actually answerable, with no invented answers? Does the constitution pin test
> discipline, the security floor, and explicit v1 scope exclusions? Return
> READY (spec is buildable as written), CONCERNS [name them], or NOT READY
> [what is missing — do not hand off]."

**Verdicts**: READY / CONCERNS / NOT READY
