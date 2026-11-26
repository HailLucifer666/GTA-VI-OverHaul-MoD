\# Classic GTA Overhaul — Master Plan, Project Setup, and Module 1



\*\*Council + Red Team protocol is active.\*\* Architectural decisions, code generation, and fixes will pass through the Architect, RAGE/SHVDN Engineer, and Adversarial Auditor.



\*\*Research status:\*\* I cannot perform live web/source verification in this response. Your 26-item list is therefore treated as an \*\*unverified product brief\*\*, not established fact. I also cannot independently verify the stated GTA VI release status. The project must not depend on leaked builds, prerelease assets, or claims sourced only from AI summaries. Every GTA VI feature should receive a source and verification status before it guides development.



\---



\# 1. Repository and Documentation Structure



Create this structure:



```text

ClassicGTA/

├── ClassicGTA.sln

├── README.md

├── docs/

│   ├── 01\_RESEARCH\_BASELINE.md

│   ├── 02\_PRD.md

│   ├── 03\_ARCHITECTURE.md

│   ├── 04\_MVP\_AND\_ROADMAP.md

│   ├── 05\_TEST\_PLAN.md

│   ├── 06\_GTA\_VI\_PORTING.md

│   ├── 07\_LEGAL\_AND\_SAFETY.md

│   └── 08\_DATA\_SCHEMA.md

├── src/

│   ├── ClassicGTA.Core/

│   │   ├── GameNatives.cs

│   │   ├── Configuration.cs

│   │   ├── ModLogger.cs

│   │   ├── OwnedEntitySet.cs

│   │   └── Persistence.cs

│   └── ClassicGTA.WeaponAndZoneUnlocker/

│       ├── WeaponAndZoneUnlocker.cs

│       └── GameNatives.cs

└── tests/

&#x20;   └── MANUAL\_TEST\_CHECKLIST.md

```



The first module is deliberately standalone. Once two or three modules work, shared functionality should move into `ClassicGTA.Core`; do not build a large framework before validating the toolchain.



\---



\# 2. Five-Minute Visual Studio Setup



\## 2.1 Install the runtime tools



1\. Install \*\*Visual Studio Community\*\* with:

&#x20;  - \*\*.NET desktop development\*\*

&#x20;  - .NET Framework 4.8 targeting pack

2\. Install the PC Story Mode versions of:

&#x20;  - ScriptHookV

&#x20;  - ScriptHookVDotNet3

&#x20;  - LemonUI for SHVDN3

3\. Confirm that the SHVDN `scripts` directory exists under your GTA V installation.

4\. Test SHVDN with an unmodified, known-working installation before adding this project.



Use offline Story Mode only. Do not load the mod into GTA Online.



\## 2.2 Create the project



1\. \*\*Create a new project → Class Library (.NET Framework)\*\*.

2\. Select \*\*C#\*\*, not the .NET 6/8 class-library template.

3\. Name: `ClassicGTA.WeaponAndZoneUnlocker`.

4\. Framework: \*\*.NET Framework 4.8\*\*.

5\. Open project properties:

&#x20;  - Assembly name: `ClassicGTA.WeaponAndZoneUnlocker`

&#x20;  - Default namespace: `ClassicGTA.WeaponAndZoneUnlocker`

&#x20;  - Platform target: `x64`

&#x20;  - Prefer 32-bit: disabled



\## 2.3 Add references



Prefer referencing the DLL versions actually installed with your game:



\- `ScriptHookVDotNet3.dll`

\- `System.Windows.Forms`



For later UI modules:



\- `LemonUI.SHVDN3.dll`



For each manually referenced third-party DLL:



```text

Reference Properties → Copy Local → False

```



This avoids accidentally shipping duplicate runtime assemblies. NuGet packages are acceptable when their versions match the installed SHVDN/LemonUI runtime.



Add these two source files:



\- `GameNatives.cs`

\- `WeaponAndZoneUnlocker.cs`



Delete the generated `Class1.cs`.



\## 2.4 Post-build deployment



Set \*\*Build Events → Post-build event command line\*\*, replacing the path with your installation:



```bat

if not exist "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Grand Theft Auto V\\scripts\\" mkdir "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Grand Theft Auto V\\scripts\\"

copy /Y "$(TargetPath)" "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Grand Theft Auto V\\scripts\\"

```



Build in \*\*Release x64\*\*, then press \*\*Insert\*\* in Story Mode to reload SHVDN scripts.



\---



\# 3. Council Review — Module 1



\### Architect



\- Native calls are isolated in `GameNatives.cs`.

\- No universal “safe-zone” flag is assumed to exist in GTA V.

\- The module re-enables common weapon controls during ordinary gameplay and permits weapon switching.

\- Story missions can still remove weapons or apply stronger scripted restrictions.

\- F10 toggles the Pocket Arsenal feature. Enabling grants weapons; disabling does \*\*not\*\* confiscate them.



\### RAGE Engineer



\- Uses SHVDN3 `Script`, `Tick`, `KeyDown`, `Game.Player.Character`, and `GTA.Native.Function.Call`.

\- Input overrides run every tick because throttling them to 250 ms would make controls unreliable.

\- Weapon enumeration runs only when F10 enables the feature.

\- The script refuses to operate when a network session is active.



\### Red Team findings



\- No peds, vehicles, blips, props, or cameras are created, so there is no entity leak.

\- `IS\_WEAPON\_VALID` filters enum entries unsupported by the installed game build.

\- Duplicate enum values are suppressed with `HashSet<uint>`.

\- No player weapon is forcibly equipped every frame.

\- The module does not claim to defeat every mission-specific weapon restriction.

\- No game assets, leaked content, reverse-engineering, or online functionality are involved.



\---



\# 4. `GameNatives.cs`



```csharp

using System;

using System.Collections.Generic;

using GTA;

using GTA.Native;



namespace ClassicGTA.WeaponAndZoneUnlocker

{

&#x20;   internal static class GameNatives

&#x20;   {

&#x20;       private static readonly int\[] WeaponControls =

&#x20;       {

&#x20;           24,  // INPUT\_ATTACK

&#x20;           25,  // INPUT\_AIM

&#x20;           37,  // INPUT\_SELECT\_WEAPON

&#x20;           45,  // INPUT\_RELOAD

&#x20;           68,  // INPUT\_VEH\_AIM

&#x20;           69,  // INPUT\_VEH\_ATTACK

&#x20;           70,  // INPUT\_VEH\_ATTACK2

&#x20;           140, // INPUT\_MELEE\_ATTACK\_LIGHT

&#x20;           141, // INPUT\_MELEE\_ATTACK\_HEAVY

&#x20;           142, // INPUT\_MELEE\_ATTACK\_ALTERNATE

&#x20;           143, // INPUT\_MELEE\_BLOCK

&#x20;           257, // INPUT\_ATTACK2

&#x20;           263, // INPUT\_MELEE\_ATTACK1

&#x20;           264  // INPUT\_MELEE\_ATTACK2

&#x20;       };



&#x20;       internal static bool IsOfflineSession()

&#x20;       {

&#x20;           return !Function.Call<bool>(Hash.NETWORK\_IS\_SESSION\_STARTED);

&#x20;       }



&#x20;       internal static bool IsNormalGameplayInputContext()

&#x20;       {

&#x20;           if (!IsOfflineSession())

&#x20;           {

&#x20;               return false;

&#x20;           }



&#x20;           if (Function.Call<bool>(Hash.IS\_PAUSE\_MENU\_ACTIVE))

&#x20;           {

&#x20;               return false;

&#x20;           }



&#x20;           if (Function.Call<bool>(Hash.IS\_CUTSCENE\_PLAYING))

&#x20;           {

&#x20;               return false;

&#x20;           }



&#x20;           if (Function.Call<bool>(Hash.IS\_PLAYER\_SWITCH\_IN\_PROGRESS))

&#x20;           {

&#x20;               return false;

&#x20;           }



&#x20;           if (Function.Call<bool>(Hash.IS\_SCREEN\_FADED\_OUT))

&#x20;           {

&#x20;               return false;

&#x20;           }



&#x20;           return Function.Call<bool>(Hash.IS\_PLAYER\_CONTROL\_ON);

&#x20;       }



&#x20;       internal static void EnableWeaponInput(Ped player)

&#x20;       {

&#x20;           Function.Call(

&#x20;               Hash.SET\_PED\_CAN\_SWITCH\_WEAPON,

&#x20;               player.Handle,

&#x20;               true);



&#x20;           for (int index = 0; index < WeaponControls.Length; index++)

&#x20;           {

&#x20;               Function.Call(

&#x20;                   Hash.ENABLE\_CONTROL\_ACTION,

&#x20;                   0,

&#x20;                   WeaponControls\[index],

&#x20;                   true);

&#x20;           }

&#x20;       }



&#x20;       internal static int GrantAllValidWeapons(Ped player)

&#x20;       {

&#x20;           HashSet<uint> processedWeapons = new HashSet<uint>();

&#x20;           int grantedCount = 0;



&#x20;           Array values = Enum.GetValues(typeof(WeaponHash));



&#x20;           foreach (WeaponHash weapon in values)

&#x20;           {

&#x20;               if (weapon == WeaponHash.Unarmed ||

&#x20;                   weapon == WeaponHash.Parachute)

&#x20;               {

&#x20;                   continue;

&#x20;               }



&#x20;               uint weaponValue = Convert.ToUInt32(weapon);



&#x20;               if (!processedWeapons.Add(weaponValue))

&#x20;               {

&#x20;                   continue;

&#x20;               }



&#x20;               if (!Function.Call<bool>(

&#x20;                   Hash.IS\_WEAPON\_VALID,

&#x20;                   weaponValue))

&#x20;               {

&#x20;                   continue;

&#x20;               }



&#x20;               Function.Call(

&#x20;                   Hash.GIVE\_WEAPON\_TO\_PED,

&#x20;                   player.Handle,

&#x20;                   weaponValue,

&#x20;                   9999,

&#x20;                   false,

&#x20;                   false);



&#x20;               OutputArgument maximumAmmo = new OutputArgument();



&#x20;               Function.Call(

&#x20;                   Hash.GET\_MAX\_AMMO,

&#x20;                   player.Handle,

&#x20;                   weaponValue,

&#x20;                   maximumAmmo);



&#x20;               int ammo = maximumAmmo.GetResult<int>();



&#x20;               if (ammo > 0)

&#x20;               {

&#x20;                   Function.Call(

&#x20;                       Hash.SET\_PED\_AMMO,

&#x20;                       player.Handle,

&#x20;                       weaponValue,

&#x20;                       ammo);

&#x20;               }



&#x20;               grantedCount++;

&#x20;           }



&#x20;           return grantedCount;

&#x20;       }

&#x20;   }

}

```



