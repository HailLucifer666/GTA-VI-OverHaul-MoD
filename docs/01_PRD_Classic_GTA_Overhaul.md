# Product Requirements Document (PRD): "Classic GTA" Overhaul Mod

> [!IMPORTANT]
> **Project Strategy:** Phase-Gated Prototyping in **GTA V (PC)** using C# (`ScriptHookVDotNet3` + `LemonUI.SHVDN3`) behind an isolated `GameNatives.cs` abstraction layer, enabling a clean native-hash and coordinate swap when **GTA VI (PC)** and its community script hook release.

---

## 1. Executive Summary & Vision

Grand Theft Auto VI (console release: November 2026; PC release pending) modernized the series toward grounded, story-driven realism—cutting or restricting 26 beloved single-player sandbox mechanics from earlier titles (*GTA: San Andreas*, *GTA IV*, *GTA V*).

The **"Classic GTA" Overhaul Mod** is a modular, free, open-source single-player C# mod pack that restores these classic sandbox freedoms. Every module can be toggled independently via an in-game `LemonUI` menu or `ClassicGTA.ini`, keeping the mod stable, lightweight, and tailored to each player's taste.

### Core Design Pillars
1. **100% Modular & Toggleable:** Every restored feature is an independent `IModModule` with its own state machine and `.ini` toggle. Disabling one module never breaks another.
2. **Engine-Agnostic Abstraction:** Zero raw GTA V native hashes inside feature logic. All engine calls route through `GameNatives.cs` so porting to *GTA VI PC* is a translation exercise, not a rewrite.
3. **Zero-Cost, Beginner-Friendly Stack:** Built strictly with \$0 tools (Visual Studio Community, `.NET Framework 4.8`, `ScriptHookV`, `ScriptHookVDotNet3`, `LemonUI.SHVDN3`). Every code deliverable is 100% compile-ready with zero `// TODO` stubs.
4. **Offline Story Mode & Policy-Safe:** Strictly offline single-player (BattlEye disabled). Uses only native in-game assets/models—never imports copyrighted third-party assets or assets ripped from other Rockstar titles.

---

## 2. Target User & Operational Constraints

| Constraint | Specification |
| :--- | :--- |
| **Developer Profile** | Beginner using AI-driven ("vibe coding") workflow; needs copy-paste-ready `.cs` files and explicit build checklists. |
| **Budget** | **\$0** (no paid IDEs, paid assets, cloud servers, or paid UI libraries). |
| **Runtime Environment** | Windows 10/11, GTA V PC (Legacy Edition recommended for maximum SHVDN3 stability; Enhanced Edition supported via SHVDN3 Enhanced forks), Story Mode only. |
| **Hot-Reload Workflow** | `ScriptHookVDotNet3` `Insert` key reload support—all modules must clean up entities, blips, and HUD state on `Aborted`. |
| **Performance Budget** | `< 0.5 ms` average per frame across all active Tier 1–2 modules; heavy world queries throttled to `250 ms – 1000 ms`. |

---

## 3. Complete 26-Feature Coverage Matrix

Every cut feature from *GTA VI* is mapped below to its engineering tier, GTA V prototype approach, GTA VI PC porting strategy, and Rockstar legal-risk assessment.

### Category A: Money, Property & Purchases (#1–#7)

| # | Cut Feature | Tier | Prototype Implementation (GTA V) | GTA VI PC Port Strategy | Legal / DMCA Risk |
| :-: | :--- | :-: | :--- | :--- | :--- |
| **1** | **Purchasing Houses & Safehouses** | Tier 2 | Coordinate-based world markers + blips + `LemonUI` purchase menu; unlocks interior teleport/spawn, bed save trigger, and garage slot persisted in `ClassicGTA.ini`. | Swap map coordinates & interior IPL/MLO IDs in `WorldLocations.cs`. | **Low** — Uses native map interiors only. |
| **2** | **Passive-Income Businesses** | Tier 2 | Purchasable storefronts/clubs/cab depots; tracks in-game days elapsed and deposits weekly passive cash + optional defend-business micro-events saved to `.ini`. | Swap business coordinates, names, and money-award native hash. | **Low** — Pure script economy logic. |
| **3** | **Buying Boats, Planes & Helicopters** | Tier 2 | Dock/helipad/hangar dealership markers + `LemonUI` catalog menu; deducts cash, spawns vehicle at designated pad, tracks ownership in `.ini` for free re-delivery. | Swap vehicle model hashes & dock/airfield spawn coordinates. | **Low** — Spawns native game vehicles only. |
| **4** | **Purchasable & Drivable Submarines** | Tier 2 | Submersible dealership at coastal docks (spawns `Submersible` / `Kraken` in GTA V) with underwater sonar blip/treasure loop. | If GTA VI lacks native sub models at launch, gate behind Tier 3 custom vehicle add-on or use native watercraft until community vehicle packs exist. | **Low** (if using native/original models; **High** if ripping GTA V models into GTA VI — *prohibited*). |
| **5** | **Stock Market Trading (LCN / BAWSAQ-style)** | Tier 2 | `LemonUI` Stock Exchange terminal with simulated tick-based price curves, portfolio persistence in `.ini`, and optional hit-contract triggers that shift rival stock prices. | 100% portable C# simulation; only currency read/write native changes. | **Low** — Pure math/UI simulation. |
| **6** | **Traditional In-Game Web Browser** | Tier 3 | `LemonUI` / Scaleform "Classic Net" portal menu linking dealerships, real estate, stock market, and emergency directory under one unified hub. | Portable menu hub; optionally hook GTA VI's native Scaleform/UI layer once documented. | **Low** — Original parody text, no real brands. |
| **7** | **Home Decorating & Furnishing** | Tier 3 | Prop placement mode inside owned safehouses (`World.CreateProp`, raycast placement, rotation controls, prop list saved to `ClassicGTA_Furniture.ini`). | Swap prop model hashes and safehouse bounding boxes. | **Low** — Uses native in-game props only. |

