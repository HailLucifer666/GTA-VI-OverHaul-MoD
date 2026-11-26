\*\*Council + Red Team protocol: ACTIVE.\*\* I re‑audited the one remaining unverified native assumption in Module 1 before writing this package, and the Red Team caught a real bug: the previous draft read ped config flag \*\*208\*\* as "weapon switching," but flag 208 is documented as `DisableExplosionReactions`, not weapon switching \[\[4]]. The correct block‑switching flag is \*\*48\*\* (`BlockWeaponSwitching` in the upstream `PedConfigFlagToggles` sequence, anchored by the verified values 32 = `WillFlyThroughWindscreen` and 78 = `IsAimingGun`) \[\[1]]. The code below is corrected accordingly.



Everything that follows is the \*\*complete, copy‑paste‑ready handoff package\*\*: master plan, every supporting document, and the audited Module 1 source.



\---



\# PART 1 — MASTER STEP‑BY‑STEP PLAN



\## Phase 0 — Evidence \& compliance (Day 1)

1\. Create the repo and the folder tree in Part 2.

2\. Record your \*\*exact GTA V edition + executable version\*\* (Legacy vs Enhanced) in `docs/12\_RESEARCH\_EVIDENCE.md`.

3\. Read `docs/10\_POLICY\_AND\_SAFETY.md`; confirm you will stay offline, non‑commercial, and use no leaked/cross‑title assets.

4\. \*\*Gate:\*\* no GTA VI native hashes, no leaked files, no unofficial ports anywhere in the repo.



\## Phase 1 — Development foundation (Day 1–2)

5\. Install VS Community 2022 + ".NET desktop development" + .NET Framework 4.8 targeting pack.

6\. Create \*\*Class Library (.NET Framework)\*\*, target \*\*4.8\*\*, platform \*\*x64\*\*, name `ClassicSandbox.Modules`.

7\. Add NuGet refs: `ScriptHookVDotNet3 3.6.0`, `LemonUI.SHVDN3 2.2.0`; add `System.Windows.Forms`.

8\. Install runtime: ScriptHookV + matching SHVDN nightly + LemonUI SHVDN3 files (see `06\_SETUP\_GUIDE.md`).

9\. Set `ReloadKeyBinding=Insert` in `ScriptHookVDotNet.ini`; configure post‑build copy.

10\. \*\*Gate:\*\* a trivial script loads and reloads via Insert without duplicates.



\## Phase 2 — MVP‑A: Module 1 (Day 2–4)

11\. Add `WeaponAndZoneUnlocker.cs` (Part 3).

12\. Build; fix any compiler error by capturing code+line+package versions.

13\. Run smoke tests R01–R10 and failure tests F01–F06 (`07\_TEST\_PLAN.md`).

14\. \*\*Gate:\*\* arsenal grants, F10 toggles, override restores, reload leaves one menu instance.



\## Phase 3 — MVP‑B: classic loop (Week 2–3)

15\. Implement `PersistenceService` (atomic JSON save + backup + schema version).

16\. Implement `EconomyService` + one purchasable property + passive income.

17\. Implement taxi state machine (`Idle→Searching→Pickup→EnRoute→Payout`).

18\. Implement one emergency dispatch with bounded entities + cleanup.

19\. \*\*Gate:\*\* earn → spend → save → reload → ownership persists → income accrues.



\## Phase 4 — Expanded sandbox (Week 4+)

20\. Dealerships, vigilante, rampages, stock simulator — each with start/cancel/success/failure/cleanup + entity caps.



\## Phase 5 — Advanced research (only after feasibility spike)

21\. First‑person camera, companions, animals, prone, dual‑wield, skateboarding, minigames. Each stays in \*\*Research\*\* until a host‑engine technique is proven.



\## Phase 6 — Future official PC adaptation (no date promised)

22\. Begin only when all six gates in `11\_GTA\_VI\_MIGRATION\_GATE.md` pass.



\---



\# PART 2 — SUPPORTING DOCUMENT PACKAGE



Create these files exactly. Contents are complete.



\## `README.md`

```markdown

\# Classic Sandbox Overhaul

Optional, original, single-player "classic GTA" mechanics for GTA V (PC),

architected for a future official GTA VI PC adapter. NOT affiliated with Rockstar.



\## Status

Phase 2 (MVP-A). Module 1 (WeaponAndZoneUnlocker) implemented; compiler/runtime

verification pending on your machine. See docs/12\_RESEARCH\_EVIDENCE.md.



\## Hard rules

\- Offline Story Mode only. Never GTA Online with mods.

\- No leaked console files, no unofficial ports, no invented GTA VI hashes.

\- Original mechanics; no cross-title asset imports; non-commercial.



\## Quick start

See docs/06\_SETUP\_GUIDE.md. Build -> copy DLL to GTA V/scripts -> press Insert.



\## Docs

docs/00..12 + docs/adr/. Tests in tests/.

```



\## `LICENSE` (your original code only)

```text

MIT License

Copyright (c) 2026 Classic Sandbox Overhaul contributors

Permission is hereby granted, free of charge, to any person obtaining a copy

of this software and associated documentation files (the "Software"), to deal

in the Software without restriction, including without limitation the rights

to use, copy, modify, merge, publish, distribute, sublicense, and/or sell

copies of the Software, and to permit persons to whom the Software is

furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all

copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR

IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,

FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE

AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER

LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,

OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE

SOFTWARE.

```