\---



\# 5. `WeaponAndZoneUnlocker.cs`



```csharp

using System;

using System.Windows.Forms;

using GTA;

using GTA.UI;



namespace ClassicGTA.WeaponAndZoneUnlocker

{

&#x20;   public sealed class WeaponAndZoneUnlocker : Script

&#x20;   {

&#x20;       private bool pocketArsenalEnabled;



&#x20;       public WeaponAndZoneUnlocker()

&#x20;       {

&#x20;           // Control overrides must run every frame.

&#x20;           // The body contains no searching, spawning, or entity iteration.

&#x20;           Interval = 0;



&#x20;           Tick += OnTick;

&#x20;           KeyDown += OnKeyDown;

&#x20;           Aborted += OnAborted;

&#x20;       }



&#x20;       private void OnTick(object sender, EventArgs e)

&#x20;       {

&#x20;           if (!GameNatives.IsNormalGameplayInputContext())

&#x20;           {

&#x20;               return;

&#x20;           }



&#x20;           Ped player = Game.Player.Character;



&#x20;           if (player == null ||

&#x20;               !player.Exists() ||

&#x20;               !player.IsAlive)

&#x20;           {

&#x20;               return;

&#x20;           }



&#x20;           GameNatives.EnableWeaponInput(player);

&#x20;       }



&#x20;       private void OnKeyDown(object sender, KeyEventArgs e)

&#x20;       {

&#x20;           if (e.KeyCode != Keys.F10 ||

&#x20;               !GameNatives.IsOfflineSession())

&#x20;           {

&#x20;               return;

&#x20;           }



&#x20;           pocketArsenalEnabled = !pocketArsenalEnabled;



&#x20;           if (!pocketArsenalEnabled)

&#x20;           {

&#x20;               Notification.Show(

&#x20;                   "Classic GTA: Pocket Arsenal disabled. " +

&#x20;                   "Previously granted weapons were retained.");



&#x20;               return;

&#x20;           }



&#x20;           Ped player = Game.Player.Character;



&#x20;           if (player == null ||

&#x20;               !player.Exists() ||

&#x20;               !player.IsAlive)

&#x20;           {

&#x20;               Notification.Show(

&#x20;                   "Classic GTA: Pocket Arsenal is enabled. " +

&#x20;                   "Press F10 while controlling a living character to grant weapons.");



&#x20;               return;

&#x20;           }



&#x20;           int weaponCount =

&#x20;               GameNatives.GrantAllValidWeapons(player);



&#x20;           Notification.Show(

&#x20;               "Classic GTA: Pocket Arsenal enabled. " +

&#x20;               weaponCount + " valid weapons processed.");

&#x20;       }



&#x20;       private void OnAborted(object sender, EventArgs e)

&#x20;       {

&#x20;           Tick -= OnTick;

&#x20;           KeyDown -= OnKeyDown;

&#x20;           Aborted -= OnAborted;



&#x20;           // This module owns no entities and requires no world cleanup.

&#x20;       }

&#x20;   }

}

```



\## Module 1 acceptance test



1\. Load Story Mode and verify that the game reaches normal gameplay.

2\. Press F10:

&#x20;  - One notification appears.

&#x20;  - Valid weapons supported by the current installation are granted.

3\. Press F10 again:

&#x20;  - The feature reports disabled.

&#x20;  - Existing weapons remain, intentionally.

4\. Test switching weapons and firing in ordinary interior gameplay.

5\. Test during a cutscene and pause menu:

&#x20;  - The module must not override general player control.

6\. Test death, character switching, and SHVDN reload.

7\. Test a story mission:

&#x20;  - Record any scripted weapon lock separately rather than treating it as a universal safe-zone bug.



\---



\# 6. Product Requirements Document



\## Product vision



A modular, offline, single-player sandbox expansion that recreates selected classic GTA mechanics through legitimate PC scripting APIs. GTA V is the prototyping host; GTA VI support is a later, separately validated port.



\## Primary user



A PC Story Mode player who wants:



\- Player-owned property and vehicles

\- Repeatable side jobs

\- Classic weapon inventory freedom

\- Persistent progression

\- Optional systems that can be enabled independently



\## Product principles



1\. \*\*Story Mode only\*\*

2\. \*\*No leaked content or proprietary asset redistribution\*\*

3\. \*\*No assumed GTA VI compatibility\*\*

4\. \*\*Every world entity has an owner and cleanup path\*\*

5\. \*\*Modules can fail without corrupting the entire installation\*\*

6\. \*\*Simulation is distinguished from restoration\*\*

7\. \*\*Vanilla behavior remains available through configuration\*\*



\## Functional requirements



| ID | Requirement | MVP acceptance criterion |

|---|---|---|

| FR-01 | Independent modules | Disabling one module does not break the others |

| FR-02 | Pocket Arsenal | F10 grants all valid current-build weapons offline |

| FR-03 | Weapon input overrides | Common controls restored in normal gameplay without forcing cutscene control |

| FR-04 | Emergency dispatch | Player can request one service; arrival and timeout states resolve |

| FR-05 | Taxi job | Pickup, destination, success, cancellation, and payout work |

| FR-06 | Persistence | Owned content survives script reload and game restart |

| FR-07 | Property purchase | Purchase is atomic; insufficient funds cannot create ownership |

| FR-08 | Business income | Income is calculated once per elapsed in-game day |

| FR-09 | Vehicle dealership | Purchased vehicle spawns at a valid, unoccupied location |

| FR-10 | Safe lifecycle | Death, reload, and cancellation do not leave runaway tasks or blips |

| FR-11 | VI portability | Gameplay modules do not contain raw native hashes |

| FR-12 | Diagnostics | Errors identify module, operation, and recoverable action |



\## Non-functional requirements



\- No heavy world scans every frame.

\- Default ambient scans: 250–1000 ms, depending on purpose.

\- No unbounded entity collections.

\- No dependence on hard-coded absolute Windows paths in source.

\- Atomic persistence: write temporary file, then replace.

\- Configuration files versioned from the first persistent release.

\- Clear distinction between:

&#x20; - `Prototype`

&#x20; - `Tested`

&#x20; - `Released`

&#x20; - `Ported`



\## Non-goals



\- GTA Online support

\- Multiplayer synchronization

\- Bypassing anti-cheat or platform protections

\- Importing assets from older GTA games

\- Reproducing copyrighted missions, films, or audio

\- Developing against leaked console material

\- Promising prone, skateboarding, or dual wielding before animation feasibility is proven



\---



\# 7. Architecture



\## Module contract



After the standalone prototype, introduce a shared contract:



```text

IModModule

├── Id

├── DisplayName

├── Start()

├── Stop()

├── OnTick(elapsedMilliseconds)

├── OnKeyDown(key)

└── Dispose()

```



