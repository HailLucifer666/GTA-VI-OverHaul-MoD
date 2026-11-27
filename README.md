<div align="center">
  <img src="docs/media/banner.svg?v=2" alt="Classic GTA Overhaul — cinematic GTA VI-style sunset with a neon VI, palms and a city skyline" width="100%">

  <h1>Classic GTA Overhaul</h1>

  <p><b>The classic GTA sandbox, rebuilt in the open.</b><br>
  A community-built C# mod for <b>GTA V Legacy (PC)</b> today — and for <b>GTA VI</b> the day PC modding opens.</p>

  <p>
    <img src="docs/media/badges/build.svg" alt="build: passing" height="28">
    <img src="docs/media/badges/tests.svg" alt="tests: 136/136" height="28">
    <img src="docs/media/badges/status.svg" alt="live in-game: Phase 6B pending" height="28">
    <img src="docs/media/badges/license.svg" alt="license: GPL-3.0" height="28">
  </p>
</div>

<div align="center">
  <img src="docs/media/hero-vice-city.jpg" alt="Classic GTA Overhaul key art: two characters in a neon Vice City sunset, with sports cars, a police cruiser and a skyline" width="900">
</div>

<p align="center"><sub>Unofficial fan project. Not affiliated with, endorsed by or sponsored by Rockstar Games or Take-Two Interactive. No game assets are redistributed.</sub></p>

---

## What this is

The old sandbox games let you do everything. Buy a safehouse. Run a taxi shift. Call the cops, then outrun them. Waste an afternoon at the casino. Adopt a dog that actually follows you around.

This project rebuilds those systems — the ones modern entries trimmed back — as **modular C# engines with real tests**, wired into a single in-game hub. It is built by players, in the open, one tested system at a time.

Two promises shape every decision:

- **It runs on GTA V today.** No waiting for a new game. Every system compiles against the real `ScriptHookVDotNet3` and `LemonUI`, and the whole suite runs offline in seconds.
- **It is built for the next game.** The gameplay logic never references `GTA.*`. When GTA VI PC modding exists, the adapter is the only layer that has to change — not the 100+ systems behind it.

## What's in the build today

**Legend:** ✅ wired into the `F7` hub · 🧪 engine built and tested, not yet wired · 🔒 still under wraps

| | Feature | What it does | Status |
|:--:|:--|:--|:--:|
| 🚕 | **Taxi Shift** | Pick up fares, beat the par time, earn the distance and speed tip. Your own cab is never deleted. | ✅ |
| 🚨 | **911 Dispatch** | Police, ambulance or fire crew spawn on a nearby road, drive to you and do their job on arrival. | ✅ |
| 🔫 | **Pocket Arsenal + Safe Zones** | Every valid weapon, max ammo, non-destructive. Combat controls and your owned weapon come back inside restricted interiors. | ✅ |
| 📈 | **LCN Stocks** | Buy, sell and hold shares; prices move on a market tick and your portfolio survives a reload. | ✅ |
| 🃏 | **Blackjack** | Rules-complete 21 with soft aces, doubles and 3:2 naturals — played with story-mode cash, never real money. | ✅ |
| 🐕 | **Guard Dogs** | Buy a dog, summon it, and command follow, sit or guard. It walks off on its own when dismissed. | ✅ |

**Also wired:** 💵 Story Cash & Mod Wallet menu · 🏠 safehouse ownership · 🏢 passive business income · 🚤 Kraken delivery · 🚑 emergency side-jobs · 🤝 companion hangouts · 🚗 **GTA VI Arcade Restorations** (Instant Car Theft, Classic Wanted LoS Cooldown + Pay 'n' Spray Bribe, No Refuel/Survival/Vignette) · 🎮 **Classic Cheats & Turf Wars** (3-wave district takeovers + passive turf income)

**In the lab** (built and unit-tested, waiting for their adapter): in-game web hub · furniture placement · rampages · mission medals · first-person camera · RPG skills

**Classified** — three systems stay under wraps until the first live session. No spoilers, no promises.

## By the numbers (real, measured)

Every number below comes directly from the repo build, test runner, and live `GTA5.exe 1.0.3889.0` logs — nothing estimated.

| Metric | Measured Value | How it is verified |
|:--|:--|:--|
| **Production C# source** | **84 files · 7,732 lines** | `31` Contracts · `23` Core · `4` Infrastructure · `23` GTA V Adapter |
| **Test suite** | **12 files · 3,557 lines · 136/136 PASS** | `.\build.ps1` (`Release \| x64`, `TreatWarningsAsErrors=true`, `0` warnings) |
| **Sabotage probes (`/kee`)** | **17/17 RED → restored GREEN** | `.\probe.ps1` mutates real guards, asserts exact `[FAIL]`, restores byte-identical |
| **Scheduled modules & UI** | **11 `IModModule`s · 8 `F7` submenus** | 3-tier fault-isolated scheduler (`0ms` / `250ms` / `1000ms`), Gate D Online guard |
| **Engine leakage in Core** | **0 `GTA.*` / `LemonUI.*` refs** | Verified across all `54` files in `ClassicGTA.Contracts` + `ClassicGTA.Core` |
| **Hardening & red-team passes** | **4 passes (`v0.6.1`–`v0.6.3` + Phase 7)** | Closed `C-01`–`C-02`, `I-01`–`I-09`, `A-01`–`A-04`, and `H7-01`–`H7-04` |
| **Live GTA V Legacy test** | **1.0.3889.0 verified (`2026-11-27`)** | `F7` hub, Story Cash & Mod Wallet grants, `3` asset purchases, atomic `.dat` + `.bak1–.bak3` |

