# Team RELEASE — verified → shipped

First: read `_shared.md` in this folder and apply its conventions (DEV_ROOT
resolution, scaffold, dispatch rules, quality floor, wrap-up) throughout.

**Chair:** release-lead · **Members:** release-code-audit, release-security,
release-gap-compliance, release-cicd, release-e2e-wiring,
release-performance, technical-writer
**Purpose:** the go/no-go gate (precheck's six auditors) followed by the ship
protocol (mythos-ship). The bar is not "CI is green."

## Mode selection (from $BRIEF)

- Brief names a target ("ship to staging", "release v1.2", "go live") → **GATE MODE**
  (full audit → verdict → ship protocol).
- Brief says "is this ready" / "readiness check" / no deploy yet → **CHECK MODE**
  (audit → verdict only, no ship protocol).

## Step 1 — Chair: intake (release-lead, alone)

Brief: adopt role, DEV_ROOT. Establish: what's shipping (repo, branch/commit,
scope), target (staging/production, first release or update to something live),
spec to check against (`docs/PRD.md`, `docs/FEATURES.md`, or "nothing broken"),
environment access (can it actually run?), rollback plan (missing one is a gate
item, not an afterthought). Output an INTAKE SUMMARY.

## Step 2 — Six auditors in parallel

Dispatch all six with the intake summary. Each returns findings tagged
🔴 BLOCKER / 🟠 HIGH / 🟡 MEDIUM / 🟢 LOW + a section verdict:

- **release-code-audit** → line-by-line defect hunt, findings named by kee class
- **release-security** → security gate + secret-and-leak sweep
- **release-gap-compliance** → spec gap analysis (what the spec requires vs what
  exists — derive the set from the spec, not the diff) + dependency/CVE check
- **release-cicd** → CI/CD + deploy verification (build, env vars in every
  environment the release reaches, vendor config resolution order)
- **release-e2e-wiring** → end-to-end feature + wiring check — must actually
  execute the paths, not read code; browser evidence via gstack browse for web
- **release-performance** → perf + reliability with real measurements

## Step 3 — Chair: synthesis + verdict

Consolidate into the RELEASE READINESS REPORT
(`production/release/readiness-<date>.md`). Adversarially verify every 🔴
BLOCKER before including — reproduce it if the environment allows. When
auditors disagree, trust the one that actually executed the path.

Verdict first, findings after — read
`<studio>\.claude\docs\director-gates\rl-release-gate.md`:
**RL-RELEASE-GATE: PASS / PASS WITH NOTES / BLOCK.**

- PASS — zero 🔴, zero unaddressed 🟠. Ship.
- PASS WITH NOTES — zero 🔴, accepted 🟠/🟡 each with explicit owner + follow-up.
- BLOCK — any 🔴. Hand the scoped blocker list to the orchestrator for
  /implement (FIX mode), then re-audit changed scope only.

## Step 4 — Ship protocol (GATE MODE, after PASS/PASS WITH NOTES)

Per `~/.zcode/skills/mythos-ship/SKILL.md` stages 2–4, with **release-cicd**
and **release-security** executing and **release-lead** verifying:

1. Secret sweep run as its own pass (rotate → audit → sweep the class if anything
   was exposed; ship the retest assertion).
2. Deploy order stated explicitly: what applies first, what merges first, what
   must be green. Migrations applied before the code that needs them merges —
   never bulk-apply pending migrations.
3. Staging first; critical journeys actually run; watch logs ten minutes; then
   production; then watch again.
4. Rollback trigger written with a named owner; rollback path tried at least once.
5. Post-deploy note filed; monitoring/alerts confirmed configured before deploy.

## Step 5 — Documentation + wrap-up

**technical-writer** → release notes + changelog + docs updates
(`docs/release-notes/<version>.md`), handoff doc per mythos Stage 4 (what
shipped, what was left and why, known risks, how to verify, watch-outs).

Report: the verdict, report path, what shipped and where, what to watch + the
rollback owner, docs written, and next step: `/harden` after any incident, or
done.