\## `THIRD\_PARTY\_NOTICES.md`

```markdown

\- ScriptHookVDotNet: zlib license (upstream repo).

\- LemonUI: MIT license (upstream repo).

\- ScriptHookV: freeware by Alexander Blade; do not redistribute game assets.

\- This project ships NO Rockstar game assets, textures, models, audio, or maps.

\- Rockstar, GTA are trademarks of Take-Two/Rockstar. Used descriptively only.

```



\## `.gitignore`

```gitignore

bin/

obj/

.vs/

\*.user

\*.suo

packages/

TestResults/

```



\## `docs/00\_PROJECT\_CHARTER.md`

```markdown

\# Charter

Purpose: restore classic sandbox \*mechanics\* (earn/spend/own/persist) as optional

single-player systems; keep logic portable for a future official PC adapter.

Scope: GTA V prototyping now; GTA VI adapter only after official PC + legal tools.

Constraints: $0 toolchain; beginner-friendly complete code; offline; non-commercial;

no leaked/cross-title content; no multiplayer interference.

Governance: every architectural decision/code/bugfix passes Architect + RAGE

Specialist + Red Team before release (see ADRs).

Success metrics: (1) reload-stable modules, (2) persistent economy loop,

(3) zero entity leaks, (4) no unverified native hashes shipped.

```



\## `docs/01\_PRD.md`

```markdown

\# Product Requirements Document

\## Problem

Modern GTA removed player agency (property, side jobs, broad arsenal). Players want

optional classic loops without touching official story or multiplayer.

\## Persona

PC single-player modder, beginner-friendly install, wants persistent progression.

\## User stories \& acceptance criteria

US-1 Arsenal: As a player I press F10 to open an arsenal and grant weapons.

&#x20; AC: Given Story Mode loaded, When F10 pressed, Then menu opens; granting fills

&#x20; ammo to engine max; selecting equips; no crash on reload.

US-2 Switch override: As a player I can keep weapon switching enabled.

&#x20; AC: When enabled, switching works; when disabled/aborted, tracked ped's original

&#x20; block-switch state is restored if the ped still exists.

US-3 Property: As a player I buy one safehouse and keep it after reload.

&#x20; AC: Given enough cash, purchase deducts once; ownership persists; income accrues

&#x20; per in-game day with an offline cap.

US-4 Taxi: As a player I complete a fare and get paid by distance.

&#x20; AC: State machine reaches Payout exactly once; cancel on exit/death cleans entities.

US-5 Dispatch: As a player I summon an ambulance that heals on arrival.

&#x20; AC: Heal only when vehicle reaches player; bounded active dispatches; cleanup on abort.

\## Functional requirements

WPN-01..07 (arsenal, curated valid hashes, engine-max ammo, manual select, switch

override, restore-on-stop, no universal-bypass claim).

SVC-01..06 (dispatch, caps, tracking, cleanup, arrival-heal, fire-suppression separate).

TAX-01..06 (detect, states, one passenger/destination, pay-on-success, safe cancel,

no double payout).

OWN-01..07 (funds check, persistence, no dup purchase, time-based income, offline cap,

atomic save, malformed-config recovery).

\## Non-functional requirements

NFR-1 no full-world scans per frame; NFR-2 validate handles; NFR-3 unsubscribe+release

on abort; NFR-4 backup+atomic save; NFR-5 compile vs stable SHVDN3; NFR-6 clear UX;

NFR-7 offline only; NFR-8 no redistributed assets; NFR-9 one responsibility/module;

NFR-10 pure rules separated from engine calls.

\## Out of scope

Prone, dual-wield, skateboard, sports/casino minigames, full FPS replacement, custom

animations, cross-title maps, GTA VI native integration (until Phase 6 gates).

```



\## `docs/02\_MVP\_DEFINITION.md`

```markdown

\# MVP

\## MVP-A (foundation)

VS project; build/deploy; arsenal; switch override; notifications; abort cleanup;

manual tests. Exit: load/use/disable/reload repeatedly with no duplicate menus or

retained overrides.

\## MVP-B (classic loop)

One property + persistence + passive income + one taxi job + one dispatch + save

recovery. Exit demo: taxi fare -> cash -> buy property -> reload -> ownership remains

\-> income accrues.

\## Not in MVP

All Tier-3 mechanics and any GTA VI integration.

```



\## `docs/03\_FEATURE\_CATALOG.md`

