# Classic GTA Overhaul Mod: Master Plan

PRD, MVP, tech stack, architecture, roadmap, risk register and repo plan in one document. Written 26 Nov 2026.

## 1. Reality check (read this first)

Research on current status changed a few assumptions in the original handoff. None of them kill the project, but they change the plan.

**1. GTA VI is on consoles only, with no PC date.** Fan and news sites tracking this say GTA VI is out on PS5 and Xbox Series X|S, that Rockstar has not announced a PC version, and that any PC date is speculation (early 2027 is the rumor). Treat "GTA VI on PC" as an event you wait for, not a date you plan around.

**2. No Script Hook VI exists, and nobody knows what its API will look like.** Script Hook V and OpenIV are built for GTA V's code and do not work on GTA VI. Historically, a script loader for a new Rockstar PC game appeared within days to weeks of launch, and a real mod ecosystem took months. It depends on a third party (the Script Hook author) and on whatever protection Rockstar ships. Nothing you build now can depend on it.

**3. Consequence for the architecture: a "native hash swap" will not be enough.** The original plan assumed a `GameNatives.cs` wrapper where you swap GTA V hashes for GTA VI hashes. A new RAGE version will likely change more than hashes (scripting model, entity APIs, map, UI). The portable part of your mod is the **game logic** (state machines, economy, persistence, menu flow). The non-portable part is the **bridge** to the engine. This plan therefore uses a strict Core / Bridge split (Section 6), where Core never touches a game API.

**4. GTA V is a good test bed for logic, a weak one for "is this feature missing".** Several backlog items already exist in GTA V single player (first-person mode, property purchases, stock market, rampages, carrying every weapon). A mod version of those in GTA V proves your plumbing, not the value of the feature. Items GTA V lacks natively (taxi fares, vigilante/ambulance/fire jobs, a calling-for-services system, a custom business economy) are the best test material. Section 8 rates every item.

**5. The "cut features" list is unverified.** The 26-item list in the handoff is a compiled list with bracketed citations and no real sources. Some claims (tennis "confirmed cut", the pet snake, the reasons given for removals) may be wrong or exaggerated. Phase 0 includes verifying each item against actual gameplay once you or others can see it. Do not put unverified claims in your public README.

**6. GTA V has two branches now.** Legacy and Enhanced. A maintained fork, Script Hook V .NET Enhanced, lets one .NET script run on both and was last updated in July 2026. Target Legacy first (most mature tooling), and test on Enhanced if you own it.

## 2. Vision, goals, non-goals

**Vision.** A modular, open-source "classic sandbox" mod pack that restores cut single-player activities, built so the game-logic half can move to GTA VI on PC with minimal rewriting.

**Goals**

1. Ship a playable, stable v0.1 on GTA V (PC, single player) that you can demo in a 60-second video.
2. Build a clean public repo that shows engineering judgment: architecture, tests, docs, releases.
3. Be ready on day one of a GTA VI PC script loader, with Core logic already tested and a bridge written against the new API within weeks, not months.
4. Stay at $0 and fully legal/ToS-safe.

**Non-goals**

- No online/multiplayer features of any kind.
- No use of leaked builds, console dumps, or reverse-engineering of the game executable.
- No redistribution of Rockstar assets or of Script Hook binaries.
- No adult-content features (the "hirable sex workers" item is out of scope).
- No monetization.
- No attempt to mod GTA VI on consoles (not possible).

## 3. Product Requirements Document (PRD)

### 3.1 Users

- **Primary: single-player GTA V PC players** who install mods with Script Hook V and want classic activities back. They want: one install folder, an in-game menu, no crashes, a README that works.
- **Secondary: the maintainer (you).** You want a codebase where one AI chat session can add one module without breaking the others.
- **Tertiary: recruiters and hiring managers** who spend two minutes on the repo. They want a clear README, a demo, and proof of quality (tests, CI, releases).

### 3.2 Functional requirements

Priority key: **M** = must for MVP, **S** = should (v0.2 to v0.3), **C** = could (later), **X** = GTA VI only.