The host owns configuration, logging, throttling, and shutdown. Modules own their gameplay state and registered entities.



\## Native abstraction layers



```text

Gameplay module

&#x20;   ↓

ClassicGTA service interface

&#x20;   ↓

GameNatives.cs / WorldServices.cs

&#x20;   ↓

SHVDN3 or future supported VI scripting API

```



Keep these concepts behind interfaces:



\- Player and money

\- Weapons

\- Vehicles and driving tasks

\- Peds and combat tasks

\- Blips and GPS

\- Notifications

\- Game clock

\- Model loading

\- Persistence identifiers



Coordinates and model names belong in data files, not gameplay logic.



\## Entity ownership



Track separately:



1\. \*\*Mod-owned:\*\* safe to delete on cancellation.

2\. \*\*Borrowed:\*\* player taxi, borrowed police vehicle; never delete.

3\. \*\*Detached:\*\* call `MarkAsNoLongerNeeded()` when the module no longer manages it.



Every spawned entity must have:



\- Registration

\- State-machine owner

\- Timeout

\- Cancellation behavior

\- Reload cleanup

\- Blip cleanup



\## Persistence



Initial root file:



```ini

\[Meta]

SchemaVersion=1



\[Modules]

WeaponUnlocker=true

EmergencyDispatch=true

Taxi=true

Properties=false



\[Player]

LastProcessedGameDay=0

```



Property records should use stable generated IDs rather than array positions:



```ini

\[Property.safehouse\_001]

Type=Safehouse

ContentId=vinewood\_safehouse\_01

PurchasePrice=100000

Owned=true

```



Business income must be based on the saved in-game day, not accumulated per-frame income. Reject malformed prices, duplicate IDs, and implausible timestamps without resetting the entire file.



\---



\# 8. Step-by-Step Roadmap



\## Phase 0 — Verify the brief



For each of the 26 claimed cuts, record:



\- Claim

\- Official source URL and publication date

\- Whether an official release build confirms it

\- GTA V prototyping equivalent

\- Whether the result would be restoration or simulation

\- Confidence: `Verified / Partially verified / Unverified / Disproved`



No external article should be cited merely because it appears in the supplied prompt.



\## Phase 1 — Foundation



1\. Validate Module 1.

2\. Add logging and INI configuration.

3\. Add module enable/disable settings.

4\. Add entity ownership and cleanup utilities.

5\. Add atomic persistence.

6\. Run 30-minute reload/death/mission stability tests.



\*\*Gate:\*\* no unbounded entity growth and no persistent-script failures after repeated reloads.



\## Phase 2 — MVP



Release three small vertical slices:



1\. Weapon and input module

2\. Emergency dispatch

3\. Taxi side job



Emergency dispatch states:



```text

Idle → Dispatched → Driving → Arrived → Service → Complete

&#x20;                        └→ TimedOut/Cancelled

```



Taxi states:



```text

Idle → Searching → FareAssigned → Pickup

&#x20;    → EnRoute → Complete → Payout → Idle

```



Cancellation must handle:



\- Leaving the taxi

\- Vehicle destruction

\- Passenger death

\- Player death

\- Excessive distance

\- Script reload



\*\*MVP release gate:\*\* all states terminate cleanly; payouts occur exactly once.



\## Phase 3 — Persistent sandbox



1\. Safehouse markers

2\. Vehicle dealerships

3\. Business ownership and daily income

4\. Purchased-vehicle storage

5\. Save migration tests

6\. Boat/aircraft spawn validation



Submarines should use a supported GTA V model and valid water spawn. Do not infer GTA VI submarine mechanics from this prototype.



\## Phase 4 — Repeatable activities



Implement in this order:



1\. Vigilante

2\. Rampage

3\. Camera and stat toggles

4\. Simulated stock market



The stock market is explicitly a \*\*local simulation\*\*, not a restoration of verified GTA VI exchanges. Assassination hooks must award or schedule changes once, not every tick.



\## Phase 5 — Advanced feasibility spikes



Time-box each investigation separately:



\- Prone crawling

\- Dual wielding

\- Skateboarding

\- Companion AI

\- Guard dog

\- Blackjack

\- Bowling/darts/tennis



A spike ends in either a minimal demonstration or a documented rejection. Do not ship animation fakery as a finished mechanic.



\## Phase 6 — GTA VI port



Begin only when all are true:



\- Official PC release is available

\- A legitimate public scripting toolchain exists

\- Its Story Mode use is permitted

\- Relevant APIs are documented or safely testable

\- No leaked material is required



Then:



1\. Reconfirm every target feature in the released game.

2\. Implement `GameNatives.VI`.

3\. Replace model IDs, coordinates, controls, and camera behavior.

4\. Port one module at a time.

5\. Run the same acceptance suite.

6\. Keep GTA V and GTA VI adapters separate.



\---



\# 9. Feature Backlog by Delivery Class



| Feature | Delivery classification |

|---|---|

| Pocket Arsenal | Direct prototype |

| Emergency dispatch | Custom AI/state machine |

| Taxi | Custom state machine |

| Safehouses/businesses | Simulated ownership and world services |

| Vehicle dealerships | Persistence plus spawning |

| Stock market | Local simulation |

| Vigilante/rampage | Mission generators |

| First person/stat bars | Host-dependent API work |

| Cinemas/web browser | Do not copy assets; optional original content only |

| Casino/card games | Original UI and logic |

| Prone/dual wield/skateboard | Animation feasibility required |

| Friend hangouts | Companion scheduling and AI |

| Pets | Companion AI plus persistence |

| Mission replay | Do not promise; mission-script compatibility is a major risk |

| Home decoration | Placement persistence plus collision/budget limits |



\---



\# 10. Test and Release Gates



Every module must pass:



\- Startup with an empty configuration

\- Startup with malformed configuration

\- Repeated SHVDN reload

\- Player death during every state

\- Character switching

\- Vehicle/ped destruction

\- Mission start and cancellation

\- Pause/cutscene behavior

\- Insufficient-funds purchase

\- Double activation and duplicate payout attempts

\- 30-minute soak test

\- Entity/blip count returns to baseline after cancellation

\- No operation in a network session



Release only with:



\- Source code

\- Dependency versions

\- Installation and removal instructions

\- Known limitations

\- Save-backup instructions

\- No bundled game assets or third-party DLLs unless their licenses permit redistribution



\---



\# 11. Legal and Safety Policy



\- Offline Story Mode only.

\- Do not distribute modified executables, game archives, leaked builds, or extracted assets.

\- Use original UI, icons, text, and audio.

\- Check the license before redistributing SHVDN or LemonUI binaries.

\- Treat the native database as technical reference material, not proof that a feature is safe or compatible.

\- Keep Rockstar policy and game updates under review; policy can change.

\- Label early builds clearly and recommend a separate Story Mode save.



This plan delivers a compiling first prototype now while preventing the largest later failures: unverified GTA VI assumptions, tightly coupled natives, entity leaks, duplicate payouts, and advanced-animation commitments without feasibility evidence.











\# Classic GTA Overhaul  

\## Complete Project Plan and Documentation Pack — v0.1



\*\*Council + Red Team protocol:\*\* Active.



This document pack is ready to copy into your repository. It covers the project charter, research controls, PRD, MVP, technology stack, architecture, persistence, module specifications, roadmap, testing, releases, legal safeguards, and the eventual GTA VI port.



> \*\*Important baseline:\*\* Your list of GTA VI changes is treated as an \*\*unverified product brief\*\*, not established fact. No live source research is claimed here. GTA V is the prototyping host. GTA VI support cannot be promised until an official PC release, a legitimate scripting toolchain, and the relevant game behavior are available for testing.



\---



\# 1. Master Execution Plan



Follow these phases in order. Do not start advanced mechanics before the persistence, cleanup, and testing foundations are stable.



| Phase | Deliverable | Exit gate |

|---|---|---|

| 0. Research | Verified feature inventory and provenance register | Every claimed GTA VI cut has a status and source |

| 1. Toolchain | Compiling, deployable, hot-reloadable GTA V mod | Empty project loads repeatedly without errors |

| 2. Foundation | Configuration, logging, native adapter, entity tracking, persistence | Foundation tests pass |

| 3. Module 1 | Pocket Arsenal and weapon-input overrides | F10 and control tests pass |

| 4. Module 2 | Emergency Services Dispatch | All arrival, timeout, cancellation, and cleanup paths pass |

| 5. Module 3 | Classic Taxi side job | Exactly-once payouts and full cancellation tests pass |

| 6. MVP release | Packaged v0.1.0 with three modules | Soak test, reload test, and clean-install test pass |

