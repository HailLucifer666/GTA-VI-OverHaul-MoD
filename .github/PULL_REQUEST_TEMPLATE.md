## What changed

<!-- One paragraph. What does this PR do, in plain language? -->

## Why

<!-- Link the issue, or explain the problem this solves. -->

## Proof

- [ ] `.\build.ps1` is green (0 errors, 0 warnings, full suite passes)
- [ ] `.\probe.ps1` finishes green, and every probe went red before it was restored
- [ ] The new or updated test **fails without this change** (paste the failing line)
- [ ] If this adds a dangerous guard, a sabotage probe was added for it

## What this does not cover

<!-- Be honest. What is still unproven, untested, or out of scope? -->

## Safety

- [ ] Story Mode only — no path now acts in multiplayer
- [ ] `ClassicGTA.Contracts` and `ClassicGTA.Core` still reference no `GTA.*` or `LemonUI.*`
- [ ] No game assets, pre-release material, or tuned design data added to the public tree
- [ ] No new absolute paths, usernames, or private URLs

## Screenshots / clips

<!-- For UI or in-game changes. Remove if not applicable. -->