| ID | Requirement | Priority | Acceptance criteria |
| --- | --- | --- | --- |
| F-00 | Mod framework: module loader, enable/disable per module, central tick scheduler | M | Each module can be disabled in the .ini and the game still runs; one module throwing an exception does not stop others |
| F-01 | In-game menu (LemonUI) with a hotkey to open | M | Menu opens/closes with the configured key, lists all modules with on/off toggles, no input stuck after closing |
| F-02 | Config file (`ClassicSandbox.ini`) with hotkeys and module toggles | M | Missing or corrupt file regenerates defaults; invalid key names fall back to defaults and log a warning |
| F-03 | Logging to `ClassicSandbox.log` | M | Every module start, stop and caught exception is logged with a timestamp |
| F-04 | Safe cleanup on script reload/abort | M | After pressing Insert (reload) ten times, mod-owned peds, vehicles and blips count returns to zero each time |
| F-10 | Pocket Arsenal toggle (grant all weapons, max ammo) | M | Toggle on gives every weapon in the supported list; toggle off restores the previous loadout |
| F-11 | Emergency Dispatch (police, ambulance, fire) | M | Vehicle spawns out of sight, drives to the player, crew acts (heal / extinguish / arrive); everything is despawned within 2 minutes of finishing |
| F-12 | Taxi Hustle side job | M | Full loop: get in a taxi, start job, pick up a passenger, drive them, get paid by distance, repeat or stop; no stuck states |
| F-13 | Persistence of mod state (cash earned, owned items) | S | State survives game restart; a corrupt save file is backed up and replaced, not crashing |
| F-20 | Property and business purchases with passive income | S | Buy from a world marker, income paid per in-game day, ownership saved |
| F-21 | Vehicle dealership (boats, planes, helicopters, submarine) | S | Purchase spawns the vehicle at a set spot; ownership saved |
| F-22 | Vigilante, paramedic, firefighter jobs | S | Each has a start condition, objective loop, reward, and clean abort |
| F-23 | Rampages | S | Timed kill challenge with weapon restriction; clean start/fail/complete |
| F-24 | Stat bars (driving, flying, shooting, stamina) | C | Stats rise with use, persist, and visibly affect at least one gameplay value |
| F-25 | Stock market simulator | C | Simulated prices updating on in-game time, buy/sell via menu, saved |
| F-30 | Mini-games (darts, bowling, tennis, blackjack) | C | One mini-game at a time, only after all S items are stable |
| F-31 | Dual wield, prone crawling, skateboarding | C | Treated as research spikes; ship only if stable (see risks) |
| F-40 | Interior/safe-zone weapon unlocker | X | Defined only after GTA VI PC modding is possible and the restrictions can be observed |

### 3.3 Non-functional requirements

- **Performance:** the mod's own work averages under 1 ms per frame. Anything heavier runs on throttled timers (250 to 1000 ms).
- **Stability:** zero crashes in a 60-minute soak test with all MVP modules enabled.
- **Entity budget:** at most 20 mod-owned entities alive at once, all tracked in one registry and released when done.
- **Compatibility:** works with a clean install of Script Hook V and Script Hook V .NET; documented behaviour with Enhanced.
- **Install:** one folder to copy into `scripts/`, one README page of instructions.
- **Offline only:** the mod refuses to run if it detects GTA Online (see Section 11).

### 3.4 Success metrics

- v0.1 published as a GitHub release with a demo video.
- Zero open crash bugs older than 7 days.
- README has a working install guide that a stranger can follow (test it on a friend or a fresh Windows user profile).
- Core logic unit tests passing in CI on every push.

## 4. MVP definition (v0.1: "The Hustle Pack")

**Why this MVP.** The handoff started with weapon unlocking because it is easy, but in GTA V most of that is already native, so it would not demonstrate anything. The MVP instead proves the architecture with one trivial module (to validate plumbing) and two real ones that GTA V lacks.

**In scope**

1. Framework (F-00 to F-04): loader, menu, config, logging, cleanup.
2. Pocket Arsenal toggle (F-10): the "hello world" module, about an hour of work, validates the full pipeline end to end.
3. Emergency Dispatch (F-11).
4. Taxi Hustle (F-12).

**Out of scope for v0.1:** persistence, purchases, stats, mini-games, anything physics-heavy.

**Definition of done for v0.1**

- All F-00 to F-04, F-10 to F-12 acceptance criteria pass.
- 60-minute soak test passes.
- 10x reload test shows no leaked entities.
- README, LICENSE, CHANGELOG, install guide and a 60-second demo video exist.
- Tagged release `v0.1.0` on GitHub with a zip of the mod files only.