### Category B: Side Jobs, Activities & Mini-Games (#8–#16)

| # | Cut Feature | Tier | Prototype Implementation (GTA V) | GTA VI PC Port Strategy | Legal / DMCA Risk |
| :-: | :--- | :-: | :--- | :--- | :--- |
| **8** | **Taxi Driving Missions** | Tier 2 | Full 5-state machine (`Idle -> Searching -> Pickup -> EnRoute -> Payout`) detecting cab class, spawning sidewalk fares, GPS routing, tip timer, and distance payout. | Swap taxi vehicle hash list and sidewalk node natives. | **Low** |
| **9** | **Vigilante, Ambulance & Firefighter Missions** | Tier 2 | Emergency vehicle detection + procedural wave missions: Vigilante (pursuit targets), Paramedic (injured peds -> hospital), Firefighter (spawned fire coordinates -> extinguish). | Swap emergency vehicle hashes, fire/particle natives, and hospital/station coordinates. | **Low** |
| **10** | **Timed Weapon-Specific Rampages** | Tier 2 | Skull-icon world triggers; locks player to specific weapon with infinite ammo, spawns hostile waves, 2-minute countdown HUD, cash + medal payout. | Swap trigger coordinates, weapon hashes, and ped group hashes. | **Low** |
| **11** | **Tennis Mini-Game** | Tier 3 | Simplified court-zone arcade rally script (ball prop physics + AI opponent ped + swing timing input + score HUD). | High physics/animation coupling; rebuild against GTA VI animation dictionary names. | **Low** |
| **12** | **Darts & Bowling Mini-Games** | Tier 3 | Bar/alley trigger markers; Darts (reticle sway + raycast board scoring) & Bowling (lane camera + ball impulse physics + pin knock-down detection). | Portable scoring state machine; swap prop/anim hashes. | **Low** |
| **13** | **Movie Theaters & Cinemas** | Tier 3 | Theater entrance marker -> seat camera -> plays native in-game TV/video render targets or custom royalty-free parody clips. | **Never** bundle copyrighted video/audio; use native in-game TV channels only. | **Medium** (strictly native channels only to stay **Low**). |
| **14** | **Amusement Park Rides** | Tier 3 | Pier marker -> attaches player camera/ped to animated coaster/wheel path splines (or triggers native GTA V pier scripts during prototyping). | Spline-based camera/seat attachment at Leonida pier coordinates. | **Low** |
| **15** | **Casino & Card Games (Blackjack / Poker)** | Tier 2/3 | Full rules-complete Blackjack & Video Poker playable via `LemonUI` + HUD card overlay at bar/lounge markers (zero real-money gambling). | 100% portable C# card engine; swap lounge coordinates & cash native. | **Low** (strictly virtual in-game cash only). |
| **16** | **Skateboarding** | Tier 3 | Custom prop attached to feet + momentum/balance physics tick + jump/ollie impulse + locomotion animation blend. | High RAGE physics/ragdoll coupling; defer until GTA VI bone/anim hashes are mapped. | **Low** |

### Category C: Gameplay, Combat & Exploration (#17–#22)