```markdown

\# Feature catalog (screenshot backlog = hypotheses, not spec)

| # | Feature | Status | Priority | Release |

|1|Buy houses/safehouses|Hypothesis|P1|MVP-B|

|2|Passive-income businesses|Hypothesis|P1|MVP-B|

|3|Buy boats/planes/helis|Hypothesis|P2|0.3|

|4|Submarines|Hypothesis|P2|0.3|

|5|Stock market|Hypothesis|P2|0.4|

|6|Web browsers|Hypothesis|P3|Research|

|7|Home decorating|Hypothesis|P3|Research|

|8|Taxi missions|Hypothesis|P1|MVP-B|

|9|Vigilante/ambulance/fire|Hypothesis|P1/P2|MVP-B/0.3|

|10|Rampages|Hypothesis|P2|0.3|

|11|Tennis|Hypothesis|P3|Research|

|12|Darts/bowling|Hypothesis|P3|Research|

|13|Cinemas|Hypothesis|P3|Research|

|14|Park rides|Hypothesis|P3|Research|

|15|Casino/card games|Hypothesis|P3|Research|

|16|Skateboarding|Hypothesis|P3|Research|

|17|Mission replay/medals|Hypothesis|P3|Research|

|18|Free-roam FPS|Hypothesis|P3|Research|

|19|Pocket arsenal|Confirmed-feasible|P0|MVP-A|

|20|RPG stat bars|Hypothesis|P2|0.4|

|21|Prone|Hypothesis|P3|Research|

|22|Dual-wield|Hypothesis|P3|Research|

|23|Prostitutes|Declined (policy)|-|-|

|24|Friend hangouts|Hypothesis|P3|Research|

|25|Emergency dial|Hypothesis|P1|MVP-B|

|26|Pets|Hypothesis|P3|Research|

Rule: nothing moves Hypothesis->Planned without a host-game feasibility spike.

```



\## `docs/04\_ARCHITECTURE.md`

```markdown

\# Architecture

Layers: Script entry -> Feature modules/state machines -> Gameplay services ->

IGameAdapter -> GtaVGameAdapter -> verified SHVDN3 APIs.

Future: FutureGameAdapter beside GtaVGameAdapter (NOT implemented; no guessed API).

\## Project-owned interface (not a SHVDN API)

public interface IModModule { string Id {get;} void Initialize(); void Tick(); void Shutdown(); }

Services introduced on demand: IWeaponService, IDispatchService, IEconomyService,

IOwnershipService, IPersistenceService, IWorldMarkerService, INotificationService, IClockService.

\## GameNatives wrapper rule

Prefer verified SHVDN wrappers. Unavoidable raw natives live ONLY in GtaVGameAdapter

with name+hash source+params+return+tested build recorded. Never place speculative

GTA VI hashes beside GTA V hashes.

\## Tick rules

No full-world scans per frame; throttle via Interval or Game.GameTime deltas;

validate handles; bounded entity counts.

\## Entity lifecycle

Track spawned ped/vehicle/blip; MarkAsNoLongerNeeded or Delete on completion/cancel/

death/abort; never mark the player ped as no-longer-needed.

\## Taxi state machine

Idle->Searching->Pickup->EnRoute->Payout; cancel edges return to Idle with cleanup;

Payout guarded by a completed-once flag.

\## Persistence (MVP-B)

scripts/ClassicSandbox/{settings.ini, profile.json, profile.backup.json, logs/}

profile.json: {schemaVersion:1, cashDelta, ownedProperties\[], ownedVehicles\[],

businessAccumulators\[], stockHoldings\[], activityStatistics{}}

Write temp -> replace; keep backup; recover on malformed/missing.

```



\## `docs/05\_TECH\_STACK.md`

```markdown

\# Tech stack (verified)

IDE: VS Community 2022. Project: Class Library (.NET Framework), net48, x64, C# 7.3.

Compile refs: ScriptHookVDotNet3 3.6.0 (nuget, net48); LemonUI.SHVDN3 2.2.0 (net48,

depends SHVDN3>=3.6.0). Key events: System.Windows.Forms.

Runtime: ScriptHookV (v3889.0/1158.13 supports Legacy 1.0.3889.0 / Enhanced 1.0.1158.13);

SHVDN nightly v3.7.0-nightly.206 (loader renamed ScriptHookVDotNet-Loader.asi);

LemonUI SHVDN3 release files.

IMPORTANT: compile-time ref != installed runtime. Stable 3.6.0 runtime breaks on game

>=1.0.3258.0; compile vs stable, run compatible nightly. Do not mix SHVDN forks.

Sources: nuget.org/packages/ScriptHookVDotNet3/3.6.0 ; nuget.org/packages/LemonUI.SHVDN3/2.2.0 ;

github.com/scripthookvdotnet/scripthookvdotnet ; dev-c.com/gtav/scripthookv.

```



\## `docs/06\_SETUP\_GUIDE.md`

```markdown

\# Setup (5-minute checklist)

1 VS2022 + .NET desktop dev + net48 pack + latest VC++ redist + Git.

2 New Class Library (.NET Framework) -> net48 -> x64 -> ClassicSandbox.Modules; delete Class1.cs.

3 NuGet: Install-Package ScriptHookVDotNet3 -Version 3.6.0 ;

&#x20; Install-Package LemonUI.SHVDN3 -Version 2.2.0 ; add System.Windows.Forms; CopyLocal=False.

4 Runtime tree in GTA V root (GTA5.exe): ScriptHookV.dll, dinput8.dll,

&#x20; ScriptHookVDotNet-Loader.asi, ScriptHookVDotNet.dll, ScriptHookVDotNet2.dll,

&#x20; ScriptHookVDotNet3.dll, ScriptHookVDotNet.ini; scripts/ holds your DLL + LemonUI.SHVDN3.dll

&#x20; + LemonUI SHVDN3 files. Update loader+DLLs together. No RPH/FiveM LemonUI.

5 ScriptHookVDotNet.ini: ReloadKeyBinding=Insert ; ConsoleKeyBinding=F4 ;

&#x20; ScriptsLocation="scripts" ; AutoLoadScripts=true. (Alt: F4 -> Reload()).

6 Post-build: if not defined GTAV\_DIR exit /b 0 ; mkdir scripts if missing ;

&#x20; copy /Y "$(TargetPath)" "%GTAV\_DIR%\\scripts\\$(TargetFileName)".

7 Story Mode only; use launcher's official single-player/BattlEye-off option; never Online.

Troubleshoot: script not loading -> check edition/build vs runtime; wrong LemonUI folder;

loader/DLL version mismatch; scripts path.

```