**Estimated effort.** 2 to 3 weeks of evenings for a beginner working with an AI assistant, assuming one module per working session.

## 5. Tech stack ($0)

| Layer | Choice | Notes |
| --- | --- | --- |
| Language | C# | Mature GTA V scripting support |
| Runtime/target | .NET Framework 4.8 class library | Required by Script Hook V .NET v3 |
| IDE | Visual Studio Community | Free for individuals and open-source work |
| Script loader | Script Hook V + Script Hook V .NET (v3 API, use a recent nightly if the stable build lags the game patch) | You link to these in the README; you never redistribute them |
| Enhanced support | Script Hook V .NET Enhanced (optional, test later) | One build can run on Legacy and Enhanced |
| Menus | LemonUI (the SHVDN3 package, from NuGet) | Native-style menus and notifications |
| Input | `System.Windows.Forms.Keys` via the `KeyDown` event | Hotkeys from the .ini |
| Config | .ini via SHVDN's `ScriptSettings` | Simple, human-editable |
| Save data | A second small file (`ClassicSandbox.save.ini` or plain key=value) | Avoid extra third-party DLLs in `scripts/` |
| Unit tests | xUnit or NUnit test project that compiles the Core files | Runs without the game |
| Version control | Git + GitHub (public repo) | Free |
| CI | GitHub Actions (build + tests) | Free for public repos |
| Docs | Markdown in the repo | README, docs folder, CHANGELOG |
| Native reference | Public native databases (for example the FiveM native reference) to check function names | Verify every `Hash.` you use compiles; never trust a name from memory |
| AI assistant | Whichever model you have (free Claude, GLM, DeepSeek, Gemini) | See Section 13 for how to work within small context windows |

**Out of the stack on purpose:** paid tools, decompilers, leaked assets, any online service.

## 6. Architecture

### 6.1 Principle

Three layers. Dependencies point downward only.

1. **Core** (portable): pure C#, no `using GTA;` anywhere. Contains state machines, economy rules, job logic, save/load models, config parsing.
2. **Bridge** (engine-specific): implements interfaces that Core defines. This is the only code that touches `GTA.*`, `Function.Call` and LemonUI.
3. **Host** (entry point): the `Script` subclass that creates the Bridge, hands it to Core, and drives the tick loop.

When GTA VI arrives, you write a new Bridge and a new Host. Core and its tests carry over unchanged.

### 6.2 Core interfaces (examples)

```
IModule          : Name, Enabled, OnStart(), OnTick(dt), OnStop()
IGameBridge      : Player position, spawn/despawn entity, play notification, set waypoint, get time-of-day, is-in-vehicle(type)
IEntityRegistry  : Track(handle), ReleaseAll(), Count
IScheduler       : Every(ms, action), After(ms, action)
ISaveStore       : Load<T>(key), Save<T>(key, value)
IConfig          : GetKey(name, default), GetBool(name, default)
ILog             : Info, Warn, Error
```

Each module (Taxi, Dispatch, Arsenal) depends only on these interfaces. Because they are interfaces, your unit tests can pass fake versions and test a whole taxi job in milliseconds without launching the game.

### 6.3 Folder structure

```
ClassicSandbox/
  README.md  LICENSE  CHANGELOG.md  .gitignore
  docs/            (PRD, architecture, test plan, install guide, decisions log)
  src/
    ClassicSandbox.Mod/        (Host + Bridge, references GTA DLLs)
      Core/                    (no GTA references; compiled into the mod)
        Modules/  Economy/  Persistence/  Config/
      Bridge/                  (GTA V implementations)
      Host/ClassicSandboxScript.cs
  tests/
    ClassicSandbox.Core.Tests/ (links Core/*.cs, uses fake bridges)
  .github/workflows/build.yml
```

### 6.4 Rules that keep it stable