| # | Cut Feature | Tier | Prototype Implementation (GTA V) | GTA VI PC Port Strategy | Legal / DMCA Risk |
| :-: | :--- | :-: | :--- | :--- | :--- |
| **17** | **Mission Replay & Medal Ratings** | Tier 2/3 | Custom "Sandbox Contracts" & auto-snapshot save-point manager (`ClassicGTA_Replay.ini`) that tracks completion time, headshots, damage taken, and awards Bronze/Silver/Gold medals. | Pure C# telemetry tracker + snapshot teleport/loadout restore (does *not* alter official story continuity). | **Low** (avoids modifying official Rockstar story scripts). |
| **18** | **Free-Roam First-Person Mode** | Tier 2 | Custom `GameplayCamera` / attached head-bone `Camera` (`AttachTo(headBone, offset)`) with mouse-look pitch/yaw + vehicle hood/dashboard camera toggle (`V` or `F8`). | Swap head-bone ID (`SKEL_Head`) and camera native hashes. | **Low** |
| **19** | **Unlimited "Pocket Arsenal" Weapon Wheel** | **Tier 1** | `F10` toggle + menu option granting every valid `WeaponHash` with `MaxAmmo`, bypassing carry/trunk limits, plus Safe-Zone/Interior firing unlocker. | Swap weapon hash table & inventory-limit natives. | **Low** |
| **20** | **RPG Character Skill Bars** | Tier 2 | Custom XP/Stat tracker (`Stamina`, `Driving`, `Flying`, `Shooting`, `LungCapacity`) saved to `ClassicGTA.ini`, applying real gameplay buffs (stamina regen, recoil/accuracy, breath duration) + HUD bars. | 100% portable C# XP system; swap buff native hashes. | **Low** |
| **21** | **Prone Crawling** | Tier 3 | Stance state machine (`Standing -> Crouched -> Prone`) driving low-crawl animation dictionary + ground-normal alignment + aiming pitch clamp. | Requires GTA VI animation dictionary & collision capsule natives. | **Low** |
| **22** | **Dual-Wielding Weapons** | Tier 3 | Secondary weapon prop attached to left-hand bone (`IK_L_Hand`) + synchronized alternating muzzle flash, bullet raycast/projectile spawn, and ammo deduction. | Swap hand-bone ID, weapon prop hashes, and `SHOOT_SINGLE_BULLET_BETWEEN_COORDS`. | **Low** |

### Category D: Social Interactions & World Services (#23–#26)

| # | Cut Feature | Tier | Prototype Implementation (GTA V) | GTA VI PC Port Strategy | Legal / DMCA Risk |
| :-: | :--- | :-: | :--- | :--- | :--- |
| **23** | **Classic Street Companions / Health Boost** | Tier 2 | Optional opt-in classic sandbox mechanic (disabled by default in `.ini` for streamer-safety): horn-honk pickup, secluded stop, classic health-to-125% boost, clean fade-out (no explicit animations). | Swap ped spawn & health native hashes. | **Low** (kept non-explicit, strictly optional). |
| **24** | **Friend Hangouts (Companion AI)** | Tier 2 | `LemonUI` "Call Buddy" menu spawning a friendly companion ped (`PedGroup`), driving together to bars/activities, assisting in combat, and dismissing cleanly. | Swap companion ped models & `SET_PED_AS_GROUP_MEMBER` hash. | **Low** |
| **25** | **Calling Emergency Services (911 Dispatch)** | **Tier 1** | `F9` / `LemonUI` Dispatch menu summoning Police cruiser (engages hostiles), Ambulance (paramedic exits & heals player to 100%), or Fire Truck (extinguishes nearby fires). | Swap vehicle/ped hashes & `TASK_VEHICLE_DRIVE_TO_COORD`. | **Low** |
| **26** | **Purchasable & Walkable Pets (Guard Dogs)** | Tier 3 | Pet Shop marker + `LemonUI` menu to buy a dog (`a_c_rottweiler` / `a_c_shepherd` / `a_c_husky`), follow AI, "Sit / Fetch / Guard / Attack Threat" commands, persisted in `.ini`. | Swap animal model hashes & animal AI task natives. | **Low** |

---

## 4. Non-Functional Requirements (NFRs)

1. **Zero Entity & Blip Leaks:** Every `Ped`, `Vehicle`, `Prop`, and `Blip` created by any module MUST be registered with a central `EntityTracker` and either marked `MarkAsNoLongerNeeded()` or deleted cleanly on mission end or script `Aborted`.
2. **Tick-Loop Discipline:**
   - **Per-frame (`0 ms`):** Strictly reserved for `pool.Process()` (LemonUI), control-override calls (`EnableControlThisFrame`), and active camera/HUD rendering.
   - **Throttled (`250 ms – 1000 ms`):** All interior checks, distance checks, world marker proximity scans, stat XP ticks, and passive-income timers.
3. **Zero Hallucinated APIs:** Every `ScriptHookVDotNet3` class, method, and `GTA.Native.Hash` hex value must be verified against SHVDN3 v3.6+ before code release.
4. **Rockstar / Take-Two Policy Compliance:**
   - Strictly **\$0 / Non-Commercial** (no paywalls, no Patreon-locked builds, no crypto/NFTs).
   - **No Cross-Title Asset Ripping** (never copy GTA V `.ydr`/`.ytd` files into GTA VI).
   - **No Story Rewrites** (never modify official mission scripts or canon storylines).
   - **Offline Story Mode Only** (never inject into GTA Online or bypass BattlEye for multiplayer).

---

## 5. Success Metrics (Definition of Done)

- **Build Health:** `0` compiler errors, `0` compiler warnings in Visual Studio Community (`.NET Framework 4.8`, Release x64).
- **Runtime Stability:** `0` unhandled exceptions in `ScriptHookVDotNet.log` across a 30-minute stress test + 10 consecutive `Insert` hot-reloads.
- **Performance:** `< 1 FPS` delta on a mid-range GPU with all Tier 1 & Tier 2 modules enabled simultaneously.
- **Portability:** 100% of native hashes and map coordinates isolated inside `Core/GameNatives.cs` and `Core/WorldLocations.cs`.
