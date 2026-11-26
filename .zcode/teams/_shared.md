# Shared team conventions (identical in every team file)

**Studio root** = `E:\WIFI Hack\Claude-Code-Dev-Studios`
**Role files** = `<studio>\.zcode\agents\<role>.md` — canonical ZCode role definitions.

## DEV_ROOT resolution (do this first, every run)

1. The brief names a folder or project path → that is DEV_ROOT.
2. The current working directory contains a software project (any of: `package.json`,
   `pyproject.toml`, `go.mod`, `Cargo.toml`, `requirements.txt`, `*.sln`, `Gemfile`,
   `composer.json`, `.devstudio/config.yaml`, a `src/` with code) → cwd is DEV_ROOT.
3. The cwd IS the studio repo (contains `.zcode/teams`) → do NOT write project files
   here; ask the user which folder the project lives in (offer to create one).
4. Otherwise → ask the user (offer: "create a new project folder as <cwd>\<slug>?").

Never write project artifacts into the studio repo. The studio repo holds only
roles, playbooks, gates, templates, and the installer.

## Scaffold (once per DEV_ROOT, only what's missing)

Create `docs/`, `specs/`, `tests/`, `production/reviews/`, `.devstudio/`.
If `.devstudio/config.yaml` is missing, write:

```yaml
project: <name>
studio: software-dev
created: <YYYY-MM-DD>
qa: {level: standard}
```

If the project already has its own layout, respect it — add only the missing
folders above.

## Dispatching roles (the Agent tool)

- Preferred: spawn with the registered `subagent_type` (e.g. `head-pm`) —
  the studio roles are registered globally.
- Fallback if a type is not available: `subagent_type: general-purpose`, prompt begins:
  "Read `E:\WIFI Hack\Claude-Code-Dev-Studios\.zcode\agents\<role>.md` and
  adopt that persona fully, then:" followed by the task brief.
- EVERY brief must state: (a) DEV_ROOT, (b) the task, (c) the exact destination
  path(s) for deliverables — this activates the role's orchestrated-run write
  exception so it writes directly instead of round-tripping approvals.
- Independent roles run in PARALLEL (single message, several Agent calls).
  Phases are sequential: chair first, specialists parallel (waves where the
  playbook says so), chair synthesis last.
- **Concurrency fallback:** the plan may allow only ONE subagent at a time. If a
  parallel dispatch fails with a concurrency error, re-dispatch the same briefs
  sequentially in the order listed — deliverables are identical, just slower.
- If a subagent returns questions instead of a deliverable, relay them to the user
  with your own recommendation — never guess silently.

## Decision discipline

- The run produces drafts + a consolidated decision list; it does not stall for
  approvals mid-run. Roles write only to the paths the briefs name.
- Every user-facing decision is presented with options + a labeled recommendation.
- Stack choices: 2–3 scored candidates for THIS app, one labeled recommendation,
  flagged as a user decision, proceed with the recommendation unless the user
  overrides.

## Quality floor (applies to every team, from the kee methodology)

- Tests ship with behavior changes. A fix or feature without a test is a claim.
- The full suite runs before any "done": all green or the run does not end.
- The app is actually launched and used once per run that changes behavior —
  a green suite that never ran the app has shipped a broken app before.
- Never claim something is verified that was not run.

## Wrap-up (you, the orchestrator)

Read every deliverable to confirm it exists and is complete. Report: files written
(full paths), test results (real output, not claims), decisions needed, and the
recommended next team. If a subagent failed or produced something thin, say so
plainly — do not paper over it.