- **One `Script` class** drives all modules from a single tick, instead of ten scripts each running their own loop.
- **Never run heavy work every frame.** Use the scheduler for anything over a trivial check.
- **Every spawned thing goes through the entity registry.** The registry is cleaned on `Aborted` (script reload) and when a job ends; it uses `MarkAsNoLongerNeeded()` or `Delete()` as appropriate.
- **Every module call is wrapped in try/catch** in the Host so one bug is logged and that module is disabled, instead of killing everything.
- **No magic numbers or hashes in module logic.** They live in a single constants file in the Bridge.
- **State machines are enums plus a switch** in Core, with one test per transition.

## 7. Roadmap with phase gates

Each phase has an exit gate. Do not start the next phase until the gate passes.

### Phase 0: Foundations (week 1)

- Install GTA V (Legacy), disable BattlEye in the launcher, install Script Hook V and Script Hook V .NET, confirm a stock sample script loads (look for the load message and check `ScriptHookVDotNet.log`).
- Create the GitHub repo (public from day one, MIT license, README stub).
- Create the solution per Section 6.3.
- Write a one-page verification sheet for the "cut features" list (feature, verified yes/no, source).
- **Gate:** a Hello World script prints a notification in game and reloads with Insert.

### Phase 1: MVP build (weeks 2 to 4)

- Framework (F-00 to F-04), then Arsenal, then Dispatch, then Taxi. One module per session.
- Unit tests for each state machine in Core.
- **Gate:** v0.1 definition of done (Section 4) met.

### Phase 2: Economy and persistence (weeks 5 to 9)

- Save store (F-13), then properties and businesses (F-20), dealership (F-21), then jobs (F-22), rampages (F-23).
- **Gate:** 2-hour soak test passes with save/load across restarts. Release v0.2.

### Phase 3: Extras (optional, weeks 10+)

- Stats (F-24), stock simulator (F-25), first mini-game (F-30).
- Dual wield, prone, skateboarding (F-31) only as time-boxed research spikes (3 sessions each). If it is not stable by the end of a spike, write down what you learned and drop it.
- **Gate:** each extra ships only if it passes the same cleanup and soak tests.

### Phase 4: GTA VI watch (event-driven)

- Track: PC announcement, Script Hook VI (or equivalent) release, Rockstar's modding stance, anti-tamper behaviour.
- Before launch, pre-write the "port checklist" (what each Core interface needs from the new Bridge).
- **Gate:** a loader exists and a Hello World runs.

### Phase 5: GTA VI port

- Write the new Bridge for one module (Arsenal or Taxi) first, prove it, then migrate module by module.
- Add F-40 once the real restrictions can be observed.
- Re-verify every feature on the "cut" list against the real game.

## 8. Feature map: all 26 items

"GTA V test" shows whether GTA V can test the logic. "Difficulty" is for a beginner using AI help.

| # | Feature | Module | GTA V test | Difficulty | Plan |
| --- | --- | --- | --- | --- | --- |
| 1 | Buy houses/safehouses | Properties | Partly (V has its own property system) | Medium | v0.2, custom ownership list + markers |
| 2 | Passive-income businesses | Properties/Economy | Yes | Medium | v0.2 |
| 3 | Buy boats, planes, helicopters | Dealership | Yes | Medium | v0.2 |
| 4 | Submarines | Dealership | Yes (Kraken exists in game files; test by spawning) | Medium | v0.2, after #3 |
| 5 | Stock market | Stocks | Already native in V | Medium | v0.3, custom simulator so logic is portable |
| 6 | Web browser in game | n/a | No | Very hard | Out of scope |
| 7 | Home decorating | n/a | No | Very hard | Out of scope (needs custom props and interiors) |
| 8 | Taxi missions | Taxi | Yes | Medium | **MVP** |
| 9 | Vigilante / paramedic / firefighter | Jobs | Yes | Medium | v0.2 |
| 10 | Rampages | Rampage | V has them; mod version tests logic | Medium | v0.2 |
| 11 | Tennis | Minigames | No natural venue logic | Very hard | Later, optional |
| 12 | Darts, bowling | Minigames | Needs custom props and physics | Hard | Later, optional |
| 13 | Movie theaters | n/a | No | Hard | Out of scope |
| 14 | Amusement rides | n/a | V has a Ferris wheel and coaster in the world | Hard | Out of scope for now |
| 15 | Casino and card games | Minigames | UI-only blackjack is feasible | Medium to hard | Later; UI-only version first |
| 16 | Skateboarding | Spike | Physics heavy | Very hard | Research spike only |
| 17 | Mission replay and medals | Replay | Native in V | n/a | Check GTA VI behaviour first |
| 18 | Free-roam first-person | Camera | Native in V | Easy in V | v0.3 toggle, GTA VI-dependent |
| 19 | Pocket arsenal | Arsenal | V already allows it, test only | Easy | **MVP** (hello world) |
| 20 | RPG skill bars | Stats | Native in V; mod bars test logic | Medium | v0.3 |
| 21 | Prone crawling | Spike | Animation heavy | Very hard | Research spike only |
| 22 | Dual wield | Spike | Existing V mods prove it is possible but fragile | Hard | Research spike only |
| 23 | Hirable sex workers | n/a | n/a | n/a | Out of scope |
| 24 | Friend hangouts | Companions | Needs custom AI | Hard | Later |
| 25 | Call emergency services | Dispatch | Yes | Easy to medium | **MVP** |
| 26 | Buy pets (dogs) | Companions | Yes (animal peds exist) | Medium to hard | Later |