\## `docs/07\_TEST\_PLAN.md`

```markdown

\# Test plan

Build: B01 refs resolve; B02 x64 DLL produced; B03 no stray runtime copies; B04 post-build copies.

Smoke: R01 loads; R02 F10 opens; R03 F10 closes; R04 grant notif; R05 ready notif;

R06 wheel shows granted (slot rules); R07 select equips; R08 override on; R09 restore;

R10 Insert reload -> single instance.

Failure: F01 F10 during load/cutscene ignored; F02 death mid-load stops safely;

F03 reload mid-load releases asset; F04 double grant rejected; F05 save-load no stale handle;

F06 mission restriction recorded (no universal-bypass claim).

Perf: measure frame time before/after, menu closed, during load, after 10 reloads;

count loaded scripts. Publish no perf claim without named machine+build.

```



\## `docs/08\_RELEASE\_CHECKLIST.md`

```markdown

\# Release gate

\[ ] Compiles clean (code+line captured if not). \[ ] R01-R10 pass. \[ ] F01-F06 pass.

\[ ] Abort releases assets + restores tracked state. \[ ] No duplicate handlers after reload.

\[ ] No unverified native hashes. \[ ] No leaked/cross-title assets. \[ ] Offline-only.

\[ ] THIRD\_PARTY\_NOTICES present. \[ ] Known-limitations documented. \[ ] Evidence register updated.

```



\## `docs/09\_RISK\_REGISTER.md`

```markdown

\# Risks

|Risk|Sev|Mitigation|

|PortWatch treated as official PC|Critical|Separate official vs community evidence|

|Leaked console material|Critical|Prohibit|

|Invented GTA VI APIs|Critical|Adapter only after gates|

|Cross-title assets|High|Original mechanics only|

|Story/mission edits|High|Optional sandbox; avoid continuity|

|Online interference|Critical|Story Mode only|

|SHVDN/build mismatch|High|Record build; compatible runtime|

|Mixing SHVDN forks|High|One distribution|

|Nightly API drift|Medium|Compile vs stable 3.6.0|

|Entity bloat|High|Registry + cleanup tests|

|Save corruption|High|Version+backup+atomic|

|Duplicate handlers|High|Abort unsubscribe + reload test|

|Fake zone detection|High|No arbitrary radius as interior test|

|Wrong ped flag id|High|Verified flag 48; 208 is DisableExplosionReactions|

|Overpromising Tier-3|Medium|Research gates|

|False "tested" claims|Medium|Separate review/compile/runtime|

```



\## `docs/10\_POLICY\_AND\_SAFETY.md`

```markdown

\# Policy \& safety

Rockstar Mod Guidelines (2026-09-10): no expanding official storylines; no combining

IP/assets across titles; no mods for unreleased platforms; multiplayer only via licensed

Creator Platform; non-commercial unless authorized; no real-money gambling/lootboxes.

Offline alone does NOT guarantee compliance. Review before distribution.

Safety: Story Mode only; use official launcher single-player/BattlEye-off option;

never enter GTA Online with mods; ScriptHookV closes game on multiplayer attempt.

No leaked builds, no executable reverse-engineering, no redistributed assets.

Source: rockstargames.com/community-resources/mod-guidelines

```



\## `docs/11\_GTA\_VI\_MIGRATION\_GATE.md`

```markdown

\# GTA VI migration gate (all must pass; no date promised)

1 Official PC release. 2 Publicly documented modding interface. 3 License permits work.

4 Tools legally distributable. 5 Clean test install. 6 Verified native/API mappings.

Portable now: payout formulas, state transitions, ownership rules, income accumulators,

stock sim, save schema, menu view-models, activity defs, cooldowns/caps, tests.

NOT preparable now: GTA VI hashes/classes/offsets, Script Hook VI signatures, Leonida

coords from leaks, console-extracted assets, exe patches, compat-layer hooks.

Example portable contract:

public interface IEconomyService { int GetCash(); bool TrySpend(int amount); void Credit(int amount); }

```



\## `docs/12\_RESEARCH\_EVIDENCE.md`

```markdown

\# Evidence register (checked 2026-11-26)

Confirmed: GTA VI console launch 2026-11-19 (Rockstar Newswire "Now Available").

Not established: official GTA VI PC release; Script Hook VI.

Community-only: PortWatch progress % (unofficial ports; Take-Two takedown reported).

Verified tooling: SHVDN stable 3.6.0 / nightly 206; ScriptHookV v3889.0/1158.13;

LemonUI.SHVDN3 2.2.0; SHVDN ini defaults (ReloadKeyBinding=None, ConsoleKeyBinding=F4).

Verified APIs: GTA.Script(Tick/KeyDown/Aborted/Wait), Game.GameTime/IsLoading/IsCutsceneActive/

IsPaused, WeaponCollection.Give/Select, Weapon.Ammo/MaxAmmo, WeaponAsset.Request/IsLoaded/

MarkAsNoLongerNeeded, Ped.CanSwitchWeapons, LemonUI ObjectPool/NativeMenu/NativeItem.

Ped flags: 48=BlockWeaponSwitching (upstream enum; anchors 32/78); 208=DisableExplosionReactions

(FiveM) -> previous draft's 208 was WRONG.

Unverified: all screenshot "cut feature" claims; treat as hypotheses.

Record YOUR edition+build here: \_\_\_\_\_\_.

```