| 7. Persistent sandbox | Safehouses, businesses, dealerships, storage | Ownership survives reload and restart without duplication |

| 8. Activities | Vigilante, Rampage, stats, camera, stock simulation | Each activity resolves without runaway entities |

| 9. Advanced spikes | Prone, dual wielding, skateboarding, pets, companions, mini-games | Each is demonstrated, revised, or explicitly rejected |

| 10. Stabilization | Performance, compatibility, balancing, documentation | Release-candidate checklist passes |

| 11. GTA VI research | Official PC/API capability assessment | Porting is legally and technically feasible |

| 12. GTA VI port | Separate adapter, content data, and test baseline | Modules are ported and tested individually |



\*\*Recommended delivery rule:\*\* one playable vertical slice at a time. Do not generate all modules in parallel.



\---



\# 2. Repository Structure



Use one deployable mod assembly initially. This avoids unnecessary SHVDN dependency-loading problems while retaining clean internal boundaries.



```text

ClassicGTA/

├── ClassicGTA.sln

├── README.md

├── LICENSE

├── NOTICE.md

├── CHANGELOG.md

├── CONTRIBUTING.md

├── .gitignore

│

├── docs/

│   ├── 00\_PROJECT\_CHARTER.md

│   ├── 01\_RESEARCH\_BASELINE.md

│   ├── 02\_PRD.md

│   ├── 03\_MVP.md

│   ├── 04\_TECH\_STACK.md

│   ├── 05\_ARCHITECTURE.md

│   ├── 06\_DATA\_SCHEMA.md

│   ├── 07\_MODULE\_SPECIFICATIONS.md

│   ├── 08\_ROADMAP.md

│   ├── 09\_TEST\_PLAN.md

│   ├── 10\_GTA\_VI\_PORTING.md

│   ├── 11\_LEGAL\_AND\_SAFETY.md

│   ├── 12\_RELEASE\_PROCESS.md

│   └── 13\_REQUIREMENTS\_TRACEABILITY.md

│

├── src/

│   ├── ClassicGTA/

│   │   ├── ClassicGTA.csproj

│   │   ├── ClassicGTAScript.cs

│   │   │

│   │   ├── Core/

│   │   │   ├── IModModule.cs

│   │   │   ├── ModuleManager.cs

│   │   │   ├── ModuleContext.cs

│   │   │   ├── ModuleState.cs

│   │   │   ├── GameClock.cs

│   │   │   └── ThrottledAction.cs

│   │   │

│   │   ├── Platform/

│   │   │   ├── GameNatives.cs

│   │   │   ├── PlayerService.cs

│   │   │   ├── VehicleService.cs

│   │   │   ├── PedService.cs

│   │   │   ├── BlipService.cs

│   │   │   ├── TaskService.cs

│   │   │   ├── CameraService.cs

│   │   │   └── NotificationService.cs

│   │   │

│   │   ├── Infrastructure/

│   │   │   ├── Configuration.cs

│   │   │   ├── ModLogger.cs

│   │   │   ├── Persistence.cs

│   │   │   ├── SaveMigration.cs

│   │   │   ├── OwnedEntitySet.cs

│   │   │   └── KeybindManager.cs

│   │   │

│   │   ├── Modules/

│   │   │   ├── Arsenal/

│   │   │   ├── EmergencyDispatch/

│   │   │   ├── Taxi/

│   │   │   ├── Properties/

│   │   │   ├── Dealerships/

│   │   │   ├── Businesses/

│   │   │   ├── Vigilante/

│   │   │   ├── Rampage/

│   │   │   ├── Progression/

│   │   │   ├── StockMarket/

│   │   │   └── Advanced/

│   │   │

│   │   ├── Content/

│   │   │   ├── Properties.ini

│   │   │   ├── Dealerships.ini

│   │   │   ├── Taxi.ini

│   │   │   └── Balance.ini

│   │   │

│   │   └── UI/

│   │       ├── MainMenu.cs

│   │       ├── MenuFactory.cs

│   │       └── HudFormatter.cs

│   │

│   └── ClassicGTA.Tests/

│       ├── ClassicGTA.Tests.csproj

│       ├── TaxiPricingTests.cs

│       ├── BusinessIncomeTests.cs

│       ├── SaveValidationTests.cs

│       ├── StateMachineTests.cs

│       └── StockSimulationTests.cs

│

├── tests/

│   ├── MANUAL\_TEST\_CHECKLIST.md

│   ├── SOAK\_TEST.md

│   └── COMPATIBILITY\_MATRIX.md

│

└── dist/

&#x20;   └── README.txt

```



The `dist` directory should contain release documentation locally, not necessarily be committed with generated binaries.



\---



\# 3. `00\_PROJECT\_CHARTER.md`



\## Project name



\*\*Classic GTA Overhaul\*\*



\## Vision



Create a modular, offline, single-player sandbox expansion that recreates selected classic GTA mechanics through legitimate PC scripting APIs.



\## Prototyping host



\- Grand Theft Auto V on PC

\- Story Mode only

\- ScriptHookV

\- ScriptHookVDotNet3

\- LemonUI for SHVDN3

\- .NET Framework 4.8, x64



\## Long-term objective



Port suitable gameplay systems to GTA VI PC if and when:



1\. An official PC version exists.

2\. A legitimate public scripting toolchain exists.

3\. Its APIs support the required mechanics.

4\. Development does not depend on leaked material or executable modification.



\## Product principles



1\. Offline Story Mode only.

2\. No leaked builds, extracted assets, or executable reverse-engineering.

3\. No native hashes inside gameplay state machines.

4\. Every entity, blip, and task has an owner and cleanup path.

5\. Every persistent record has validation and a schema version.

6\. Every module can be disabled independently.

7\. Custom simulations are not advertised as native game restoration.

8\. Preserve vanilla behavior whenever a module is disabled.

9\. Prefer simple, tested systems over large unfinished frameworks.

10\. Never promise GTA VI compatibility based only on similar native names.



\## Success criteria



The project is successful when a user can install one package, enable only the desired modules, play extended Story Mode sessions, reload scripts, and retain valid persistent progress without runaway entity growth or corrupted saves.



\---



\# 4. `01\_RESEARCH\_BASELINE.md`



\## Evidence policy



The supplied 26-item feature list is a research queue. Do not put “confirmed GTA VI feature cut” in release notes until supported by appropriate evidence.



\## Evidence levels



| Level | Meaning |

|---|---|

| E0 | Unsourced claim or AI-generated summary |

| E1 | Community report, video, or secondary article |

| E2 | Named interview, official promotional material, or multiple credible reports |

| E3 | Verified in an official release build |

| E4 | Verified on the exact PC build and scripting toolchain used by the mod |



\## Research register



Create one record per claim:



```text

Research ID:

Feature:

Original claim:

Source URL:

Source publisher:

Publication date:

Direct quotation or observed behavior:

Evidence level:

Verified game version:

Verified platform:

GTA V prototyping equivalent:

Restoration or simulation:

Porting implication:

Research status:

Researcher notes:

```



Valid research statuses:



```text

Unverified

PartiallyVerified

VerifiedConsole

VerifiedPC

Disproved

NotTestable

```



\## Rules



\- Record publication and access dates.

\- Distinguish pre-release interviews from shipping behavior.

\- Do not use AI summaries as primary sources.

\- Do not use leaked footage or assets as development dependencies.

\- Recheck claims after major game updates.

\- Do not assume that a removed storefront, menu, or activity implies a particular internal implementation.

\- The eventual PC port must use E4 verification for supported functionality.



\---



\# 5. `02\_PRD.md` — Product Requirements Document



\## 5.1 Target users



\### Primary user



A PC Story Mode player who wants repeatable sandbox activities, persistent ownership, and fewer inventory restrictions.



\### Secondary user



A beginner mod developer who needs readable source, isolated modules, clear configuration, and predictable installation.



\## 5.2 User needs



\- Install and remove the mod without modifying game executables.

\- Enable modules independently.

\- Understand whether a feature is native, scripted, or simulated.

\- Purchase content without accidental duplicate charges.

\- Cancel activities safely.

\- Retain ownership across game restarts.

\- Recover from malformed configuration without losing all progress.

\- Play without network communication or account integration.



\## 5.3 Scope



\### Initial scope



\- Pocket Arsenal

\- Weapon-input overrides

\- Emergency dispatch

\- Taxi missions

\- Safehouse ownership

\- Business income

\- Vehicle dealerships

\- Purchased-vehicle storage

\- Vigilante

\- Rampage

\- Simulated progression

\- Simulated stock market



\### Conditional scope



\- First-person toggles

\- Companion hangouts

\- Guard animals

\- Furniture placement

\- Local information-browser UI

\- Original blackjack implementation

\- Other mini-games

