# Software dev studio auto-routing (installed 2026-11-06)

This repo (`E:\WIFI Hack\Claude-Code-Dev-Studios`) hosts Arghya's
software-dev-studio agent system: four teams of registered subagents, with
playbooks under its `.zcode\teams\` folder. It is the sibling of the game
studio at `E:\WIFI Hack\Game-DEV\Claude-Code-Game-Studios`.

**Applies to software-development work.** If the session is about game
development, use the game-studio routing instead. If it is not a coding task
at all, ignore this section.

## Auto-trigger: route the request without waiting for a slash command

| The request is about... | Dispatch |
|---|---|
| a new product idea, PRD, requirements, "what should we build" | **spec** team |
| building / implementing / scaffolding / fixing / integrating code | **implement** team |
| auditing, reviewing a diff/PR, preflight, security check, "why do the tests pass but it's broken" | **harden** team |
| shipping, deploying, releasing, go/no-go readiness | **release** team |
| mixed / unclear / "show the studio" | **devteam** router |

## How to execute a route

1. Announce first: "Dispatching **\<team\>** team — \<chair\> + N specialists."
2. Read `E:\WIFI Hack\Claude-Code-Dev-Studios\.zcode\teams\<team>.md` and
   follow it exactly (it begins with shared conventions in `_shared.md`).
3. One team per request. A quick factual question gets a direct answer —
   optionally ONE consult agent — never a full team spawn.
4. Multi-phase asks ("build this product end to end"): propose the pipeline
   (spec → implement → harden → release) and run ONE phase, ending with a
   user checkpoint.
5. Project files are written into the user's project folder (DEV_ROOT per the
   team files), never into this studio repo.

## Precedence

Explicit slash commands always win. Game-dev requests belong to the game
studio's routing block. Non-coding conversations route nowhere.
