# Project Constitution — <project name>

> The project's non-negotiables. Every team member (human or agent) works
> inside these. Changing the constitution is a user decision, recorded here.

## Test discipline
Every behavior change ships its test. A fix without a test is a claim.
The full suite is green before "done", and the app is launched.

## Security floor
No hardcoded secrets · parameterized queries · validated at the boundary on
the trusted side · errors never leak internals · new env vars validated at
startup and documented in `.env.example`.

## Scope rules
What v1 excludes (from PRD). Scope creep gets "that's Phase 2", not a PR.

## Definition of done
- [ ] Tests written and watched failing before counted
- [ ] Full suite green (counts stated)
- [ ] App launched and changed path exercised
- [ ] Docs/comments updated where non-obvious
- [ ] No secrets in diff or history