\- Advanced animation mechanics



\### Out of scope



\- GTA Online or multiplayer

\- Anti-cheat bypasses

\- Modified game executables

\- Leaked content

\- Redistribution of game assets

\- Recreation of copyrighted films, missions, or audio

\- Guaranteed story-mission replay

\- Cross-save synchronization

\- Cloud accounts, telemetry, ads, or paid services



\## 5.4 Functional requirements



| ID | Requirement |

|---|---|

| FR-001 | The mod shall refuse normal operation when a network session is detected. |

| FR-002 | Each module shall have an independent enabled setting. |

| FR-003 | All direct native calls shall be isolated in platform services or `GameNatives`. |

| FR-004 | F10 shall toggle Pocket Arsenal and display the result. |

| FR-005 | Enabling Pocket Arsenal shall grant valid supported weapons without forcing an equip. |

| FR-006 | Disabling Pocket Arsenal shall not confiscate previously granted weapons. |

| FR-007 | Weapon-input overrides shall not force control during cutscenes or player switching. |

| FR-008 | The player shall be able to request police, ambulance, and fire services. |

| FR-009 | A dispatch shall terminate through completion, timeout, cancellation, or unrecoverable failure. |

| FR-010 | The taxi module shall support pickup, transport, completion, and cancellation. |

| FR-011 | A fare shall pay out at most once. |

| FR-012 | Leaving or destroying the taxi shall cancel or resolve the active fare. |

| FR-013 | Purchases shall validate funds before changing ownership. |

| FR-014 | Money deduction and ownership creation shall behave as one logical transaction. |

| FR-015 | Ownership shall survive script reload and game restart. |

| FR-016 | Business income shall be processed once per eligible in-game day. |

| FR-017 | Purchased vehicles shall spawn only at validated spawn locations. |

| FR-018 | Mod-owned entities shall be cleaned up or detached according to explicit ownership rules. |

| FR-019 | Borrowed player vehicles shall never be deleted by activity cleanup. |

| FR-020 | Invalid persistent values shall be rejected without crashing the host script. |

| FR-021 | The stock market shall be labeled as a local simulation. |

| FR-022 | Advanced mechanics shall not be advertised until their feasibility spike passes. |

| FR-023 | Errors shall identify the module and failed operation. |

| FR-024 | Module failure shall not leave an unbounded per-frame retry loop. |



\## 5.5 Non-functional requirements



| ID | Requirement |

|---|---|

| NFR-001 | No heavy world scan shall run every frame by default. |

| NFR-002 | Ambient searches should use configurable intervals, normally 250–1000 ms. |

| NFR-003 | Input and camera operations may run per frame only when required. |

| NFR-004 | Entity collections shall have documented maximum sizes. |

| NFR-005 | Persistent writes shall be atomic where practical. |

| NFR-006 | Save data shall include a schema version. |

| NFR-007 | No absolute user-specific paths shall be hard-coded in source. |

| NFR-008 | The mod shall make no network requests. |

| NFR-009 | Logs shall be bounded or rotated. |

| NFR-010 | Public methods receiving configuration data shall validate ranges and identifiers. |

| NFR-011 | Game-dependent content shall be externalized where practical. |

| NFR-012 | A module’s normal disabled state shall have negligible runtime cost. |



\## 5.6 Definition of Done



A feature is done only when:



\- Its code compiles in Release x64.

\- Configuration is documented.

\- Normal operation is tested.

\- Cancellation is tested.

\- Player death is tested.

\- Script reload is tested.

\- Cleanup is verified.

\- Persistence is tested where applicable.

\- Duplicate rewards are tested where applicable.

\- Known limitations are documented.

\- No online behavior exists.

\- The changelog and traceability matrix are updated.



\---



\# 6. `03\_MVP.md`



\## MVP objective



Prove that the project can deliver reliable gameplay, not merely menus and demonstrations.



\## Included modules



\### A. Arsenal and weapon controls



\- F10 toggle

\- Valid-weapon filtering

\- Maximum supported ammunition request

\- No forced weapon selection

\- Input restoration only during eligible normal gameplay

\- Online-session guard

\- No spawned entities



\### B. Emergency Services Dispatch



\- LemonUI menu opened with F9 by default

\- Police, ambulance, and fire options

\- One active dispatch per service

\- Configurable spawn distances and timeouts

\- Arrival detection

\- Service-specific behavior

\- Cancellation and cleanup

\- No repeated charges or rewards



\### C. Classic Taxi



\- Activation while driving an eligible taxi

\- One active fare

\- Sidewalk passenger selection or controlled passenger spawning

\- Pickup and destination blips

\- Route display

\- Distance-based payout

\- Fare cancellation

\- Borrowed-taxi protection

\- Entity and blip cleanup



\## Explicitly excluded from MVP



\- Property purchases

\- Businesses

\- Persistent vehicles

\- Vigilante

\- Rampage

\- Stock simulation

\- Advanced animations

\- Mini-games

\- Companion AI

\- Any GTA VI code



\## Default MVP controls



| Control | Action |

|---|---|

| F10 | Toggle Pocket Arsenal |

| F9 | Open emergency dispatch menu |

| F6 | Cancel current Classic GTA activity, where applicable |



All bindings must be configurable.



\## MVP acceptance scenarios



\### Arsenal



1\. Load Story Mode with default configuration.

2\. Press F10 once and confirm one enable notification.

3\. Confirm valid weapons are granted.

4\. Confirm no weapon is automatically forced into the player’s hands.

5\. Press F10 again and confirm weapons are retained.

6\. Test death, character switching, pause, cutscenes, and reload.



\### Dispatch



1\. Request each service independently.

2\. Verify that repeated activation cannot create unlimited vehicles.

3\. Allow a service to arrive.

4\. Force a timeout using an obstructed or unreachable scenario.

5\. Kill the player during dispatch.

6\. Destroy the service vehicle.

7\. Reload scripts during dispatch.

8\. Verify that owned blips and entities are cleaned up or intentionally detached.



\### Taxi



1\. Enter an eligible taxi and activate the module.

2\. Receive exactly one fare.

3\. Pick up the passenger.

4\. Drive to the destination.

5\. Verify exactly one payout.

6\. Repeat while exiting early.

7\. Repeat with passenger death.

8\. Repeat with taxi destruction.

9\. Repeat with player death.

10\. Reload during pickup and during transport.



\## MVP release gate



Release `v0.1.0` only when:



\- Three complete clean installations succeed.

\- Thirty consecutive script reloads produce no unhandled mod failure.

\- A 30-minute mixed-activity soak test passes.

\- Entity and blip counts return to baseline after cancellation.

\- No payout or charge duplication is found.

\- No network-session operation occurs.



\---



\# 7. `04\_TECH\_STACK.md`



\## Required stack



| Layer | Selection | Reason |

|---|---|---|

| Language | C# 7.3-compatible syntax | Safe baseline for .NET Framework 4.8 |

| Runtime | .NET Framework 4.8 | SHVDN3 compatibility |

| Build | Visual Studio Community, Release x64 | Free and supported |

| Game hook | ScriptHookV | Required Story Mode script host |

| .NET API | ScriptHookVDotNet3 | GTA scripting API |

| UI | LemonUI.SHVDN3 | Native-style menus |

| Input | SHVDN key events; `System.Windows.Forms.Keys` | Simple configurable keybinds |

| Configuration | INI | Human-readable and easy to recover |

| Persistence | Versioned INI initially | Suitable for small ownership records |

| Logging | Local UTF-8 text file | No external service |

| Version control | Git | Free rollback and release tagging |

| Unit tests | MSTest | Included in Visual Studio ecosystem |

| Distribution | ZIP with source release | Simple installation and auditability |



\## Dependency policy



\- Pin and record tested dependency versions.

\- Do not silently upgrade ScriptHookVDotNet3 or LemonUI.

\- Reference the same major runtime version used in the game installation.

\- Avoid unnecessary NuGet packages.

\- Check licenses before redistributing third-party DLLs.

\- Prefer installation instructions over bundling dependencies when redistribution terms are unclear.



\## Performance policy



| Work type | Frequency |

|---|---|

| Essential input controls | Per frame |

| Camera updates | Per frame only while active |

| Active mission proximity checks | Approximately 100–250 ms |

| Nearby passenger searches | Approximately 500–1000 ms |

| Business-day checks | Approximately 1000 ms or on clock changes |

| Persistence | Events only, not every frame |

| Diagnostic entity counts | On demand or very infrequently |



\## Coding policy



\- Fail safely and visibly.

\- Do not swallow exceptions without logging.

\- Avoid broad catches around the entire application without a recovery strategy.

\- No static mutable entity collections without centralized ownership.

\- No raw native hashes in module folders.

\- No gameplay decisions based solely on display names.

