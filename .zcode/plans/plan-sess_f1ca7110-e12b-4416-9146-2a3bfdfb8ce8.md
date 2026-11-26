# Plan — Visual, community-first README for the Classic GTA Overhaul

## Context

The repo's front page is currently an engineering status sheet: honest, but written for a code reviewer, not for the PC gaming community you want to recruit. The goal is a GitHub front page that looks like a serious open-source project, moves a little, shows enough to excite people and keeps the recipe private — and that invites players and modders to help build the GTA VI-ready version before GTA VI PC modding even exists.

The code is further along than the README suggests: `v0.6.3-hardened`, 116 tests, 12 scripted probes, a per-slot mod wallet, a strict persistence codec, and adapter-seam tests. Nothing has run inside GTA V yet, and the README should say so proudly rather than hide it — "we test before we ship" is the brand.

## Decisions locked from your answers

- **Money:** free only. No sale language, no "full version" tease, no paywall mention anywhere in the public README.
- **Secrets:** code public, design detail private. The PRD, roadmap detail, tuning numbers and coordinates stay out of the public repo; the README teases by name and status only.
- **Motion:** self-hosted SVG only. No shields.io, no external badge services, no GIFs (there is no real footage until Phase 6B).
- **Poster:** cropped to remove every Rockstar mark — the R* logo, the ESRB badge, the "compatible with GTA V & GTA VI" badge, and the "Grand Theft Auto VI" wordmark. The neon "Classic GTA Overhaul" title, tagline, icon row and vehicles stay.
- **License:** GPL-3.0.

> Because GPL-3.0 permits commercial redistribution, the README must stop calling the project "Non-Commercial". It becomes "Free and open source under GPL-3.0, with no paywalled features in this repository" — which is true, and keeps the free promise without contradicting the license.

## Deliverables

### 1. `README.md` — section by section

1. **Animated hero banner** — `<div align="center">` + `<img src="docs/media/banner.svg">`, 1280×300. Neon Vice City palette pulled from the poster (hot pink `#FF3FA4`, cyan `#22D3EE`, deep purple `#1B1035`, sunset orange `#FF8A3D`). Title "CLASSIC GTA OVERHAUL" with a draw-on stroke, a slow horizon-grid drift and one scanline. Tagline: *"The classic GTA sandbox, rebuilt in the open — for GTA V today, for GTA VI when PC modding opens."*
2. **Hero poster** — centered `<img src="docs/media/hero-vice-city.jpg">`, max-width 900px. One line beneath, always visible: *"Unofficial fan project. Not affiliated with, endorsed by or sponsored by Rockstar Games or Take-Two Interactive. No game assets are redistributed."*
3. **Status strip** — four hand-authored SVG badges in `docs/media/badges/`: `build passing`, `116 tests`, `live in-game: pending Phase 6B`, `GPL-3.0`. Updated by hand at each release; no external services.
4. **The pitch** — three short paragraphs: what it is; what works today (offline-verified, menu-wired); where it is going (engine-agnostic core, adapter swapped when GTA VI opens).
5. **Feature grid — "not all shown"** — this is where the secrecy lives:
   - **In the build today** — 6 hero cards with emoji and one line each: Taxi Shift, 911 Dispatch, Pocket Arsenal + Safe-Zone, LCN Stocks, Blackjack, Guard Dogs.
   - **Also wired** — 5 compact chips: safehouse ledger, passive business income, Kraken delivery, emergency side-jobs, companion hangouts.
   - **In the lab** — 6 chips, names only: ClassicNet, furnishing, rampages, mission medals, first-person, RPG skills.
   - **Classified** — 3 cards showing `???` with "revealed after the first live session". These are the nine unbuilt features, which are absent from the public repo, so the mystery is real.