### What we tried & proved so far
- **Strict atomic persistence:** Poisoned primary `.dat` + truncated `.bak1` across persisted engines; confirmed automatic recovery from `.bak2` and zero silent overwrite of corrupt ledgers.
- **Online fail-closed boundary:** Verified every registered scheduler module (`11/11`) executes `0` gameplay adapter calls when `IsOnlineSession == true`, and active Territory Wars abort without deleting entities.
- **Three-way entity ownership (`20`-entity cap):** Proved player/borrowed vehicles are never deleted on abort, while refused spawns over budget delete the untracked handle cleanly.
- **Live GTA V Legacy (`1.0.3889.0`):** Confirmed clean load under ScriptHookV `3889.0` + SHVDN `v3.7.0-nightly.206` with `0` errors in `ClassicGTA.log`; full 30-reload & 30-minute soak test is next in Phase 6B.

## Under the hood

<div align="center">
  <img src="docs/media/architecture.svg" alt="Four layers: contracts, core, infrastructure and a swappable GTA adapter, with animated data flow" width="900">
</div>

Game logic never references `GTA.*` or `LemonUI.*`. That is not a style preference — the build enforces it, and it is why the mod can outlive the game it currently runs on.

## The honest bit

**First live load on GTA V Legacy (`1.0.3889.0`, ScriptHookV `3889.0`, SHVDN `v3.7.0-nightly.206`) is verified** — the `F7` hub, Money & Mod Wallet grants, asset purchases (`business.malibu_club`, `sub.kraken`, `safehouse.del_perro`), and atomic `.dat` + 3-backup persistence in `ClassicGTA_Data\` all ran cleanly with zero errors. Full stress and soak verification is still in progress.

That is what **Phase 6B** tracks:

- [x] First in-game load on GTA V Legacy (`1.0.3889.0` · `2026-11-27`)
- [ ] 30 hot reloads without an entity or blip leak
- [ ] 30-minute soak with frame-time telemetry
- [ ] Entity high-water mark recorded
- [ ] Every wired feature touched once, by hand

If you own **GTA V Legacy on PC** and can install ScriptHookV, you are the single most useful person in this project right now. Open an issue titled *"Phase 6B test run"* and we will walk you through the protocol.

## Build it

```powershell
# Windows · .NET Framework 4.8 · Visual Studio Build Tools
.\build.ps1     # restores pinned packages, builds 5 projects, runs 136/136 tests
.\probe.ps1     # 17 sabotage probes: break a guard, watch the right test go red, restore
```

No global installs, no package cache outside the repo. `build.ps1` pulls the two pinned dependencies into `.packages\` on first run and verifies their SHA-256.

Player install steps and developer setup live in [`docs/BUILDING.md`](docs/BUILDING.md).

## Join the crew

This is a community project, not a solo release. There is room for every skill level.

| You bring | You would work on |
|:--|:--|
| **C# gameplay** | State machines, economy rules, edge cases, tests. |
| **SHVDN / native research** | Spawning, tasks, blips, cameras — and proving behaviour in-game. |
| **LemonUI / UX** | The `F7` hub, menus, notifications, readability. |
| **3D / art** | Icons, key art, trailers once footage exists. |
| **QA** | Test plans, regression passes, the Phase 6B protocol. |
| **Docs / video** | Guides, changelogs, clips, community answers. |

**Your first PR in four steps:**

1. Pick an issue tagged `good first issue` — or open one that describes what you want to build.
2. Branch, make the change, and **ship a test with it**. A fix without a test is a claim, not a fix.
3. Run `.\build.ps1` and `.\probe.ps1` locally. Both must be green.
4. Open the PR and fill in the template.

Read [`CONTRIBUTING.md`](CONTRIBUTING.md) before you start. Be decent to each other — [`CODE_OF_CONDUCT.md`](CODE_OF_CONDUCT.md).

<!-- Community invite goes here once the Discord/Matrix link exists. -->
**Community:** use the **Discussions** tab to introduce yourself, ask questions or claim work.

## Roadmap

| Phase | Goal | State |
|:--|:--|:--:|
| **Core** | Engine-agnostic systems, economy, persistence, Phase 7 GTA VI Arcade Pack, 136 tests, 17 probes | ✅ Done |
| **Phase 6B** | Live GTA V Legacy verification: reloads, soak, frame time, leaks | 🔄 In Progress (first load ✅) |
| **GTA VI** | Swap the adapter when a public PC scripting interface ships | ⏳ Waiting on the game |

## Support the project

This mod is **free and open source, forever** — no paywalls, no locked features, no paid tier. The way to support it is to use it, break it, and tell us what broke.

⭐ Star the repo · 🐞 Report a bug · 🧪 Run the Phase 6B protocol · 💬 Answer someone's question

## Credits & legal

- **License:** [GPL-3.0](LICENSE). Fork it, study it, improve it — keep it open.
- **Story Mode only.** No network or anti-cheat interaction; the mod hard-blocks itself in multiplayer.
- **Vanilla assets only.** No ripped models, no cross-title asset copying, no pre-release material.
- **No real-money gambling.** Casino play uses story-mode cash.
- **Key art** is AI-assisted and de-branded for this fan project. All trademarks belong to their owners.

<div align="center">
  <br>
  <b>Built by the community. For the community.</b><br>
  <sub>If you have ever wanted the old sandbox back — this is how we get it.</sub>
</div>