\## ADRs (`docs/adr/`)

```markdown

ADR-001 Phase-gated development: build in GTA V now; GTA VI only after gates. Consequence: no speculative VI code.

ADR-002 Stable API baseline: compile vs SHVDN3 3.6.0; run compatible nightly. Consequence: avoids nightly-only API drift.

ADR-003 Game adapter boundary: raw natives only in GtaVGameAdapter. Consequence: portable logic, swappable adapter.

ADR-004 Original content only: no cross-title assets/story edits. Consequence: policy-compliant by design.

ADR-005 No speculative VI native map: no guessed hashes. Consequence: migration deferred until verified.

```



\## `config/ClassicSandbox.example.ini` (consumed starting MVP‑B)

```ini

; Schema example for MVP-B services. Module 1 uses fixed F10 and no save file.

\[General]

Enabled=true

\[Property]

SafehousePrice=10000

IncomePerDay=250

OfflineCapDays=3

\[Taxi]

BaseFare=20

PerKm=15

\[Dispatch]

MaxActive=1

```



\## `tests/BUG\_REPORT\_TEMPLATE.md`

```markdown

Game edition+build: / SHVDN version: / LemonUI version: /

Steps: / Expected: / Actual: / Compiler error (code+line) if any: /

ScriptHookVDotNet.log excerpt: / Repro frequency:

```



\---



\# PART 3 — MODULE 1 (COMPLETE, AUDITED, FLAG‑CORRECTED)



`src/ClassicSandbox.Modules/WeaponAndZoneUnlocker.cs`