\- No blocking waits inside UI callbacks.

\- No unbounded `while` loops in script constructors.

\- No recursive save migration without a maximum supported version.



\---



\# 8. `05\_ARCHITECTURE.md`



\## 8.1 Architectural decision record



\### Architect



Use a thin host, independent modules, platform services, and pure gameplay logic. Avoid splitting every module into a separate DLL during the MVP.



\### RAGE engineer



Keep native interaction at the edge. GTA entities are volatile: handles can become invalid after death, reload, mission transitions, or streaming. Every long-running operation must revalidate its dependencies.



\### Red Team



A native wrapper alone does not guarantee portability. AI tasks, weapon behavior, camera modes, money systems, and world coordinates may differ in GTA VI. Porting therefore requires behavioral adapters and new content data, not only hash replacement.



\## 8.2 Runtime flow



```text

SHVDN host

&#x20;   ↓

ClassicGTAScript

&#x20;   ↓

ModuleManager

&#x20;   ├── Configuration

&#x20;   ├── Logging

&#x20;   ├── Persistence

&#x20;   ├── Entity ownership

&#x20;   ├── Keybind routing

&#x20;   └── Platform services

&#x20;           ↓

&#x20;       Enabled modules

&#x20;           ↓

&#x20;       Gameplay state machines

```



\## 8.3 Module lifecycle



Each module follows:



```text

Constructed → Initialized → Running → Suspended → Running

&#x20;                                 ↘ Stopping → Disposed

```



Required behavior:



\- `Initialize`: read validated settings and register no unnecessary world entities.

\- `Start`: enable input and gameplay processing.

\- `Suspend`: stop generating work without losing valid persistent data.

\- `Stop`: cancel activities and clean up owned resources.

\- `Dispose`: unsubscribe events and release references.

\- Exceptions transition the module into a failed state rather than repeatedly throwing every tick.



\## 8.4 Entity ownership classes



\### Mod-owned



Created solely for an activity. The module may delete them when safe.



Examples:



\- Spawned dispatch vehicle

\- Spawned driver

\- Generated taxi passenger

\- Mission blips



\### Borrowed



Existing world or player entities used temporarily. Never delete them.



Examples:



\- Player’s taxi

\- Player character

\- An existing police vehicle used by the player

\- An ambient pedestrian selected instead of spawned



\### Detached



No longer managed. Call the appropriate no-longer-needed operation and remove local references where supported.



\## 8.5 State-machine rules



\- Every state has an entry action.

\- Every state has explicit transition conditions.

\- Every activity has timeout or failure handling.

\- Terminal states are idempotent.

\- `Complete`, `Cancel`, and `Cleanup` can be called more than once without duplicating effects.

\- Rewards and purchases use transaction flags before world cleanup.



\## 8.6 Suggested intervals



```text

Host Tick: 0 ms

Module scheduler: elapsed-time based

Slow modules: own interval or scheduled next-run time

UI: event-driven where possible

```



Avoid giving every module an independent per-frame heavy callback.



\## 8.7 Error recovery



\- Disable only the failed module when isolation is reliable.

\- Log module ID, state, operation, and exception.

\- Avoid saving partially validated data.

\- Do not automatically repeat failed purchases or payouts.

\- Provide a “reset active activities” configuration option that does not erase ownership.



\---



\# 9. `06\_DATA\_SCHEMA.md`



\## File locations



Use the mod’s scripts folder or another documented writable local folder:



```text

scripts/

├── ClassicGTA.dll

├── ClassicGTA.ini

├── ClassicGTA.save.ini

├── ClassicGTA.log

└── ClassicGTA/

&#x20;   ├── Properties.ini

&#x20;   ├── Dealerships.ini

&#x20;   ├── Taxi.ini

&#x20;   └── Balance.ini

```



\## Main configuration example



```ini

\[Meta]

ConfigVersion=1



\[General]

EnableMod=true

OfflineOnly=true

ActivityCancelKey=F6

DispatchMenuKey=F9

ArsenalToggleKey=F10

NotificationDurationMs=2500



\[Arsenal]

Enabled=true

GrantMaximumAmmo=true



\[WeaponControls]

Enabled=true



\[EmergencyDispatch]

Enabled=true

MaximumActiveDispatches=2

SpawnDistanceMin=90.0

SpawnDistanceMax=140.0

ArrivalDistance=12.0

DispatchTimeoutSeconds=120



\[Taxi]

Enabled=true

PassengerSearchRadius=90.0

PassengerSpawnDistanceMin=45.0

PassengerSpawnDistanceMax=85.0

PickupRadius=8.0

DestinationRadius=10.0

BaseFare=8

PerMeterRateNumerator=3

PerMeterRateDenominator=100

MinimumFare=10

MaximumFare=2000



\[Persistence]

SaveFileName=ClassicGTA.save.ini

BackupCount=3

MaximumBusinessDaysPerCatchUp=7

```



These are initial tuning values, not claims about ideal game balance.



\## Save example



```ini

\[Meta]

SchemaVersion=1

SaveRevision=42



\[Progress]

LastProcessedGameDay=183

ArsenalEnabled=false



\[Property.safehouse\_001]

ContentId=vinewood\_safehouse\_01

Owned=true

PurchasePrice=100000

PurchasedGameDay=91



\[Property.business\_001]

ContentId=downtown\_business\_01

Owned=true

PurchasePrice=250000

PurchasedGameDay=102

DailyIncome=1250

LastIncomeGameDay=183



\[Vehicle.vehicle\_001]

ContentId=personal\_stinger\_01

ModelName=stinger

VehicleClass=Sports

PrimaryColor=12

SecondaryColor=12

Stored=true

SpawnPointId=vinewood\_garage\_01



\[Stats.Shooting]

Value=25

Experience=0



\[StockMarket]

Seed=194727

LastSimulationGameDay=183

Cash=0

```



\## Persistence rules



\- The game remains authoritative for current player cash unless a module explicitly implements a separate wallet.

\- Never write player cash on every tick.

\- Save after completed purchases, completed fares, and processed business days.

\- Use stable string IDs, not collection indices.

\- Validate prices, model names, colors, and spawn IDs.

\- Reject duplicate record IDs.

\- Write a temporary file first.

\- Preserve the previous valid save as a backup.

\- Never interpret save-file content as executable code.

\- Migrations must be sequential and tested.



\## Transaction order



\### Purchase



1\. Validate content and ownership.

2\. Validate funds.

3\. Revalidate that the player state is safe.

4\. Deduct money.

5\. Create ownership record.

6\. Attempt atomic persistence.

7\. If persistence fails, restore money and mark the purchase incomplete.

8\. Only then spawn optional presentation effects.



\### Activity payout



1\. Confirm activity is active and not already settled.

2\. Mark it settled in memory.

3\. Calculate payout from validated values.

4\. Award money.

5\. Persist statistics if applicable.

6\. Clean up the activity.



\---



\# 10. `07\_MODULE\_SPECIFICATIONS.md`



\## 10.1 Arsenal and Weapon Controls



\*\*Purpose:\*\* Inventory freedom and restoration of common weapon controls.



\*\*States:\*\*



```text

Disabled

Enabled

```



\*\*Rules:\*\*



\- Grant only valid weapons for the installed build.

\- Suppress duplicate enum values.

\- Never force-equip a weapon every frame.

\- Do not claim to defeat mission-specific scripting.

\- Do not run normal overrides during cutscenes or player switching.



\## 10.2 Emergency Services Dispatch



\*\*States:\*\*



```text

Idle

→ Requested

→ Spawning

→ Driving

→ Arrived

→ ProvidingService

→ Completed



Any active state

→ TimedOut | Cancelled | Failed

→ CleaningUp

→ Idle

```



\*\*Service behavior:\*\*



\- Police: arrival and basic presence first; advanced crime response is a later feature.

\- Ambulance: arrival followed by a bounded healing interaction.

\- Fire: arrival followed by detection and extinguishing of supported nearby fires.



\*\*Limits:\*\*



\- Maximum active dispatches.

\- Maximum owned vehicles and peds.

\- Spawn failure retry count.

\- Driving timeout.

\- Arrival timeout.

\- Service timeout.



\*\*Red-team concerns:\*\*



\- AI drivers may not reach the player.

\- The player may move far away.

\- Emergency vehicles can become stuck or destroyed.

\- Healing and fire behavior must not repeat indefinitely.



\## 10.3 Classic Taxi



\*\*States:\*\*



```text

Idle

→ WaitingForTaxi

→ Searching

→ FareAssigned

→ DrivingToPickup

→ WaitingForPassenger

→ EnRoute

→ Completing

→ Paid

→ Cooldown

→ Searching



Active fare

→ Cancelled | Failed

→ CleaningUp

→ WaitingForTaxi

```



