> Gate definition. The spawning skill passes this file's path to the chair agent; the AGENT reads it — the parent session should not.

# CR-HARDENING-GATE — Code-Reviewer Hardening Gate

Agent: `code-reviewer` | Model tier: session (inherit)

**Trigger**: End of every /harden run, after all lens sections are written.

**Context to pass**:
- DEV_ROOT, the mode (AUDIT / REVIEW / PREFLIGHT)
- The consolidated `production/reviews/harden-<date>.md` report
- Any data-only fixes already applied

**Prompt**:
> "Gate the hardening run. Does every finding name its defect class, carry
> CONFIRMED or PLAUSIBLE, cite file:line, and end with a Fix: line? Was every
> security finding given its real severity regardless of score? Were the
> scope gates applied before the diff was read (premise measured, matrix
> built, real question asked)? Is the triage complete — every finding CLOSED /
> QUICK WIN / BACKLOG / ACCEPT with a one-line reason, remediation sequenced
> where order matters, and what to leave alone named explicitly? Are the
> applied data-only fixes listed separately from the queued stories? Return
> CLEAN (no open CRITICAL/IMPORTANT, report complete), CONCERNS [name them],
> or FAIL [blockers remain — list them]."

**Verdicts**: CLEAN / CONCERNS / FAIL
