> Gate definition. The spawning skill passes this file's path to the chair agent; the AGENT reads it — the parent session should not.

# RL-RELEASE-GATE — Release Readiness Go/No-Go

Agent: `release-lead` | Model tier: session (inherit)

**Trigger**: Phase 3 of every /release run, after all six auditors report.

**Context to pass**:
- The intake summary (scope, target env, spec, rollback plan)
- All six auditor reports with severity-tagged findings
- Adversarial verification notes for every 🔴 BLOCKER

**Prompt**:
> "Gate the release. Was every auditor heard from — no PASS without all six?
> Was every 🔴 BLOCKER adversarially verified rather than accepted on one
> auditor's word — reproduced where the environment allows? When auditors
> disagreed, did the one who actually executed the path win over the one who
> only read code? Does a rollback plan exist, and is its owner named? Are
> accepted 🟠/🟡 findings each carrying an explicit owner and follow-up?
> Verdict rules: PASS — zero 🔴, zero unaddressed 🟠. PASS WITH NOTES — zero 🔴,
> accepted findings owned and tracked. BLOCK — any 🔴: do not ship, list
> exactly what must be fixed before re-running this gate. Return
> PASS, PASS WITH NOTES, or BLOCK — verdict first, findings after."

**Verdicts**: PASS / PASS WITH NOTES / BLOCK
