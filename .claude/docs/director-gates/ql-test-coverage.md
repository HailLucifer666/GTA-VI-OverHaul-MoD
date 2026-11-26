> Gate definition. The spawning skill passes this file's path to the QA agent; the AGENT reads it — the parent session should not.

# QL-TEST-COVERAGE — Test Coverage Review (probe-based)

Agent: `team-qa` (in /implement) or `test-auditor` (in /harden) | Model tier: session (inherit)

**Trigger**: Wave 3 of /implement (coverage of implemented stories) and the
test-audit pass of /harden (probe of existing tests).

**Context to pass**:
- List of implemented stories/fixes with their types (Logic / Integration / UI / Config)
- Test file paths and the full-suite output
- Spec acceptance criteria (from `specs/<feature>/spec.md`) or PRD criteria

**Prompt**:
> "Review test coverage and trustworthiness. Is every Logic story covered by a
> passing unit test? Is every acceptance criterion mapped to at least one test?
> Are hostile inputs covered (empty, unicode, delimiter itself, generated
> format, duplicates, null) or only the author's imagined inputs? Does each
> test's fixture reach the same branch production reaches — warm the state and
> re-check the ones that pass cold? Mutation-prove the important guards: break
> the exact statement each assertion depends on, watch it fail for the stated
> reason, restore, and record the probe with its date. Hunt false greens: any
> check structurally incapable of seeing its defect (a spec that skips when its
> container is missing, a fixture more generous than the wire, an assertion on
> a value the runtime never emits)? Name every suite not run with the reason.
> Return COVERED (coverage meets standards and probes ran), GAPS [specific
> missing tests + which mutations failed to fail], or UNTESTED [critical logic
> is untested or the suite is untrustworthy — do not advance]."

**Verdicts**: COVERED / GAPS / UNTESTED