```csharp

using System;

using System.Collections.Generic;

using System.Windows.Forms;

using GTA;

using GTA.Native;

using GTA.UI;

using LemonUI;

using LemonUI.Menus;



namespace ClassicSandbox.Modules

{

&#x20;   public sealed class WeaponAndZoneUnlocker : Script

&#x20;   {

&#x20;       private sealed class ArsenalEntry

&#x20;       {

&#x20;           public readonly WeaponHash Hash;

&#x20;           public readonly string Name;

&#x20;           public ArsenalEntry(WeaponHash hash, string name) { Hash = hash; Name = name; }

&#x20;       }



&#x20;       private static readonly ArsenalEntry\[] Arsenal =

&#x20;       {

&#x20;           new ArsenalEntry(WeaponHash.Knife, "Knife"),

&#x20;           new ArsenalEntry(WeaponHash.Nightstick, "Nightstick"),

&#x20;           new ArsenalEntry(WeaponHash.Bat, "Baseball Bat"),

&#x20;           new ArsenalEntry(WeaponHash.Crowbar, "Crowbar"),

&#x20;           new ArsenalEntry(WeaponHash.KnuckleDuster, "Knuckle Duster"),

&#x20;           new ArsenalEntry(WeaponHash.Pistol, "Pistol"),

&#x20;           new ArsenalEntry(WeaponHash.CombatPistol, "Combat Pistol"),

&#x20;           new ArsenalEntry(WeaponHash.APPistol, "AP Pistol"),

&#x20;           new ArsenalEntry(WeaponHash.Pistol50, "Pistol .50"),

&#x20;           new ArsenalEntry(WeaponHash.SNSPistol, "SNS Pistol"),

&#x20;           new ArsenalEntry(WeaponHash.HeavyPistol, "Heavy Pistol"),

&#x20;           new ArsenalEntry(WeaponHash.VintagePistol, "Vintage Pistol"),

&#x20;           new ArsenalEntry(WeaponHash.Revolver, "Revolver"),

&#x20;           new ArsenalEntry(WeaponHash.MicroSMG, "Micro SMG"),

&#x20;           new ArsenalEntry(WeaponHash.SMG, "SMG"),

&#x20;           new ArsenalEntry(WeaponHash.AssaultSMG, "Assault SMG"),

&#x20;           new ArsenalEntry(WeaponHash.MachinePistol, "Machine Pistol"),

&#x20;           new ArsenalEntry(WeaponHash.CombatPDW, "Combat PDW"),

&#x20;           new ArsenalEntry(WeaponHash.AssaultRifle, "Assault Rifle"),

&#x20;           new ArsenalEntry(WeaponHash.CarbineRifle, "Carbine Rifle"),

&#x20;           new ArsenalEntry(WeaponHash.AdvancedRifle, "Advanced Rifle"),

&#x20;           new ArsenalEntry(WeaponHash.SpecialCarbine, "Special Carbine"),

&#x20;           new ArsenalEntry(WeaponHash.BullpupRifle, "Bullpup Rifle"),

&#x20;           new ArsenalEntry(WeaponHash.CompactRifle, "Compact Rifle"),

&#x20;           new ArsenalEntry(WeaponHash.MG, "MG"),

&#x20;           new ArsenalEntry(WeaponHash.CombatMG, "Combat MG"),

&#x20;           new ArsenalEntry(WeaponHash.Gusenberg, "Gusenberg Sweeper"),

&#x20;           new ArsenalEntry(WeaponHash.PumpShotgun, "Pump Shotgun"),

&#x20;           new ArsenalEntry(WeaponHash.SawnOffShotgun, "Sawed-Off Shotgun"),

&#x20;           new ArsenalEntry(WeaponHash.AssaultShotgun, "Assault Shotgun"),

&#x20;           new ArsenalEntry(WeaponHash.BullpupShotgun, "Bullpup Shotgun"),

&#x20;           new ArsenalEntry(WeaponHash.HeavyShotgun, "Heavy Shotgun"),

&#x20;           new ArsenalEntry(WeaponHash.DoubleBarrelShotgun, "Double Barrel Shotgun"),

&#x20;           new ArsenalEntry(WeaponHash.SniperRifle, "Sniper Rifle"),

&#x20;           new ArsenalEntry(WeaponHash.HeavySniper, "Heavy Sniper"),

&#x20;           new ArsenalEntry(WeaponHash.MarksmanRifle, "Marksman Rifle"),

&#x20;           new ArsenalEntry(WeaponHash.GrenadeLauncher, "Grenade Launcher"),

&#x20;           new ArsenalEntry(WeaponHash.RPG, "RPG"),

&#x20;           new ArsenalEntry(WeaponHash.Minigun, "Minigun"),

&#x20;           new ArsenalEntry(WeaponHash.HomingLauncher, "Homing Launcher"),

&#x20;           new ArsenalEntry(WeaponHash.Grenade, "Grenades"),

&#x20;           new ArsenalEntry(WeaponHash.StickyBomb, "Sticky Bombs"),

&#x20;           new ArsenalEntry(WeaponHash.ProximityMine, "Proximity Mines"),

&#x20;           new ArsenalEntry(WeaponHash.Molotov, "Molotovs"),

&#x20;           new ArsenalEntry(WeaponHash.SmokeGrenade, "Smoke Grenades"),

&#x20;           new ArsenalEntry(WeaponHash.BZGas, "BZ Gas"),

&#x20;           new ArsenalEntry(WeaponHash.PetrolCan, "Jerry Can"),

&#x20;           new ArsenalEntry(WeaponHash.FireExtinguisher, "Fire Extinguisher"),

&#x20;           new ArsenalEntry(WeaponHash.StunGun, "Stun Gun")

&#x20;       };



&#x20;       // Upstream PedConfigFlagToggles: 48 = BlockWeaponSwitching.

&#x20;       // Anchors verified in SHVDN source: 32 = WillFlyThroughWindscreen, 78 = IsAimingGun.

&#x20;       // NOTE: 208 is DisableExplosionReactions (FiveM), NOT weapon switching.

&#x20;       private const int BlockWeaponSwitchingFlag = 48;



&#x20;       private readonly ObjectPool pool = new ObjectPool();

&#x20;       private readonly NativeMenu menu = new NativeMenu("Classic Sandbox", "Pocket Arsenal");

&#x20;       private readonly Queue<ArsenalEntry> loadQueue = new Queue<ArsenalEntry>();

&#x20;       private readonly HashSet<WeaponHash> grantedWeapons = new HashSet<WeaponHash>();



&#x20;       private bool switchOverrideEnabled;

&#x20;       private bool grantInProgress;

&#x20;       private bool shuttingDown;

&#x20;       private int trackedPedHandle;

&#x20;       private bool originalCanSwitchWeapons;

&#x20;       private bool hasOriginalSwitchState;

&#x20;       private WeaponAsset pendingAsset;

&#x20;       private bool hasPendingAsset;

&#x20;       private ArsenalEntry pendingEntry;

&#x20;       private int assetRequestStartedAt;



&#x20;       public WeaponAndZoneUnlocker()

&#x20;       {

&#x20;           BuildMenu();

&#x20;           pool.Add(menu);

&#x20;           Tick += OnTick;

&#x20;           KeyDown += OnKeyDown;

&#x20;           Aborted += OnAborted;

&#x20;       }



&#x20;       private void BuildMenu()

&#x20;       {

&#x20;           NativeItem grantAll = new NativeItem("Grant Full Arsenal",

&#x20;               "Loads the curated weapons and fills their ammunition.");

&#x20;           grantAll.Activated += delegate { BeginGrantAll(); };



&#x20;           NativeItem enableOverride = new NativeItem("Enable Switch Override",

&#x20;               "Allows the player ped to switch weapons while enabled.");

&#x20;           enableOverride.Activated += delegate { SetSwitchOverride(true); };



&#x20;           NativeItem disableOverride = new NativeItem("Disable Switch Override",

&#x20;               "Restores the tracked ped's original switch permission.");

&#x20;           disableOverride.Activated += delegate { SetSwitchOverride(false); };



&#x20;           NativeItem close = new NativeItem("Close", "Close this menu. F10 opens it again.");

&#x20;           close.Activated += delegate { menu.Visible = false; };



&#x20;           menu.Add(grantAll);

&#x20;           menu.Add(enableOverride);

&#x20;           menu.Add(disableOverride);

&#x20;           menu.Add(close);



&#x20;           for (int i = 0; i < Arsenal.Length; i++)

&#x20;           {

&#x20;               ArsenalEntry entry = Arsenal\[i];

&#x20;               NativeItem item = new NativeItem(entry.Name,

&#x20;                   "Grant, fill ammunition, and equip this weapon.");

&#x20;               item.Tag = entry;

&#x20;               item.Activated += delegate (object sender, EventArgs args)

&#x20;               {

&#x20;                   NativeItem activatedItem = sender as NativeItem;

&#x20;                   if (activatedItem == null) return;

&#x20;                   ArsenalEntry selected = activatedItem.Tag as ArsenalEntry;

&#x20;                   if (selected != null) BeginSingleGrant(selected);

&#x20;               };

&#x20;               menu.Add(item);

&#x20;           }

&#x20;       }



&#x20;       private void OnKeyDown(object sender, KeyEventArgs e)

&#x20;       {

&#x20;           if (shuttingDown || e.KeyCode != Keys.F10) return;

&#x20;           if (e.Control || e.Alt || e.Shift) return;

&#x20;           if (Game.IsLoading || Game.IsCutsceneActive || Game.IsPaused) return;



&#x20;           if (menu.Visible) { menu.Visible = false; return; }



&#x20;           Ped ped = Game.Player.Character;

&#x20;           if (!IsPedUsable(ped))

&#x20;           {

&#x20;               Notification.Show("Pocket Arsenal is unavailable right now.");

&#x20;               return;

&#x20;           }

&#x20;           menu.Visible = true;

&#x20;           e.SuppressKeyPress = true;

&#x20;       }



&#x20;       private void OnTick(object sender, EventArgs e)

&#x20;       {

&#x20;           if (shuttingDown) return;

&#x20;           pool.Process();

&#x20;           if (Game.IsLoading || Game.IsCutsceneActive) return;



&#x20;           Ped ped = Game.Player.Character;

&#x20;           if (!IsPedUsable(ped))

&#x20;           {

&#x20;               if (menu.Visible) menu.Visible = false;

&#x20;               return;

&#x20;           }



&#x20;           if (switchOverrideEnabled)

&#x20;           {

&#x20;               EnsureTrackedPed(ped);

&#x20;               ped.CanSwitchWeapons = true;

&#x20;           }

&#x20;           ProcessAssetQueue(ped);

&#x20;       }



&#x20;       private void BeginGrantAll()

&#x20;       {

&#x20;           if (grantInProgress) { Notification.Show("An arsenal load is already running."); return; }

&#x20;           Ped ped = Game.Player.Character;

&#x20;           if (!IsPedUsable(ped)) { Notification.Show("The player is not ready."); return; }



&#x20;           loadQueue.Clear();

&#x20;           grantedWeapons.Clear();

&#x20;           for (int i = 0; i < Arsenal.Length; i++) loadQueue.Enqueue(Arsenal\[i]);

&#x20;           grantInProgress = true;

&#x20;           Notification.Show("Pocket Arsenal: loading " + loadQueue.Count + " weapons.");

&#x20;       }



&#x20;       private void BeginSingleGrant(ArsenalEntry entry)

&#x20;       {

&#x20;           if (grantInProgress) { Notification.Show("Wait for the current arsenal load to finish."); return; }

&#x20;           Ped ped = Game.Player.Character;

&#x20;           if (!IsPedUsable(ped)) { Notification.Show("The player is not ready."); return; }

&#x20;           loadQueue.Clear();

&#x20;           grantedWeapons.Clear();

&#x20;           loadQueue.Enqueue(entry);

&#x20;           grantInProgress = true;

&#x20;       }



&#x20;       private void ProcessAssetQueue(Ped ped)

&#x20;       {

&#x20;           if (!grantInProgress) return;



&#x20;           if (!hasPendingAsset)

&#x20;           {

&#x20;               if (loadQueue.Count == 0) { FinishGrantAll(); return; }

&#x20;               ArsenalEntry next = loadQueue.Dequeue();

&#x20;               WeaponAsset asset = new WeaponAsset(next.Hash);

&#x20;               if (!asset.IsValid) return;

&#x20;               pendingEntry = next;

&#x20;               pendingAsset = asset;

&#x20;               hasPendingAsset = true;

&#x20;               assetRequestStartedAt = Environment.TickCount;

&#x20;               pendingAsset.Request();

&#x20;               return;

&#x20;           }



&#x20;           if (pendingAsset.IsLoaded)

&#x20;           {

&#x20;               Weapon weapon = ped.Weapons.Give(pendingEntry.Hash, 0, false, false);

&#x20;               if (weapon != null \&\& weapon.IsPresent)

&#x20;               {

&#x20;                   int maximum = weapon.MaxAmmo;

&#x20;                   if (maximum > 0) weapon.Ammo = maximum;

&#x20;                   grantedWeapons.Add(pendingEntry.Hash);

&#x20;               }

&#x20;               ReleasePendingAsset();

&#x20;               return;

&#x20;           }



&#x20;           int elapsed = unchecked(Environment.TickCount - assetRequestStartedAt);

&#x20;           if (elapsed >= 3000)

&#x20;           {

&#x20;               Notification.Show("Could not load: " + pendingEntry.Name);

&#x20;               ReleasePendingAsset();

&#x20;               return;

&#x20;           }

&#x20;           pendingAsset.Request();

&#x20;       }



&#x20;       private void FinishGrantAll()

&#x20;       {

&#x20;           grantInProgress = false;

&#x20;           Ped ped = Game.Player.Character;

&#x20;           if (IsPedUsable(ped) \&\& grantedWeapons.Contains(WeaponHash.Pistol))

&#x20;               ped.Weapons.Select(WeaponHash.Pistol);

&#x20;           Notification.Show("Pocket Arsenal ready: " + grantedWeapons.Count + " weapons available.");

&#x20;           grantedWeapons.Clear();

&#x20;       }



&#x20;       private void SetSwitchOverride(bool enabled)

&#x20;       {

&#x20;           Ped ped = Game.Player.Character;

&#x20;           if (!IsPedUsable(ped)) { Notification.Show("The player is not ready."); return; }



&#x20;           if (enabled)

&#x20;           {

&#x20;               EnsureTrackedPed(ped);

&#x20;               switchOverrideEnabled = true;

&#x20;               ped.CanSwitchWeapons = true;

&#x20;               Notification.Show("Weapon-switch override enabled.");

&#x20;           }

&#x20;           else

&#x20;           {

&#x20;               RestoreTrackedPed();

&#x20;               switchOverrideEnabled = false;

&#x20;               hasOriginalSwitchState = false;

&#x20;               trackedPedHandle = 0;

&#x20;               Notification.Show("Weapon-switch override disabled.");

&#x20;           }

&#x20;       }



&#x20;       private void EnsureTrackedPed(Ped ped)

&#x20;       {

&#x20;           if (hasOriginalSwitchState \&\& trackedPedHandle == ped.Handle) return;

&#x20;           RestoreTrackedPed();

&#x20;           trackedPedHandle = ped.Handle;

&#x20;           bool blocked = Function.Call<bool>(Hash.GET\_PED\_CONFIG\_FLAG, ped.Handle,

&#x20;               BlockWeaponSwitchingFlag, false);

&#x20;           originalCanSwitchWeapons = !blocked;

&#x20;           hasOriginalSwitchState = true;

&#x20;       }



&#x20;       private void RestoreTrackedPed()

&#x20;       {

&#x20;           if (!hasOriginalSwitchState || trackedPedHandle == 0) return;

&#x20;           if (!Function.Call<bool>(Hash.DOES\_ENTITY\_EXIST, trackedPedHandle)) return;

&#x20;           Function.Call(Hash.SET\_PED\_CAN\_SWITCH\_WEAPON, trackedPedHandle, originalCanSwitchWeapons);

&#x20;       }



&#x20;       private static bool IsPedUsable(Ped ped)

&#x20;       {

&#x20;           return ped != null \&\& ped.Exists() \&\& !ped.IsDead \&\& ped.IsHuman;

&#x20;       }



&#x20;       private void ReleasePendingAsset()

&#x20;       {

&#x20;           if (hasPendingAsset) pendingAsset.MarkAsNoLongerNeeded();

&#x20;           hasPendingAsset = false;

&#x20;           pendingEntry = null;

&#x20;       }



&#x20;       private void OnAborted(object sender, EventArgs e)

&#x20;       {

&#x20;           if (shuttingDown) return;

&#x20;           shuttingDown = true;

&#x20;           Tick -= OnTick;

&#x20;           KeyDown -= OnKeyDown;

&#x20;           Aborted -= OnAborted;

&#x20;           menu.Visible = false;

&#x20;           pool.HideAll();

&#x20;           ReleasePendingAsset();

&#x20;           loadQueue.Clear();

&#x20;           grantedWeapons.Clear();

&#x20;           grantInProgress = false;

&#x20;           RestoreTrackedPed();

&#x20;           switchOverrideEnabled = false;

&#x20;           hasOriginalSwitchState = false;

&#x20;           trackedPedHandle = 0;

&#x20;       }

&#x20;   }

}

```