\*\*Fare calculation:\*\*



```text

Base fare

\+ maximum(0, route distance − included distance) × distance rate

\+ optional time component

```



Clamp to configured minimum and maximum values. Use integer money calculations.



\*\*Cancellation:\*\*



\- Player exits beyond grace period

\- Taxi destroyed or irrecoverably disabled

\- Passenger dies or disappears

\- Player dies or is arrested

\- Destination becomes invalid

\- Script reload

\- Manual cancellation



\*\*Important:\*\* A passenger entering the vehicle is not sufficient proof of fare completion. Settlement occurs only after valid destination arrival.



\## 10.4 Properties



\*\*States:\*\*



```text

Unowned → Purchasable → Owned

Owned → Entering/UsingStorage → Owned

```



\*\*MVP of this module:\*\*



\- World marker

\- Purchase menu

\- Ownership persistence

\- Spawn point or storage access

\- No custom furniture



Do not imply that purchasing a marker creates a new interior if none exists.



\## 10.5 Businesses



\- Purchase business records.

\- Pay once per elapsed in-game day.

\- Process catch-up only to a configured cap.

\- Do not award income based on real-world offline time.

\- Avoid autosaving the game automatically.

\- Prevent negative or overflowing income values.



\## 10.6 Dealerships and Storage



\- Boats require valid water spawn points.

\- Aircraft require clear areas large enough for the model.

\- Submarines require an appropriate supported model and water volume.

\- Spawn descriptors, not arbitrary live entity handles, are persisted.

\- Store limited visual customization data initially.

\- Do not promise full damage persistence.

\- Charge before persistence completes only through the transaction procedure.



\## 10.7 Vigilante



```text

Idle

→ RequiresPoliceVehicle

→ GeneratingTarget

→ Pursuit

→ TargetResolved

→ NextWave | Completed

→ Cleanup

```



Resolve targets through documented conditions. Avoid undefined “mission success” detection.



\## 10.8 Rampage



```text

Idle

→ Countdown

→ Active

→ Success | Failed | Cancelled

→ Cleanup

```



Track only eligible kills. Prevent previously spawned targets from contributing after cancellation.



\## 10.9 Progression and Stats



\- Simulated stats: Stamina, Driving, Flying, Shooting.

\- Store values in the mod save.

\- Define exactly which actions award experience.

\- Apply only effects supported by the host API.

\- Clamp values and use diminishing returns.

\- Do not describe these as native GTA VI stats.



\## 10.10 Stock Market Simulator



\- Explicitly local and fictional.

\- Deterministic seeded simulation.

\- Saved cash, positions, price history, and last simulation day.

\- Fixed tick interval, normally in-game hours or days.

\- No real financial data or network access.

\- Assassination effects are scheduled once using event IDs.

\- Prevent selling more shares than owned.

\- Use decimal arithmetic for prices and integer arithmetic for shares.



\## 10.11 Advanced mechanics



Each requires a separate feasibility report:



```text

Required animation assets:

Required native/API support:

Multiplayer risk: must remain none

Player collision implications:

Camera implications:

Save requirements:

Minimum demonstration:

Performance budget:

Failure behavior:

Decision: Demonstrated / Revised / Rejected

```



Do not use the main branch for unstable physics experiments.



\---



\# 11. Feature Traceability for the Supplied 26 Items



| Brief item | Project treatment |

|---|---|

| 1. Houses and safehouses | Properties module; scripted ownership and access |

| 2. Passive businesses | Businesses module; persistent in-game-day income |

| 3. Boats, planes, helicopters | Dealerships and storage |

| 4. Submarines | Supported host-model prototype; VI support conditional |

| 5. Stock exchanges | Fictional local simulation, not native exchange restoration |

| 6. Web browser | Optional local information UI; no live web browser required |

| 7. Furniture | Advanced placement/persistence spike |

| 8. Taxi | MVP module |

| 9. Emergency side jobs | Dispatch first; Vigilante later; ambulance/fire jobs separately |

| 10. Rampages | Activity module after MVP |

| 11. Tennis | Advanced mini-game spike |

| 12. Darts and bowling | Advanced mini-game spikes |

| 13. Cinemas | Low priority; original content only |

| 14. Amusement rides | Advanced camera/attachment feasibility spike |

| 15. Casino/card games | Original blackjack UI and rules first |

| 16. Skateboarding | High-risk animation/physics spike |

| 17. Mission replay and medals | Out of initial scope; story-script compatibility risk |

| 18. First-person mode | Host API feasibility task |

| 19. Pocket Arsenal | MVP module |

| 20. RPG skill bars | Simulated progression module |

| 21. Prone crawling | High-risk animation spike |

| 22. Dual wielding | High-risk animation/combat spike |

| 23. Adult services | Not an initial requirement; no copied assets or explicit content dependency |

| 24. Friend hangouts | Advanced companion scheduling and AI |

| 25. Emergency calling | MVP dispatch menu; phone integration optional later |

| 26. Pets | Advanced companion AI and persistence |



\---



\# 12. `08\_ROADMAP.md`



\## Milestone M0 — Research and repository



1\. Create the Git repository.

2\. Add all documentation.

3\. Record the exact game and dependency versions.

4\. Enter all research claims with `Unverified` status.

5\. Establish the offline-only rule.

6\. Establish semantic versioning and changelog format.



\*\*Exit:\*\* clean repository, reproducible setup instructions.



\## Milestone M1 — Empty host and deployment



1\. Create the .NET Framework 4.8 class library.

2\. Configure x64.

3\. Add SHVDN3 and LemonUI references.

4\. Add post-build deployment.

5\. Load an empty host.

6\. Reload it repeatedly.

7\. Verify log creation.



\*\*Exit:\*\* no startup or reload errors.



\## Milestone M2 — Foundation



1\. Implement validated configuration.

2\. Implement bounded logging.

3\. Implement native/platform wrappers.

4\. Implement module lifecycle.

5\. Implement entity ownership.

6\. Implement atomic persistence.

7\. Implement keybind routing.

8\. Add pure-logic unit tests.



\*\*Exit:\*\* invalid configuration, save failure, and reload tests pass.



\## Milestone M3 — Arsenal



Use the previously provided standalone module first if desired, then integrate it into the host.



\*\*Exit:\*\* all Arsenal MVP scenarios pass.



\## Milestone M4 — Dispatch



Implement police first, then ambulance, then fire. Do not build all three before proving dispatch state management.



\*\*Exit:\*\* all service outcomes terminate and clean up correctly.



\## Milestone M5 — Taxi



Implement a controlled test fare before general pedestrian selection. This isolates state-machine bugs from ambient-world selection bugs.



\*\*Exit:\*\* payout and cancellation matrix passes.



\## Milestone M6 — MVP release



1\. Freeze features.

2\. Run soak testing.

3\. Test on a clean scripts folder.

4\. Write installation and removal instructions.

5\. Package source and release notes.

6\. Tag `v0.1.0`.



\## Milestone M7 — Persistent ownership



Order:



1\. One safehouse

2\. Multiple safehouses

3\. One business

4\. Multiple businesses

5\. Land vehicle dealership

6\. Vehicle storage

7\. Boats

8\. Aircraft

9\. Submarines



\## Milestone M8 — Activities



Order:



1\. Vigilante single target

2\. Vigilante waves

3\. Rampage single challenge

4\. Rampage content definitions

5\. Stats

6\. Camera support

7\. Stock simulation



\## Milestone M9 — Advanced spikes



Time-box each independently. No spike blocks maintenance of stable modules.



\## Milestone M10 — GTA VI assessment and port



Begin only after the porting gates in the next section.



\---



\# 13. `09\_TEST\_PLAN.md`



\## Test levels



\### Unit tests



Test without loading GTA V:



\- Fare calculations

\- State transition guards

\- Income calculations

\- Stock transactions

\- Save validation

\- Save migration

\- Keybind parsing

\- Entity registration limits

\- Duplicate settlement prevention



\### Integration tests



Require GTA V Story Mode:



\- Spawning and model loading

\- AI driving

\- Ped entering/exiting vehicles

\- Blips and GPS

\- Money operations

\- Camera behavior

\- Mission interference

\- Reload cleanup



\## Universal manual test matrix



Run each applicable test in every module state:



| Test | Expected result |

|---|---|

| Fresh configuration | Safe defaults generated |

| Malformed configuration | Defaults or validation error; no crash |

| Missing save | New valid save created |

| Malformed save | Backup retained; controlled recovery |

| Player death | Activity cancels or suspends safely |

| Player arrest | Activity cancels safely |

| Character switch | Handles revalidated |

| Vehicle destruction | Dependent state exits |

| Ped disappearance | Timeout or controlled failure |

