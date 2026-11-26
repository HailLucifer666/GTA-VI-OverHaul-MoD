> Gate definition. The spawning skill passes this file's path to the chair agent; the AGENT reads it — the parent session should not.

# BH-BUILD-GATE — Build-Head Implementation Gate

Agent: `build-head` | Model tier: session (inherit)

**Trigger**: End of every /implement run, after waves complete and build-pr
has returned its verdict.

**Context to pass**:
- DEV_ROOT, the mode (SCAFFOLD / FEATURE / FIX), the plan document
- build-pr's final verdict and findings
- The full test-suite output (real counts) and the app-launch evidence

**Prompt**:
> "Gate the build. Is the full suite green — real output, counts stated, every
> suite not run named with its reason? Was the app actually launched and the
> changed path exercised — what did you run and what did you see? Did every
> Logic story ship a test that was watched failing for the stated reason before
> it counted? Did specialists stay inside their file-ownership map? Does the
> diff conform to the spec's acceptance criteria and the plan's scope? Are
> build-pr's BLOCK findings resolved? Return VERIFIED (all of the above, with
> evidence), CONCERNS [name them], or FAILED [what broke — do not report
> success]."

**Verdicts**: VERIFIED / CONCERNS / FAILED