\---



\# PART 4 — RED TEAM AUDIT \& HONEST STATUS



\- \*\*Caught \& fixed:\*\* flag 208 misread as weapon switching; it is `DisableExplosionReactions` \[\[4]]. Now uses verified flag 48 (`BlockWeaponSwitching`) \[\[1]].

\- \*\*Earlier draft defects already removed:\*\* missing `System.Windows.Forms`, invented `ParentWindow`/`ButtonItem`, non‑existent `Game.GameTimeDelta`/`Game.IsContentUpdateAvailable`, `WeaponHash.BaseballBat`, fake zone radius, unprocessed menu.

\- \*\*Scope honesty:\*\* this is a switch‑override + curated arsenal, \*\*not\*\* a universal interior/mission firing bypass.

\- \*\*Verification status:\*\* source/API review done against stable sources; \*\*compiler and GTA V runtime NOT executed here\*\*; GTA VI compatibility unknown. This is a complete implementation \*candidate\*, not a certified "tested" release.



\# PART 5 — IMMEDIATE NEXT ACTIONS

1\. Create repo + docs above. 2. Record edition/build. 3. Create net48 x64 library + refs. 4. Install compatible runtime. 5. Add Module 1. 6. Build; capture any error (code+line+versions). 7. Run R01–R10/F01–F06. 8. Only then start MVP‑B (`PersistenceService`, `EconomyService`, property, taxi, dispatch).



\*\*Bottom line:\*\* the project is viable as a GTA V single‑player overhaul with portable logic; PortWatch is not an official PC modding platform; and Module 1 now uses only verified APIs with the ped‑flag bug corrected.