| Cutscene | No forced controls |

| Pause menu | No accidental activation |

| Story mission | No persistent runaway behavior |

| Script reload | Resources cleaned up |

| Repeated hotkey | No duplicate entities or transactions |

| Network session | Mod refuses normal operation |

| 30-minute soak | No unbounded growth |



\## Performance test



Record:



\- Average and worst observable frame behavior

\- Active mod-owned vehicles

\- Active mod-owned peds

\- Active blips

\- Active module states

\- Log growth

\- Save-file growth



Compare against:



1\. Mod disabled

2\. Mod enabled but idle

3\. One active activity

4\. Maximum supported activity load



\## Save compatibility test



For each released schema:



1\. Create a representative save.

2\. Upgrade to the new build.

3\. Load it.

4\. Verify every record.

5\. Save again.

6\. Confirm the backup remains usable.

7\. Confirm no money, property, or shares are duplicated.



\## Release-blocking defects



\- Save corruption

\- Money duplication

\- Online functionality

\- Unbounded entity generation

\- Repeated per-frame exceptions

\- Failure to clean up on reload

\- Crash during normal activation

\- Undeletable purchased records

\- Arbitrary code execution through configuration or save files



\---



\# 14. `10\_GTA\_VI\_PORTING.md`



\## Porting gates



Porting begins only when:



\- Official PC game is installed legitimately.

\- Scripting is publicly available and suitable for offline use.

\- Dependency licenses and game policies permit the intended distribution.

\- Required APIs can be tested safely.

\- The target behavior is reconfirmed in the shipping PC build.



\## Porting sequence



1\. Freeze the GTA V release used as the behavioral baseline.

2\. Re-run the research register against GTA VI PC.

3\. Create a separate solution configuration and adapter namespace.

4\. Implement logging and host lifecycle.

5\. Implement offline-session detection.

6\. Implement player and notification services.

7\. Port Arsenal.

8\. Port entity and model services.

9\. Port Dispatch.

10\. Port Taxi.

11\. Port persistence with content-ID mapping.

12\. Port properties and dealerships using new coordinate data.

13\. Reassess camera, stats, animations, and advanced mechanics independently.



\## Content that must be replaced or revalidated



\- Native names and hashes

\- Model names

\- Weapon identifiers

\- Vehicle classes

\- Ped types

\- Coordinates

\- Interior identifiers

\- Camera modes

\- Control IDs

\- Money APIs

\- AI task behavior

\- Game-clock APIs

\- Blip and route behavior

\- Notification APIs



\## Forbidden assumption



> “The same native hash exists, therefore the feature works identically.”



Even equivalent native names can have different preconditions, state ownership, timing, or mission restrictions.



\## Save compatibility



Do not automatically interpret GTA V property coordinates or model names as valid GTA VI content. Use platform-qualified content IDs and an explicit migration map.



\---



\# 15. `11\_LEGAL\_AND\_SAFETY.md`



\## Development rules



\- Story Mode only.

\- Do not test in GTA Online.

\- Do not include anti-cheat bypass instructions.

\- Do not use leaked console builds or assets.

\- Do not reverse-engineer executables for this project.

\- Do not distribute modified game executables or archives.

\- Do not copy missions, films, music, voice acting, or other assets from older games.

\- Use original code, UI, text, and audio.

\- Verify third-party redistribution licenses.

\- Keep a `NOTICE.md` listing dependencies and their respective projects.

\- Reassess policy before each public release.



\## User-safety rules



\- Recommend backing up Story Mode saves.

\- State that game updates can break the mod.

\- Provide removal instructions.

\- Do not autosave over the user’s game without permission.

\- Do not include telemetry or network activation.

\- Label experimental builds clearly.



\## Recommended original-code license



MIT is a simple option for your original source, but selecting a license is your decision. Third-party dependencies and game assets are not covered by your license.



\---



\# 16. `12\_RELEASE\_PROCESS.md`



\## Versioning



Use semantic versioning:



```text

MAJOR: incompatible save or architecture changes

MINOR: backward-compatible modules and features

PATCH: fixes and balancing

```



Examples:



```text

v0.1.0 — MVP

v0.2.0 — Properties and businesses

v0.3.0 — Dealerships and storage

v0.4.0 — Vigilante and Rampage

v1.0.0 — Stable GTA V release

```



A GTA VI port should use a clearly separate package and compatibility label.



\## Release checklist



\- \[ ] Release x64 build succeeds.

\- \[ ] Dependency versions documented.

\- \[ ] No debug-only test content enabled.

\- \[ ] Unit tests pass.

\- \[ ] Manual module tests pass.

\- \[ ] Reload and soak tests pass.

\- \[ ] Save migration tests pass.

\- \[ ] Offline guard tested.

\- \[ ] Default configuration included.

\- \[ ] Installation instructions included.

\- \[ ] Removal instructions included.

\- \[ ] Known limitations included.

\- \[ ] Source and changelog published.

\- \[ ] No prohibited assets or dependencies bundled.



\## Support policy



Each bug report should include:



\- Mod version

\- Game version

\- SHVDN3 version

\- LemonUI version

\- Other installed script mods

\- Active module

\- Activity state

\- Reproduction steps

\- Relevant log excerpt

\- Save or configuration sample with sensitive paths removed



\---



\# 17. `README.md` — Copy-Ready Summary



```text

Classic GTA Overhaul is an offline Story Mode scripting project that prototypes

classic sandbox mechanics in GTA V PC.



Current status: Foundation / early development.

GTA VI support: not available and not guaranteed.



Requirements:

\- GTA V PC Story Mode

\- ScriptHookV

\- ScriptHookVDotNet3

\- LemonUI.SHVDN3 for menu modules

\- .NET Framework 4.8 runtime



Installation:

1\. Back up your Story Mode saves.

2\. Install the required dependencies according to their own instructions.

3\. Copy ClassicGTA.dll and the supplied configuration/content files into scripts.

4\. Start Story Mode offline.

5\. Review ClassicGTA.log if the mod does not initialize.



Default controls:

F10 — Pocket Arsenal

F9  — Emergency Dispatch

F6  — Cancel current activity



Removal:

1\. Exit the game.

2\. Remove ClassicGTA.dll from scripts.

3\. Optionally retain ClassicGTA.ini and ClassicGTA.save.ini as backups.



Limitations:

\- Story Mode only.

\- Scripted simulations are not native game restoration.

\- Story missions can override controls, entities, and player state.

\- Game or dependency updates may require a new mod build.

```



\---



\# 18. Risk Register



| Risk | Probability | Impact | Mitigation |

|---|---:|---:|---|

| GTA VI research claims are wrong | High initially | Medium | Evidence register and PC revalidation |

| SHVDN/game update breaks mod | High | High | Pin versions; compatibility matrix |

| AI dispatch becomes stuck | High | Medium | Timeouts, retries, cancellation |

| Passenger selection produces invalid fares | Medium | Medium | Controlled spawn fallback and validation |

| Duplicate purchase or payout | Medium | High | Idempotent settlement and transaction order |

| Reload leaves entities behind | Medium | High | Central ownership and cleanup |

| Save corruption | Medium | High | Atomic writes, validation, backups |

| Property purchase lacks a real interior | High | Medium | Describe actual functionality honestly |

| Advanced animation is infeasible | High | Medium | Time-boxed spikes and rejection path |

| Too much code is generated at once | High | High | One vertical slice per milestone |

| Native wrapper creates false portability confidence | Medium | High | Behavioral tests and platform content data |

| Performance declines over time | Medium | High | Throttling, caps, soak tests |

| Online-policy violation | Low if controlled | Critical | Hard offline scope and network-session guard |



\---



\# 19. Immediate Next Actions



Execute these in order:



1\. Create the repository and documentation files above.

2\. Record the exact GTA V, ScriptHookV, SHVDN3, and LemonUI versions.

3\. Create the .NET Framework 4.8 x64 project.

4\. Verify empty-host deployment and Insert-key reload.

5\. Integrate the completed `WeaponAndZoneUnlocker` prototype.

6\. Run its acceptance tests.

7\. Add configuration, logging, and the offline-session guard.

8\. Add centralized entity tracking \*\*before\*\* Emergency Dispatch.

9\. Implement only the police dispatch vertical slice.

10\. Test completion, timeout, destruction, death, and reload.

11\. Add ambulance and fire behavior.

12\. Implement one controlled taxi fare.

13\. Expand taxi to general fare selection.

14\. Freeze and release the MVP.

15\. Only then begin persistent properties.



\*\*The key project constraint is not code volume—it is lifecycle reliability.\*\* Three modules that survive death, cancellation, reload, and repeated use are a stronger foundation than twenty partially generated systems with duplicated natives, leaking entities, and untested save writes.