## 9. Testing and QA plan

**Automated (no game needed).** Core unit tests for: taxi state machine, fare calculation, dispatch lifecycle, economy (income per day), save/load round trip, config parsing with bad input.

**In-game manual checklist (run before every release)**

1. Fresh load: notification appears, menu opens, all MVP modules listed.
2. Reload with Insert 10 times: entity count stays at baseline.
3. Taxi: complete 5 jobs in a row; abort one mid-ride by leaving the car; die mid-job; change weapon mid-job.
4. Dispatch: call each service in daylight, at night, indoors, while already in a car, while wanted.
5. Load a save and quick-save during an active job: no stuck blips or ghost peds.
6. 60-minute soak with all modules enabled; watch frame time.
7. Run with the second most popular mod set you can install (for example a trainer) to catch conflicts.

**Release gate.** A release does not go out with an open crash or leak bug.

## 10. Risk register and review checklist

| Risk | Likelihood | Impact | Mitigation |
| --- | --- | --- | --- |
| No GTA VI PC release, or PC release with no usable script loader | Medium | High | The GTA V version is a complete product on its own; Core is portable |
| GTA VI uses anti-tamper that blocks script loaders | Medium | High | Do not fight it; wait for community tools; stay within rules |
| Hallucinated native or API names from AI | High | Medium | Compile after every change; check names in an official native reference or SHVDN docs before use |
| Entity leaks causing slowdown | High | Medium | Registry + 10x reload test + entity budget |
| Script Hook breaks after a game patch | Medium | Medium | Pin tested versions in README; use nightly SHVDN if needed |
| Beginner gets stuck in a long AI session | High | Medium | One module per session, handoff notes (Section 13) |
| DMCA / policy trouble | Low | High | Section 11 rules |
| Physics/animation features never stabilize | High | Low | Time-boxed spikes, optional |
| Burnout on a long solo project | Medium | High | Ship v0.1 fast; small releases |

**Review checklist (replaces the persona ritual).** Before accepting AI-written code, ask the assistant to answer yes/no to each, and compile it yourself:

1. Does every method and `Hash.` name exist in SHVDN3 or the native reference? (Check, do not trust.)
2. Is anything spawned tracked in the registry and cleaned on abort?
3. Is anything heavy running every frame?
4. Is every module call wrapped in try/catch with logging?
5. Does Core contain any `GTA.*` reference?
6. Are there hard-coded hashes or coordinates outside the constants file?
7. Does it touch online mode, other people's assets, or anything outside offline single player?

This keeps the useful part of the Council + Red Team idea at a fraction of the token cost, which matters on a free tier with limited context.

## 11. Legal and policy

This is practical guidance, not legal advice; read the current official policy yourself.

- **Offline, single player only.** Do not use the mod in GTA Online. Add a guard that disables the mod if the player is in an online session.
- **Check the current Rockstar / Take-Two single-player mod position** and link it in your README. As far as I know, the long-standing stance tolerates non-commercial single-player mods that avoid online play and avoid other companies' IP and real brands. Confirm the current wording before release.
- **GTA VI has no official mod policy yet.** Re-check when the PC version is announced.
- **No Rockstar assets in your repo** (no extracted models, textures, audio, maps). Code only.
- **Do not redistribute Script Hook V, SHVDN or LemonUI binaries.** Link to their official pages.
- **No leaked builds, console dumps or executable reverse-engineering.** Anything GTA VI-specific waits for public community tools.
- **No monetization or donations tied to the mod.**
- **Name carefully.** Avoid putting the trademarked game title in the project name. Use a name like "Classic Sandbox Overhaul" and describe it as "a mod for GTA V" in the README.
- **License your own code** (MIT is simple and recruiter-friendly) and state that the project is unaffiliated with Rockstar Games or Take-Two.

