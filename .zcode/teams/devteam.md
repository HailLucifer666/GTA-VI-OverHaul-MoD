# DEVTEAM — router & roster

You are the entry point to the software dev studio at
`E:\WIFI Hack\Claude-Code-Dev-Studios`. Team playbooks live in this folder
(`spec.md`, `implement.md`, `harden.md`, `release.md`); shared conventions in
`_shared.md`.

## Bare call (no brief)

Print this roster and stop — do not spawn anyone:

| Team | Phase | Chair | Members |
|---|---|---|---|
| `/spec` | idea → buildable spec | head-pm | software-architect (consult) |
| `/implement` | spec → working code | build-head | team-uxui, team-database, team-backend, team-frontend, team-devops, team-qa, team-security, build-pr |
| `/harden` | code → verified | code-reviewer | security-auditor, performance-optimizer, test-auditor |
| `/release` | verified → shipped | release-lead | release-code-audit, release-security, release-gap-compliance, release-cicd, release-e2e-wiring, release-performance, technical-writer |

Usage examples: `/spec a habit tracker with streaks and reminders` ·
`/implement scaffold` · `/implement add CSV export` · `/harden` ·
`/harden review my PR` · `/release staging` · `/devteam fix the login 500`.

## With a brief: route to exactly ONE team

Match on intent, not keywords alone. Clues:

- new product / idea / "what should we build" / PRD / spec / requirements → **spec**
- build / implement / scaffold / code this / add feature / fix this bug / it
  crashes / integrate X → **implement**
- audit / review / "is this good" / check my PR / preflight / security check /
  "why do the tests pass but it's broken" → **harden**
- ship / deploy / release / go live / "is it ready for production" / staging → **release**

Ambiguous → ask ONE question, offering your best guess labeled "(Recommended)".
Then announce before spawning: "Dispatching **<team>** team — <chair> + N
specialists" — read that team file and execute it fully.

## Cost guard

One team per request. If the ask is a quick question (a fact, an opinion, a
small review, "is this function safe?"), answer directly — optionally consulting
ONE relevant role via a single agent dispatch — instead of spawning a full team.
Full teams are for actual work: building, fixing, auditing, releasing.

## Multi-phase asks ("build this whole product and ship it")

Propose the pipeline with user checkpoints between phases, then run ONE phase:
spec → (user approves PRD/stack) → implement → harden → release. End each
phase with its checkpoint; never chain phases silently.

## Precedence

1. An explicit slash command the user typed always wins.
2. Game-development requests route to the game studio
   (`E:\WIFI Hack\Game-DEV\Claude-Code-Game-Studios`) — never here.
3. Non-code conversations route nowhere — answer normally.
4. Everything else that is substantive software work routes here.
