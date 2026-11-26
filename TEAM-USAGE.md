# Software Dev Studio — Usage

Your software studio is callable from **any ZCode session, in any folder**.
All real content lives at `E:\WIFI Hack\Claude-Code-Dev-Studios`; the only
things on C: are tiny pointer files (`C:\Users\acarg\.zcode\agents\` and
`C:\Users\acarg\.zcode\skills\{spec,implement,harden,release,devteam}`).

## The four teams

| Command | Phase | Chair + members | What you get |
|---|---|---|---|
| `/spec` | idea → spec | head-pm + software-architect (consult) | PRD, tech-stack ADR, architecture, features, constitution, feature specs |
| `/implement` | spec → code | build-head + uxui, database, backend, frontend, devops, qa, security, build-pr | working code in waves, with tests, suite green, app launched |
| `/harden` | code → verified | code-reviewer + security-auditor, performance-optimizer, test-auditor | findings by kee defect class, secret sweep, perf measurements, probed tests — triaged |
| `/release` | verified → shipped | release-lead + six auditors, technical-writer | RELEASE READINESS REPORT (PASS / PASS WITH NOTES / BLOCK), then ship protocol, release notes |
| `/devteam` | router | — | roster, or routes any request to the right team |

## Calling them

```
/spec a habit tracker with streaks and reminders
/implement scaffold
/implement add CSV export
/harden                     ← audit the whole repo
/harden review my PR
/release staging
/devteam                    ← roster
/devteam fix the login 500  ← routes to implement (FIX mode)
```

Any of the 23 roles can also be dispatched directly through the Agent tool
(`head-pm`, `code-reviewer`, `release-lead`, …) for a one-person consultation.

## Auto-trigger (no slash command needed)

The global routing block in `C:\Users\acarg\.zcode\AGENTS.md` routes
substantive software-dev requests in any folder: "build X", "fix this bug",
"review this repo", "is it ready to ship" dispatch the matching team on their
own. Precedence: explicit slash commands win; game-dev requests route to the
game studio; quick questions get direct answers (cost guard), never a team
spawn. To remove auto-triggering, delete the routing block from that file.

## Where files go

Project docs (`docs/`, `specs/`, `production/`) and code are written into the
**project folder** of the session you call from (DEV_ROOT — the playbook asks
if the folder is unclear). The studio repo itself is never polluted with
project files.

## Costs

`/implement` is the heavy one (up to 9 subagents across 4 waves + synthesis);
`/spec` is one agent + one consult; `/harden` and `/release` are 4–7 in
parallel plus chair synthesis. The router's cost guard keeps quick questions
from triggering teams. Multi-phase pipelines run one phase per request with a
checkpoint between phases.

## Under the hood

- Roles: `<studio>\.zcode\agents\<role>.md` — adapted from your in-house
  `C:\Users\acarg\.claude\agents\` library (see `_studio-setup\ADAPTATION-SPEC.md`)
- Team playbooks: `<studio>\.zcode\teams\{spec,implement,harden,release,devteam}.md`
- Gates & templates referenced by the roles: `<studio>\.claude\docs\`
- Rebuild the C: shims: `node E:\WIFI Hack\Claude-Code-Dev-Studios\_studio-setup\build-teams.mjs`
- Aliases: `/head-pm` and `/precheck` (the old dead commands) now point at
  /spec and /release respectively.