## 12. Repo and portfolio plan

The repo is also your resume, so build it to be skimmed in two minutes.

- **README (top to bottom):** one-line pitch, a GIF or video of Taxi Hustle, feature list with checkmarks, install steps (numbered, tested), controls table, known issues, roadmap, link to the architecture doc, legal note.
- **docs/:** this plan split into `PRD.md`, `ARCHITECTURE.md`, `TEST_PLAN.md`, `DECISIONS.md` (a short log: "I chose X over Y because Z").
- **Releases:** tag each version, attach a zip, keep a CHANGELOG.
- **CI badge:** GitHub Actions building the solution and running Core tests; the green badge is a cheap signal of quality.
- **Issues/Projects:** turn the roadmap phases into GitHub milestones and the PRD rows into issues. Closing issues in public shows process.
- **One short write-up** (blog post or README section): "How I designed a game mod so it can move to a new engine". This is the artifact that shows engineering thinking to a hiring manager.
- **Be honest in the README** about what is verified and what is planned.

## 13. Working with AI on a free tier

- **One module per chat.** Start each chat by pasting: the Core interfaces file, the module spec (the PRD row plus acceptance criteria), and the current error text if debugging. Do not paste the whole project.
- **Keep a `HANDOFF.md`** in the repo with: current phase, what works, what is broken, the next single task, and a list of files touched. Paste it at the start of the next session.
- **Always compile and run before moving on.** If the code does not compile, paste the exact error back; do not ask for a rewrite from scratch.
- **Ask for complete files** for new modules, and small diffs for fixes.
- **Commit after every working step** so you can always roll back.
- **Starter prompt template:**

> You are helping me build module \[X\] of a GTA V single-player mod in C# (.NET Framework 4.8, ScriptHookVDotNet3, LemonUI). Follow the Core/Bridge rule: no `GTA.*` references in Core. Here are the interfaces: \[paste\]. Here is the spec and acceptance criteria: \[paste\]. Write the complete files. Then list every GTA/SHVDN API or Hash you used so I can verify they exist, and answer the 7-point review checklist yes/no.

## 14. Day-1 checklist

1. Install GTA V (Legacy) and let it run once. In the Rockstar launcher, set it to offline mode and disable BattlEye for single player.
2. Install Script Hook V (copy `ScriptHookV.dll` and `dinput8.dll`, skip the native trainer if you do not want it) and Script Hook V .NET (the `ScriptHookVDotNet*` files) into the game folder. Make a `scripts` folder.
3. Install Visual Studio Community with the ".NET desktop development" workload.
4. Create a Class Library targeting **.NET Framework 4.8**. Add references to `ScriptHookVDotNet3.dll` (from the game folder) and the LemonUI SHVDN3 package from NuGet.
5. Add a post-build step that copies your output DLL (and LemonUI's DLL) into `<GTA V>\scripts\`.
6. Write the Hello World: a `Script` subclass that shows a notification on load and on a hotkey. Launch the game, confirm it appears, press Insert to reload.
7. `git init`, add `.gitignore` for Visual Studio, commit, and push to a new public GitHub repo.
8. Open the next chat with the Section 13 template and ask for the framework (F-00 to F-04).

## 15. Glossary

- **Script Hook V:** the plugin that lets custom scripts run inside GTA V.
- **SHVDN:** Script Hook V .NET, which lets scripts be written in C#.
- **LemonUI:** a library for native-looking menus.
- **Native / hash:** an engine function identified by a number, called through `Function.Call`.
- **Bridge:** the code that talks to the game engine.
- **Core:** the game-independent logic of the mod.
- **Soak test:** running the game for a long time with the mod on to find leaks and crashes.
- **Entity registry:** one list that tracks every ped, vehicle and blip the mod creates, so it can clean them up.