6. **Animated architecture diagram** — `docs/media/architecture.svg`, four layers with a pulse travelling along dashed connectors. Plain-language caption: *"The brain never touches GTA's code. Swap one layer and the whole mod moves to the next game."*
7. **"The first live run hasn't happened yet"** — an honest panel with a checklist (30 hot-reloads, 30-minute soak, entity high-water mark, frame time) and an invitation: *"Be in the first test session."* This replaces the missing gameplay footage with a call to action instead of faking one.
8. **Build it in 60 seconds** — code block with `.\build.ps1` and `.\probe.ps1`, prerequisites (Windows, .NET Framework 4.8, VS Build Tools), and a link to `docs/BUILDING.md`.
9. **Join the crew** — the recruitment section: role cards (C# gameplay, SHVDN/native research, LemonUI/UX, 3D/art, QA, docs/video), a prominent line that *the highest-value help right now is anyone with GTA V Legacy installed who can run the Phase 6B protocol*, a four-step "your first PR" path, and links to `CONTRIBUTING.md` and the community invite.
10. **Roadmap** — three public phases, no dates: offline-verified core → Phase 6B live verification → GTA VI adapter when a public scripting interface exists.
11. **Support** — star, report, test. No donation link, no paid tier.
12. **Credits & legal** — GPL-3.0, story mode only, no online or anti-cheat interaction, vanilla assets only, no cross-title ripping, non-affiliation.
13. **Footer** — "Built by the community. For the community." + a star CTA.

Every image gets real alt text; every claim is a status, not a promise.

### 2. Media assets (`docs/media/`)

| File | Spec |
|---|---|
| `hero-vice-city.jpg` | Crop of the 1254×1254 poster, roughly `y=620..1100` (verify visually), resized to 1254×480, JPEG q82, target under 350 KB |
| `banner.svg` | 1280×300, self-contained, SMIL/CSS animation only, no `<script>`, no external fonts or URLs, target under 30 KB |
| `architecture.svg` | 1000×400, animated dashed connectors + `animateMotion` pulse, under 30 KB |
| `badges/build.svg`, `tests.svg`, `status.svg`, `license.svg` | Flat neon badges, ~1 KB each |
| `social-preview.png` | 1280×640, optional, for the repo's social preview setting |

`docs/media/` is used because `Assets/` is gitignored and on Windows a lower-case `assets/` would resolve to the same ignored folder.

### 3. Open-source front files

- `LICENSE` — GPL-3.0 full text.
- `CONTRIBUTING.md` — dev setup, branch-per-change, the "test with every fix" rule, PR checklist, where to ask.
- `CODE_OF_CONDUCT.md` — Contributor Covenant.
- `.github/ISSUE_TEMPLATE/bug_report.yml`, `feature_request.yml`, `config.yml`; `.github/PULL_REQUEST_TEMPLATE.md`.
- `docs/FEATURES.md` — the public status table (no tuning numbers).
- `docs/BUILDING.md` — player install + developer build.
- `docs/FAQ.md` — "Is this GTA VI?", "Does it work online?", "Will it be paid?", "Why does it need GTA V?".
- Optional `.github/workflows/build.yml` (windows-latest, runs `.\build.ps1`) so "build passing" is a fact, not a claim.

### 4. Publication path — one decision to approve

Publishing the current private repo would expose `PROGRESS_TRACKER.md` (private URL and `E:\` paths), `AGENTS.md`, `TEAM-USAGE.md`, `.zcode/` and `.claude/` (125 lines containing the Windows account name), the six design docs, and 10.59 MiB of assets in history.

**Recommended: create a new public repository with a single clean initial commit.** It contains only `README.md`, `LICENSE`, `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, `.github/`, `docs/media/`, `docs/FEATURES.md`, `docs/BUILDING.md`, `docs/FAQ.md`, `src/`, `tests/`, `build.ps1`, `probe.ps1`, `.gitignore`, `.gitattributes`, `nuget.config`, and the solution. The existing private repo stays the development repo. This keeps every secret, avoids a history rewrite, and can be reviewed before anyone sees it.

## Implementation order

1. Crop and compress the poster; visually confirm no Rockstar logo, ESRB mark, compatibility badge or GTA VI wordmark survives.
2. Author `banner.svg`, `architecture.svg` and the four badges.
3. Write `README.md` against the verified numbers (run `.\build.ps1` first; use the real count).
4. Add LICENSE, CONTRIBUTING, CODE_OF_CONDUCT, issue/PR templates, the three public docs.
5. Render locally, then push to a private draft repo and check the page on github.com.
6. Only after your go-ahead: create the clean public repo and push the curated tree.

## Verification

- `.\build.ps1` green before publishing; the test number in the badge matches the run.
- Every relative link resolves: `docs/media/*`, `LICENSE`, `docs/*.md`.
- Safety grep on the public tree: zero hits for `C:\Users`, `E:\`, `WIFI Hack`, `HailLucifer`, `GTA VI BUILD`.
- Every SVG loads as XML, contains no `<script>` and no external URL; every `<img>` has alt text.
- GitHub render check: hero, badges, feature grid, `<details>` blocks and animations all render; page weight under ~1.5 MB total.
- Post-publish: verify images load through Camo on a logged-out browser.
- Re-check the cropped hero at 100% for any surviving trademark.

## Guardrails

- No Rockstar or ESRB marks in any published image.
- No sale, paywall or "full version" language anywhere.
- No design docs, tuning tables, coordinates or internal tracker in the public repo.
- No claim of live in-game verification until Phase 6B actually runs.
- No external badge or image services.
- The art is AI-generated; confirm you are comfortable with its usage rights before publishing. If not, the banner alone can carry the page.

## The one thing I need from you

A community link for the "Join the crew" section (Discord, Matrix or Reddit invite). Until you have one, the section links to the repo's Discussions tab and a placeholder comment marks where the invite goes. I will not invent a link.