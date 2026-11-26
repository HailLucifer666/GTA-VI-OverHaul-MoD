I’m still here. The previous response was incomplete and included code that should not be treated as compile-ready or verified. I’ll correct the record, check the GTA VI PC status and the linked Portwatch page, then give you a complete plan with usable supporting documents and a properly scoped first module.

You’re right — I stopped at a proposal instead of finishing the engineering handoff. Worse, the previous response presented unverified GTA VI claims as facts and included code that would not compile.



\*\*I’m retracting that draft. Below is the corrected, research-backed plan and a complete first-module implementation.\*\*



\# Classic GTA Overhaul  

\## Verified Project Baseline · November 26, 2026



\### Council verdict



\*\*Proceed with GTA V prototyping. Do not begin an unofficial GTA VI port or claim GTA VI compatibility.\*\*



The important distinction is:



> \*\*GTA VI has launched on consoles. That does not mean GTA VI has launched on PC, and a community compatibility project is not an official PC release or Script Hook VI.\*\*



Rockstar’s launch announcement confirms availability on PlayStation 5 and Xbox Series X|S. Its platform support information likewise lists those console platforms, not PC. \[\[1]](https://www.rockstargames.com/newswire/article/39akk424a9oo53/grand-theft-auto-vi-now-available)



The PortWatch page you linked tracks unofficial attempts to run console-derived material through compatibility layers, recompilation projects, and private builds. It reports experimental performance, rendering problems, incomplete story progress, and a Take-Two takedown affecting a port repository and its forks. It does \*\*not\*\* establish an officially released PC version or a supported Script Hook VI API. \[\[2]](https://portwatch-vi.xyz/#race)



Therefore:



\- \*\*No leaked console files.\*\*

\- \*\*No unofficial executable ports.\*\*

\- \*\*No invented GTA VI native hashes.\*\*

\- \*\*No promise that a future PC scripting API will resemble SHVDN.\*\*

\- \*\*Build reusable gameplay logic now; implement a GTA VI adapter only after an official PC release and a legally usable, documented modding interface exist.\*\*



Rockstar’s current Mod Guidelines also prohibit combining IP/assets between titles, modifying games to operate on unreleased platforms, and expanding established storylines. They require non-commercial modding outside authorized channels. “Offline” alone does not make every mod compliant. \[\[3]](https://www.rockstargames.com/community-resources/mod-guidelines)



\---



\# 1. Corrected product definition



\## Product name



\*\*Classic Sandbox Overhaul\*\*



Use “Classic GTA” only as an informal project description, not as branding suggesting Rockstar affiliation.



\## Product goal



Restore the feel of older GTA sandbox systems through \*\*original, optional, single-player activities\*\*:



\- Earn money through repeatable jobs.

\- Spend money on useful ownership systems.

\- Receive recurring benefits from owned assets.

\- Carry a broader personal weapon inventory.

\- Summon useful world services.

\- Keep progression persistent across sessions.



\## What this project is not



It is not:



\- An official GTA VI modification.

\- A recreation of removed GTA VI missions.

\- A port of assets from another GTA title.

\- A multiplayer trainer.

\- A leaked-build compatibility layer.

\- A guaranteed future GTA VI mod.



\## Recommended design principle



\*\*Restore mechanics, not copyrighted content.\*\*



For example:



| Desired classic experience | Compliant implementation direction |

|---|---|

| Taxi work | Original pickup/drop-off activity using the host game’s existing taxi vehicles |

| Property ownership | Original purchase points and persistence, without importing buildings from another game |

| Passive income | Original simulation rules and notifications |

| Emergency dispatch | Original service state machine using host-game entities |

| Pocket arsenal | Host-game weapon inventory APIs |

| Stock market | Fictional, local-only simulated companies and prices |

| Companion hangouts | Original companion behavior, without rewriting official character storylines |

| Skateboarding/minigames | Original mechanics/assets where feasible; defer if they require imported content |



This is a design recommendation, not a legal guarantee. Distribution still requires review against the applicable terms and guidelines.



\---



\# 2. Evidence register



Your screenshot is a \*\*feature hypothesis list\*\*, not a verified specification.



Claims such as “tennis was cut for quality reasons,” “furniture customization was scrapped,” or specific interview statements need primary-source verification before entering the PRD.



| Claim | Current treatment |

|---|---|

| GTA VI console release | Confirmed by Rockstar \[\[1]](https://www.rockstargames.com/newswire/article/39akk424a9oo53/grand-theft-auto-vi-now-available) |

| Official GTA VI PC release | Not established by the official sources reviewed |

| Script Hook VI availability | Not established |

| PortWatch progress percentages | Community measurements/claims, not official compatibility guarantees \[\[2]](https://portwatch-vi.xyz/#race) |

| Screenshot’s removed-feature claims | Unverified backlog hypotheses |

| GTA V SHVDN3 scripting | Supported by the official project documentation \[\[4]](https://github.com/scripthookvdotnet/scripthookvdotnet/wiki/Getting-Started) |

| LemonUI SHVDN3 menus | Supported by the official examples \[\[5]](https://github.com/LemonUIbyLemon/Examples/blob/master/LemonUI.SHVDN3.Example/Example.cs) |

| Current Rockstar mod restrictions | Published in the Mod Guidelines \[\[3]](https://www.rockstargames.com/community-resources/mod-guidelines) |



\*\*Rule:\*\* no feature enters “Confirmed” status merely because it appears in a video, screenshot, wiki, or AI answer.



\---



\# 3. Complete supporting-document package



Create these files in the repository. Their substantive requirements are defined in the sections that follow.



```text

ClassicSandbox/

├─ README.md

├─ LICENSE

├─ THIRD\_PARTY\_NOTICES.md

├─ .gitignore

├─ ClassicSandbox.sln

│

├─ docs/

│  ├─ 00\_PROJECT\_CHARTER.md

│  ├─ 01\_PRD.md

│  ├─ 02\_MVP\_DEFINITION.md

│  ├─ 03\_FEATURE\_CATALOG.md

│  ├─ 04\_ARCHITECTURE.md

│  ├─ 05\_TECH\_STACK.md

│  ├─ 06\_SETUP\_GUIDE.md

│  ├─ 07\_TEST\_PLAN.md

│  ├─ 08\_RELEASE\_CHECKLIST.md

│  ├─ 09\_RISK\_REGISTER.md

│  ├─ 10\_POLICY\_AND\_SAFETY.md

│  ├─ 11\_GTA\_VI\_MIGRATION\_GATE.md

│  ├─ 12\_RESEARCH\_EVIDENCE.md

│  └─ adr/

│     ├─ ADR-001-phase-gated-development.md

│     ├─ ADR-002-stable-api-baseline.md

│     ├─ ADR-003-game-adapter-boundary.md

│     ├─ ADR-004-original-content-only.md

│     └─ ADR-005-no-speculative-vi-native-map.md

│

├─ src/

│  └─ ClassicSandbox.Modules/

│     ├─ ClassicSandbox.Modules.csproj

│     ├─ WeaponAndZoneUnlocker.cs

│     └─ Properties/

│        └─ AssemblyInfo.cs

│

├─ config/

│  └─ ClassicSandbox.example.ini

│

└─ tests/

&#x20;  ├─ MANUAL\_TEST\_CASES.md

&#x20;  └─ BUG\_REPORT\_TEMPLATE.md

```



\*\*Important:\*\* Module 1 below deliberately uses only APIs present in stable SHVDN v3.6.0. It does not depend on recently added nightly-only methods.



\---



\# 4. PRD — product requirements



\## 4.1 Target user



A PC player who wants optional classic-style sandbox activities in single-player, without multiplayer interference or mandatory changes to the main story.



\## 4.2 Product pillars



1\. \*\*Optional:\*\* every major system can be disabled.

2\. \*\*Persistent:\*\* ownership and earnings survive reloads.

3\. \*\*Recoverable:\*\* failed activities clean up and can be restarted.

4\. \*\*Lightweight:\*\* no expensive world scans every frame.

5\. \*\*Portable logic:\*\* game-specific calls stay behind adapters.

6\. \*\*Original content:\*\* no cross-title asset imports.

7\. \*\*Single-player only:\*\* no online-service interaction.



\## 4.3 Functional requirements



\### Weapons



\- \*\*WPN-01:\*\* Provide an F10 arsenal menu.

\- \*\*WPN-02:\*\* Grant a curated set of valid host-game weapons.

\- \*\*WPN-03:\*\* Fill granted weapons to their reported ammunition capacity.

\- \*\*WPN-04:\*\* Allow manual weapon selection from the menu.

\- \*\*WPN-05:\*\* Reapply weapon-switch permission while the override is enabled.

\- \*\*WPN-06:\*\* Restore the tracked player ped’s original switch permission when the module stops.

\- \*\*WPN-07:\*\* Avoid claiming that arbitrary mission, interior, animation, or control locks have been bypassed.



\### Emergency services



\- \*\*SVC-01:\*\* Provide police, ambulance, and fire-service requests.

\- \*\*SVC-02:\*\* Limit active dispatches.

\- \*\*SVC-03:\*\* Track spawned vehicles, personnel, and blips.

\- \*\*SVC-04:\*\* Clean up on completion, cancellation, death, and script abort.

\- \*\*SVC-05:\*\* Heal only when the ambulance actually reaches the player.

\- \*\*SVC-06:\*\* Treat fire suppression as a separate tested capability, not an assumed vehicle behavior.



\### Taxi



\- \*\*TAX-01:\*\* Detect an eligible taxi vehicle.

\- \*\*TAX-02:\*\* Use explicit states: `Idle`, `Searching`, `Pickup`, `EnRoute`, `Payout`.

\- \*\*TAX-03:\*\* Create one passenger and one destination at a time.

\- \*\*TAX-04:\*\* Pay only after successful completion.

\- \*\*TAX-05:\*\* Cancel safely when the player leaves the vehicle or the passenger becomes invalid.

\- \*\*TAX-06:\*\* Prevent duplicate payouts.



\### Ownership and economy



\- \*\*OWN-01:\*\* Purchase only with sufficient funds.

\- \*\*OWN-02:\*\* Persist ownership and transaction history.

\- \*\*OWN-03:\*\* Prevent duplicate purchases.

\- \*\*OWN-04:\*\* Generate income from elapsed in-game time.

\- \*\*OWN-05:\*\* Cap offline accumulation.

\- \*\*OWN-06:\*\* Use atomic file replacement for saves.

\- \*\*OWN-07:\*\* Recover from malformed or missing configuration.



\### Future activities



Vigilante, rampages, stock simulation, companions, and minigames must each have:



\- A start condition.

\- A cancel condition.

\- A success condition.

\- A failure condition.

\- A cleanup path.

\- A persistence policy.

\- A bounded entity count.



\## 4.4 Non-functional requirements



| Area | Requirement |

|---|---|

| Performance | No full-world entity scans in per-frame callbacks |

| Stability | Validate entity handles before use |

| Reload safety | Unsubscribe events and release tracked resources |

| Save safety | Backup plus temporary-file write and replacement |

| Compatibility | Compile against stable SHVDN3 APIs |

| UX | Clear notifications and visible enabled/disabled state |

| Safety | No online functionality |

| Legal | No redistributed game assets or leaked material |

| Maintainability | One responsibility per module/service |

| Testability | Pure rules separated from engine calls where practical |



\---



\# 5. MVP definition



\## Recommended MVP: two releases, not one oversized prototype



\### MVP-A — playable foundation



Includes:



1\. Visual Studio project.

2\. Reliable build/deployment workflow.

3\. Weapon arsenal.

4\. Weapon-switch override.

5\. Notifications.

6\. Abort cleanup.

7\. Manual test checklist.



\*\*Exit condition:\*\* the player can repeatedly load, use, disable, and reload the module without duplicated menus or retained overrides.



\### MVP-B — classic gameplay loop



Adds:



1\. One purchasable property.

2\. Persistent ownership.

3\. Passive income.

4\. One complete taxi job.

5\. One emergency-service request.

6\. Save recovery and cancellation tests.



\*\*Exit condition:\*\*



```text

Complete taxi job

&#x20;   → receive money

&#x20;   → buy property

&#x20;   → reload game

&#x20;   → ownership remains

&#x20;   → property generates income

```



That is the smallest convincing demonstration of the product.



\## Not in the MVP



\- Prone crawling.

\- Dual wielding.

\- Skateboarding.

\- Tennis/bowling/blackjack.

\- Full first-person camera replacement.

\- Custom animations.

\- Cross-game maps.

\- GTA VI native integration.



These are not “easy toggles.” They require engine capabilities that must be demonstrated individually.



\---



\# 6. Feature catalog and delivery order



| Priority | Feature | Difficulty | Main risk | Release |

|---|---|---:|---|---|

| P0 | Arsenal menu | Low | Invalid weapon assets | MVP-A |

| P0 | Weapon-switch override | Low | Assuming it bypasses all restrictions | MVP-A |

| P1 | Emergency dispatch | Medium | Entity leaks, pathfinding | MVP-B |

| P1 | Property purchase | Medium | Save corruption, duplicate purchase | MVP-B |

| P1 | Passive income | Medium | Time accounting exploits | MVP-B |

| P1 | Taxi job | Medium | Passenger lifecycle and route failures | MVP-B |

| P2 | Vehicle dealerships | Medium | Spawn location and ownership persistence | 0.3 |

| P2 | Vigilante | Medium | Combat AI and cleanup | 0.3 |

| P2 | Rampage | Medium | Kill attribution and timer correctness | 0.3 |

| P2 | Stock simulator | Medium | Save schema and economic balance | 0.4 |

| P2 | Stat display/progression | Medium | Confusing custom stats with engine stats | 0.4 |

| P3 | First-person free roam | High | Camera/control conflicts | Research |

| P3 | Companion hangouts | High | AI, scheduling, dialogue constraints | Research |

| P3 | Guard dogs | High | Animal AI and navigation | Research |

| P3 | Prone/skateboarding | Very high | Animation and physics support | Research |

| P3 | Dual wielding | Very high | Animation, aiming, damage synchronization | Research |

| P3 | Sports/casino minigames | Very high | UI, rules, animation, original assets | Research |



\*\*Backlog rule:\*\* a feature cannot move from Research to Planned until a small host-game feasibility test succeeds.



\---



\# 7. Technical architecture



\## 7.1 Council decision



Use \*\*modular scripts plus shared services\*\*, not a giant monolithic `Tick`.



```text

Script entry points

&#x20;   ↓

Feature modules / state machines

&#x20;   ↓

Gameplay services

&#x20;   ↓

IGameAdapter

&#x20;   ↓

GTA V adapter

&#x20;   ↓

Verified SHVDN3 APIs

```



A future adapter would sit beside the GTA V adapter:



```text

IGameAdapter

├─ GtaVGameAdapter       ← implemented now

└─ FutureGameAdapter     ← not implemented; no guessed API

```



\## 7.2 Proposed boundaries



```csharp

public interface IModModule

{

&#x20;   string Id { get; }

&#x20;   void Initialize();

&#x20;   void Tick();

&#x20;   void Shutdown();

}

```



This is the \*\*project’s own interface\*\*, not a claimed SHVDN API.



Recommended services:



```text

IWeaponService

IDispatchService

IEconomyService

IOwnershipService

IPersistenceService

IWorldMarkerService

INotificationService

IClockService

```



Do not create all of these immediately. Introduce each when the corresponding feature needs it.



\## 7.3 Native-wrapper rule



The Architect’s original requirement is retained, but corrected:



\- Prefer verified SHVDN wrappers.

\- Put unavoidable raw native calls in a GTA V-specific adapter.

\- Record the native name, hash source, parameters, return type, and tested game build.

\- Never place speculative GTA VI hashes beside GTA V hashes as though they were known equivalents.



\## 7.4 Persistence model



Eventually replace a flat INI blob with:



```text

scripts/ClassicSandbox/

├─ settings.ini

├─ profile.json

├─ profile.backup.json

└─ logs/

```



For the first module, no save file is necessary.



Later profile data should include:



```json

{

&#x20; "schemaVersion": 1,

&#x20; "cashDelta": 0,

&#x20; "ownedProperties": \[],

&#x20; "ownedVehicles": \[],

&#x20; "businessAccumulators": \[],

&#x20; "stockHoldings": \[],

&#x20; "activityStatistics": {}

}

```



Use a schema version from the first persistent release.



\---



\# 8. Technology stack



\## Verified baseline



| Layer | Choice |

|---|---|

| IDE | Visual Studio Community 2022 |

| Project type | Class Library \*\*(.NET Framework)\*\* |

| Target | .NET Framework 4.8 |

| Language | C# 7.3-compatible syntax for maximum beginner-project compatibility |

| Compile-time scripting reference | `ScriptHookVDotNet3` 3.6.0 |

| Compile-time UI reference | `LemonUI.SHVDN3` 2.2.0 |

| Runtime hook | ScriptHookV |

| Runtime .NET loader | Compatible SHVDN nightly |

| Menu runtime | Matching LemonUI SHVDN3 release files |

| Key events | `System.Windows.Forms.KeyEventArgs` |

| Source control | Git |

| Cost | $0 for the development tools |



The stable SHVDN NuGet package targets .NET Framework 4.8. \[\[6]](https://www.nuget.org/packages/ScriptHookVDotNet3/3.6.0)



LemonUI.SHVDN3 2.2.0 also targets .NET Framework 4.8 and depends on SHVDN3 3.6.0 or newer. \[\[7]](https://www.nuget.org/packages/LemonUI.SHVDN3/2.2.0)



\### Important distinction



\*\*Compile-time reference ≠ installed runtime.\*\*



The SHVDN project explicitly recommends compiling against stable APIs while using a compatible nightly runtime when the current game build requires it. It also warns that stable 3.6.0 has compatibility problems with GTA V builds 1.0.3258.0 and later. \[\[8]](https://github.com/scripthookvdotnet/scripthookvdotnet)



At the time checked, the latest published nightly was `v3.7.0-nightly.206`, released November 15, 2026. \[\[9]](https://github.com/scripthookvdotnet/scripthookvdotnet-nightly/releases)



ScriptHookV’s published page listed support for Legacy build `1.0.3889.0` and Enhanced build `1.0.1158.13`. Verify your exact executable version before installation. \[\[10]](https://www.dev-c.com/gtav/scripthookv/)



\*\*Edition warning:\*\* upstream SHVDN and third-party “SHVDN Enhanced” forks are different projects. Do not mix their binaries. This handoff targets the upstream SHVDN distribution; verify your GTA V edition/build against its current documentation before testing.



\---



\# 9. Visual Studio setup checklist



\## A. Install prerequisites



1\. Install Visual Studio Community 2022.

2\. Select \*\*.NET desktop development\*\*.

3\. Confirm \*\*.NET Framework 4.8 targeting pack\*\* is installed.

4\. Install the latest supported Microsoft Visual C++ redistributable required by ScriptHookV.

5\. Install Git if desired.



\## B. Create the project



1\. \*\*Create a new project\*\*.

2\. Choose \*\*Class Library (.NET Framework)\*\*.

3\. Do \*\*not\*\* choose the modern “Class Library” template.

4\. Name it:



```text

ClassicSandbox.Modules

```



5\. Select \*\*.NET Framework 4.8\*\*.

6\. Set platform to \*\*x64\*\*.

7\. Delete the generated `Class1.cs`.



The official SHVDN guide specifies the .NET Framework class-library template and a script inheriting from `GTA.Script`. \[\[4]](https://github.com/scripthookvdotnet/scripthookvdotnet/wiki/Getting-Started)



\## C. Add references



Use NuGet for compile-time references:



```powershell

Install-Package ScriptHookVDotNet3 -Version 3.6.0

Install-Package LemonUI.SHVDN3 -Version 2.2.0

```



Add a reference to:



```text

System.Windows.Forms

```



For referenced assemblies, set \*\*Copy Local = False\*\* where appropriate. The game already supplies the SHVDN and LemonUI runtime assemblies.



\## D. Install the GTA V runtime



In the GTA V root directory containing `GTA5.exe`:



```text

GTA V/

├─ GTA5.exe

├─ ScriptHookV.dll

├─ dinput8.dll

├─ ScriptHookVDotNet-Loader.asi

├─ ScriptHookVDotNet.dll

├─ ScriptHookVDotNet2.dll

├─ ScriptHookVDotNet3.dll

├─ ScriptHookVDotNet.ini

└─ scripts/

&#x20;  ├─ ClassicSandbox.Modules.dll

&#x20;  ├─ LemonUI.SHVDN3.dll

&#x20;  └─ other LemonUI SHVDN3 release files

```



Current upstream SHVDN uses `ScriptHookVDotNet-Loader.asi`; older guides may call it `ScriptHookVDotNet.asi`. Update the loader and DLLs together. \[\[8]](https://github.com/scripthookvdotnet/scripthookvdotnet)



Do not copy RPH or FiveM versions of LemonUI into this installation.



\## E. Configure reload



Current SHVDN defaults include:



```ini

ReloadKeyBinding=None

ConsoleKeyBinding=F4

ScriptsLocation="scripts"

AutoLoadScripts=true

```



To enable the requested Insert hot-reload:



```ini

ReloadKeyBinding=Insert

ConsoleKeyBinding=F4

ScriptsLocation="scripts"

AutoLoadScripts=true

```



The current configuration file confirms these setting names and defaults. \[\[11]](https://github.com/scripthookvdotnet/scripthookvdotnet/blob/main/ScriptHookVDotNet.ini)



The documented alternative is:



```text

F4 → Reload() → Enter

```



\## F. Post-build deployment



Set the game path once in the project file, or use a local environment variable.



Example post-build command:



```bat

if not defined GTAV\_DIR exit /b 0

if not exist "%GTAV\_DIR%\\scripts" mkdir "%GTAV\_DIR%\\scripts"

copy /Y "$(TargetPath)" "%GTAV\_DIR%\\scripts\\$(TargetFileName)"

```



For a Steam installation, an example environment variable is:



```text

GTAV\_DIR=C:\\Program Files (x86)\\Steam\\steamapps\\common\\Grand Theft Auto V

```



Your actual path may differ.



If Windows blocks writing into Program Files, do \*\*not\*\* run Visual Studio permanently as administrator. Prefer a game library in a user-writable location or adjust the deployment method.



\---



\# 10. Module 1 — corrected scope



\## What the implementation actually does



\- Opens a LemonUI menu with \*\*F10\*\*.

\- Grants a curated arsenal of verified GTA V weapon enum members.

\- Uses asynchronous weapon-asset loading.

\- Fills ammo to the weapon’s reported maximum.

\- Supports individual weapon selection.

\- Reapplies `Ped.CanSwitchWeapons`.

\- Tracks the affected ped.

\- Restores the tracked ped’s switch permission on abort when safely possible.

\- Cleans up pending weapon-asset requests.

\- Avoids spawning vehicles, peds, props, or blips.



\## What it does not claim



It does \*\*not\*\* universally unlock firing inside every interior or mission.



`SET\_PED\_CAN\_SWITCH\_WEAPON` controls weapon switching. It is not proof that every scripted firing restriction, safe-zone rule, animation state, or disabled control has been removed. The native is documented as a weapon-switch toggle. \[\[12]](https://docs.fivem.net/natives/?\_0xED7F7EFE9FABF340)



A genuine zone-specific firing override requires:



1\. A reproducible restricted location.

2\. Identification of the actual blocking mechanism.

3\. A narrowly scoped intervention.

4\. Testing that normal mission behavior remains recoverable.



That belongs in a later, separately audited module.



\---



\# 11. Complete Module 1 code



Create:



```text

src/ClassicSandbox.Modules/WeaponAndZoneUnlocker.cs

```



Replace its contents with:



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



&#x20;           public ArsenalEntry(WeaponHash hash, string name)

&#x20;           {

&#x20;               Hash = hash;

&#x20;               Name = name;

&#x20;           }

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



&#x20;       private readonly ObjectPool pool = new ObjectPool();

&#x20;       private readonly NativeMenu menu =

&#x20;           new NativeMenu("Classic Sandbox", "Pocket Arsenal");



&#x20;       private readonly Queue<ArsenalEntry> loadQueue =

&#x20;           new Queue<ArsenalEntry>();



&#x20;       private readonly HashSet<WeaponHash> grantedWeapons =

&#x20;           new HashSet<WeaponHash>();



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

&#x20;           NativeItem grantAll = new NativeItem(

&#x20;               "Grant Full Arsenal",

&#x20;               "Loads the curated weapons and fills their ammunition.");



&#x20;           grantAll.Activated += delegate

&#x20;           {

&#x20;               BeginGrantAll();

&#x20;           };



&#x20;           NativeItem enableOverride = new NativeItem(

&#x20;               "Enable Switch Override",

&#x20;               "Allows the player ped to switch weapons while enabled.");



&#x20;           enableOverride.Activated += delegate

&#x20;           {

&#x20;               SetSwitchOverride(true);

&#x20;           };



&#x20;           NativeItem disableOverride = new NativeItem(

&#x20;               "Disable Switch Override",

&#x20;               "Restores the tracked ped's original switch permission.");



&#x20;           disableOverride.Activated += delegate

&#x20;           {

&#x20;               SetSwitchOverride(false);

&#x20;           };



&#x20;           NativeItem close = new NativeItem(

&#x20;               "Close",

&#x20;               "Close this menu. F10 opens it again.");



&#x20;           close.Activated += delegate

&#x20;           {

&#x20;               menu.Visible = false;

&#x20;           };



&#x20;           menu.Add(grantAll);

&#x20;           menu.Add(enableOverride);

&#x20;           menu.Add(disableOverride);

&#x20;           menu.Add(close);



&#x20;           for (int i = 0; i < Arsenal.Length; i++)

&#x20;           {

&#x20;               ArsenalEntry entry = Arsenal\[i];



&#x20;               NativeItem item = new NativeItem(

&#x20;                   entry.Name,

&#x20;                   "Grant, fill ammunition, and equip this weapon.");



&#x20;               item.Tag = entry;



&#x20;               item.Activated += delegate(object sender, EventArgs args)

&#x20;               {

&#x20;                   NativeItem activatedItem = sender as NativeItem;



&#x20;                   if (activatedItem == null)

&#x20;                   {

&#x20;                       return;

&#x20;                   }



&#x20;                   ArsenalEntry selected =

&#x20;                       activatedItem.Tag as ArsenalEntry;



&#x20;                   if (selected != null)

&#x20;                   {

&#x20;                       BeginSingleGrant(selected);

&#x20;                   }

&#x20;               };



&#x20;               menu.Add(item);

&#x20;           }

&#x20;       }



&#x20;       private void OnKeyDown(object sender, KeyEventArgs e)

&#x20;       {

&#x20;           if (shuttingDown || e.KeyCode != Keys.F10)

&#x20;           {

&#x20;               return;

&#x20;           }



&#x20;           if (e.Control || e.Alt || e.Shift)

&#x20;           {

&#x20;               return;

&#x20;           }



&#x20;           if (Game.IsLoading || Game.IsCutsceneActive || Game.IsPaused)

&#x20;           {

&#x20;               return;

&#x20;           }



&#x20;           if (menu.Visible)

&#x20;           {

&#x20;               menu.Visible = false;

&#x20;               return;

&#x20;           }



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

&#x20;           if (shuttingDown)

&#x20;           {

&#x20;               return;

&#x20;           }



&#x20;           pool.Process();



&#x20;           if (Game.IsLoading || Game.IsCutsceneActive)

&#x20;           {

&#x20;               return;

&#x20;           }



&#x20;           Ped ped = Game.Player.Character;



&#x20;           if (!IsPedUsable(ped))

&#x20;           {

&#x20;               if (menu.Visible)

&#x20;               {

&#x20;                   menu.Visible = false;

&#x20;               }



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

&#x20;           if (grantInProgress)

&#x20;           {

&#x20;               Notification.Show("An arsenal load is already running.");

&#x20;               return;

&#x20;           }



&#x20;           Ped ped = Game.Player.Character;



&#x20;           if (!IsPedUsable(ped))

&#x20;           {

&#x20;               Notification.Show("The player is not ready.");

&#x20;               return;

&#x20;           }



&#x20;           loadQueue.Clear();

&#x20;           grantedWeapons.Clear();



&#x20;           for (int i = 0; i < Arsenal.Length; i++)

&#x20;           {

&#x20;               loadQueue.Enqueue(Arsenal\[i]);

&#x20;           }



&#x20;           grantInProgress = true;



&#x20;           Notification.Show(

&#x20;               "Pocket Arsenal: loading " +

&#x20;               loadQueue.Count.ToString() +

&#x20;               " weapons.");

&#x20;       }



&#x20;       private void BeginSingleGrant(ArsenalEntry entry)

&#x20;       {

&#x20;           if (grantInProgress)

&#x20;           {

&#x20;               Notification.Show("Wait for the current arsenal load to finish.");

&#x20;               return;

&#x20;           }



&#x20;           Ped ped = Game.Player.Character;



&#x20;           if (!IsPedUsable(ped))

&#x20;           {

&#x20;               Notification.Show("The player is not ready.");

&#x20;               return;

&#x20;           }



&#x20;           loadQueue.Clear();

&#x20;           grantedWeapons.Clear();

&#x20;           loadQueue.Enqueue(entry);

&#x20;           grantInProgress = true;

&#x20;       }



&#x20;       private void ProcessAssetQueue(Ped ped)

&#x20;       {

&#x20;           if (!grantInProgress)

&#x20;           {

&#x20;               return;

&#x20;           }



&#x20;           if (!hasPendingAsset)

&#x20;           {

&#x20;               if (loadQueue.Count == 0)

&#x20;               {

&#x20;                   FinishGrantAll();

&#x20;                   return;

&#x20;               }



&#x20;               ArsenalEntry next = loadQueue.Dequeue();

&#x20;               WeaponAsset asset = new WeaponAsset(next.Hash);



&#x20;               if (!asset.IsValid)

&#x20;               {

&#x20;                   return;

&#x20;               }



&#x20;               pendingEntry = next;

&#x20;               pendingAsset = asset;

&#x20;               hasPendingAsset = true;

&#x20;               assetRequestStartedAt = Environment.TickCount;



&#x20;               pendingAsset.Request();

&#x20;               return;

&#x20;           }



&#x20;           if (pendingAsset.IsLoaded)

&#x20;           {

&#x20;               Weapon weapon = ped.Weapons.Give(

&#x20;                   pendingEntry.Hash,

&#x20;                   0,

&#x20;                   false,

&#x20;                   false);



&#x20;               if (weapon != null \&\& weapon.IsPresent)

&#x20;               {

&#x20;                   int maximum = weapon.MaxAmmo;



&#x20;                   if (maximum > 0)

&#x20;                   {

&#x20;                       weapon.Ammo = maximum;

&#x20;                   }



&#x20;                   grantedWeapons.Add(pendingEntry.Hash);

&#x20;               }



&#x20;               ReleasePendingAsset();

&#x20;               return;

&#x20;           }



&#x20;           int elapsed = unchecked(

&#x20;               Environment.TickCount - assetRequestStartedAt);



&#x20;           if (elapsed >= 3000)

&#x20;           {

&#x20;               Notification.Show(

&#x20;                   "Could not load: " + pendingEntry.Name);



&#x20;               ReleasePendingAsset();

&#x20;               return;

&#x20;           }



&#x20;           pendingAsset.Request();

&#x20;       }



&#x20;       private void FinishGrantAll()

&#x20;       {

&#x20;           grantInProgress = false;



&#x20;           Ped ped = Game.Player.Character;



&#x20;           if (IsPedUsable(ped) \&\&

&#x20;               grantedWeapons.Contains(WeaponHash.Pistol))

&#x20;           {

&#x20;               ped.Weapons.Select(WeaponHash.Pistol);

&#x20;           }



&#x20;           Notification.Show(

&#x20;               "Pocket Arsenal ready: " +

&#x20;               grantedWeapons.Count.ToString() +

&#x20;               " weapons available.");



&#x20;           grantedWeapons.Clear();

&#x20;       }



&#x20;       private void SetSwitchOverride(bool enabled)

&#x20;       {

&#x20;           Ped ped = Game.Player.Character;



&#x20;           if (!IsPedUsable(ped))

&#x20;           {

&#x20;               Notification.Show("The player is not ready.");

&#x20;               return;

&#x20;           }



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

&#x20;           if (hasOriginalSwitchState \&\&

&#x20;               trackedPedHandle == ped.Handle)

&#x20;           {

&#x20;               return;

&#x20;           }



&#x20;           RestoreTrackedPed();



&#x20;           trackedPedHandle = ped.Handle;



&#x20;           originalCanSwitchWeapons = Function.Call<bool>(

&#x20;               Hash.GET\_PED\_CONFIG\_FLAG,

&#x20;               ped.Handle,

&#x20;               208,

&#x20;               false);



&#x20;           hasOriginalSwitchState = true;

&#x20;       }



&#x20;       private void RestoreTrackedPed()

&#x20;       {

&#x20;           if (!hasOriginalSwitchState || trackedPedHandle == 0)

&#x20;           {

&#x20;               return;

&#x20;           }



&#x20;           if (!Function.Call<bool>(

&#x20;                   Hash.DOES\_ENTITY\_EXIST,

&#x20;                   trackedPedHandle))

&#x20;           {

&#x20;               return;

&#x20;           }



&#x20;           Function.Call(

&#x20;               Hash.SET\_PED\_CAN\_SWITCH\_WEAPON,

&#x20;               trackedPedHandle,

&#x20;               originalCanSwitchWeapons);

&#x20;       }



&#x20;       private static bool IsPedUsable(Ped ped)

&#x20;       {

&#x20;           return ped != null \&\&

&#x20;                  ped.Exists() \&\&

&#x20;                  !ped.IsDead \&\&

&#x20;                  ped.IsHuman;

&#x20;       }



&#x20;       private void ReleasePendingAsset()

&#x20;       {

&#x20;           if (hasPendingAsset)

&#x20;           {

&#x20;               pendingAsset.MarkAsNoLongerNeeded();

&#x20;           }



&#x20;           hasPendingAsset = false;

&#x20;           pendingEntry = null;

&#x20;       }



&#x20;       private void OnAborted(object sender, EventArgs e)

&#x20;       {

&#x20;           if (shuttingDown)

&#x20;           {

&#x20;               return;

&#x20;           }



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



\## Why this version is materially different



The rejected draft contained several concrete defects:



\- `Keys` was used without importing `System.Windows.Forms`.

\- `ParentWindow` was treated as a LemonUI menu base class.

\- `ButtonItem` was invented.

\- `Game.GameTimeDelta` was used although the verified stable API exposes `Game.GameTime`.

\- `Game.IsContentUpdateAvailable` was unrelated to weapon loading.

\- `WeaponHash.BaseballBat` was used instead of the verified `WeaponHash.Bat`.

\- A fake town-center radius was described as restricted-zone detection.

\- The menu was created but never processed.

\- Automatic weapon cycling was not the requested useful toggle behavior.

\- The claimed audit approved code that had not been compiled or tested.



The corrected implementation follows the actual stable weapon APIs and LemonUI’s `ObjectPool`/`NativeMenu` pattern. \[\[13]](https://github.com/scripthookvdotnet/scripthookvdotnet/blob/v3.6.0/source/scripting\_v3/GTA/Weapons/WeaponCollection.cs) \[\[5]](https://github.com/LemonUIbyLemon/Examples/blob/master/LemonUI.SHVDN3.Example/Example.cs)



\### Audit status — honest version



\- \*\*Source/API review:\*\* completed against the referenced stable sources.

\- \*\*Compiler execution:\*\* not performed in this chat.

\- \*\*GTA V runtime test:\*\* not performed in this chat.

\- \*\*Universal interior firing unlock:\*\* not implemented or claimed.

\- \*\*GTA VI compatibility:\*\* unknown.



So this is a \*\*complete implementation candidate\*\*, not a falsely certified “tested and working” release.



\---



\# 12. Red-team findings and required follow-up



\## Finding 1 — weapon switching is not firing permission



\*\*Severity:\*\* high requirement mismatch.



\*\*Decision:\*\* keep the switch override, but rename the future zone-bypass work separately.



Do not enable all controls globally. That can break menus, cutscenes, missions, and vehicle input.



\## Finding 2 — weapon slots can replace other weapons



GTA’s inventory uses weapon slots. Adding multiple weapons from the same slot can replace an existing weapon rather than preserving every item indefinitely.



\*\*Decision:\*\* describe the feature as a broad curated arsenal, not a mathematically unlimited inventory.



\## Finding 3 — ammo maxima are engine-defined



“Max ammo” means the weapon’s reported capacity, not an invented universal number such as 9999.



\*\*Decision:\*\* use `Weapon.MaxAmmo` and `Weapon.Ammo`.



\## Finding 4 — asset loading must yield



Blocking the script thread while waiting for an asset can stall the game.



\*\*Decision:\*\* request once, check on later ticks, retry while pending, and time out.



\## Finding 5 — reload cleanup matters even without spawned entities



This module creates no world entities, but it does alter player state and request weapon assets.



\*\*Decision:\*\* release pending assets and restore the tracked switch state on abort.



\## Finding 6 — original-state restoration is best effort



A ped handle can disappear during a save load, model change, or game transition.



\*\*Decision:\*\* restore only if the tracked entity still exists; never delete or mark the player ped as no longer needed.



\## Finding 7 — policy risk remains



Even a technically correct single-player mod can violate content or distribution restrictions.



\*\*Decision:\*\* keep the project original, non-commercial, offline, and free of imported Rockstar assets. Review the current guidelines before publication. \[\[3]](https://www.rockstargames.com/community-resources/mod-guidelines)



\---



\# 13. Test plan



\## Build tests



| ID | Action | Expected result |

|---|---|---|

| B01 | Restore NuGet packages | Both references resolve |

| B02 | Build x64 Class Library | DLL produced |

| B03 | Inspect output folder | No conflicting SHVDN runtime copies deployed accidentally |

| B04 | Run post-build event | DLL appears in `GTA V/scripts` |



\## Runtime smoke tests



| ID | Action | Expected result |

|---|---|---|

| R01 | Start Story Mode | Script loads without an immediate abort |

| R02 | Press F10 | Arsenal menu opens |

| R03 | Press F10 again | Menu closes |

| R04 | Grant Full Arsenal | Loading notification appears |

| R05 | Wait for completion | Ready notification appears |

| R06 | Open native weapon wheel | Granted weapons appear according to engine slot rules |

| R07 | Select Pistol from menu | Pistol is equipped |

| R08 | Enable switch override | Weapon switching remains available |

| R09 | Disable override | Tracked original state is restored |

| R10 | Reload with Insert | One menu instance remains; no duplicate handlers |



\## Failure tests



| ID | Action | Expected result |

|---|---|---|

| F01 | Press F10 during loading/cutscene | No menu opened |

| F02 | Trigger death during asset loading | Queue stops safely |

| F03 | Reload during asset loading | Pending asset released |

| F04 | Request arsenal twice quickly | Second request rejected while first runs |

| F05 | Load a save | No stale ped handle is manipulated |

| F06 | Enter a mission with weapon restrictions | No claim of universal bypass; record actual behavior |



\## Performance tests



Measure:



\- Frame time before enabling the module.

\- Frame time with the menu closed.

\- Frame time during arsenal loading.

\- Frame time after ten reloads.

\- Number of loaded scripts after repeated reloads.



Do not publish a performance claim until measured on a named machine and game build.



\---



\# 14. Phase-gated roadmap



\## Phase 0 — evidence and compliance



Deliver:



\- Evidence register.

\- Feature-status catalog.

\- Policy review.

\- Exact GTA V edition/build record.

\- Repository and license.



\*\*Gate:\*\* no leaked material or unsupported GTA VI assumptions.



\## Phase 1 — development foundation



Deliver:



\- VS solution.

\- Stable compile references.

\- Runtime installation guide.

\- Post-build deployment.

\- Reload instructions.

\- Logging and bug-report template.



\*\*Gate:\*\* a minimal script loads and reloads cleanly.



\## Phase 2 — MVP-A



Deliver:



\- Module 1.

\- Manual tests.

\- Cleanup audit.

\- Known-limitations document.



\*\*Gate:\*\* arsenal works repeatedly without duplicated state.



\## Phase 3 — MVP-B



Deliver:



\- Persistence service.

\- One property.

\- Passive income.

\- Taxi state machine.

\- One dispatch service.



\*\*Gate:\*\* the complete earn/spend/save/reload loop works.



\## Phase 4 — expanded sandbox



Deliver:



\- Dealerships.

\- Vigilante.

\- Rampages.

\- Stock simulator.

\- More properties and businesses.



\*\*Gate:\*\* each activity has bounded entities and tested cancellation.



\## Phase 5 — advanced research



Investigate:



\- First-person camera.

\- Companions.

\- Animals.

\- Prone movement.

\- Dual wielding.

\- Skateboarding.

\- Minigames.



\*\*Gate:\*\* a feasible host-engine technique exists; otherwise remain in Research.



\## Phase 6 — future official PC adaptation



Only begin when:



1\. The game has an official PC release.

2\. A usable modding interface is publicly documented.

3\. Its license permits the intended work.

4\. Required tools are legally distributable.

5\. A clean test installation is available.

6\. The relevant native/API mappings are verified.



\*\*No date is promised.\*\*



\---



\# 15. GTA VI migration strategy



\## What can be prepared now



Portable logic:



\- Taxi payout formulas.

\- State transitions.

\- Ownership rules.

\- Income accumulators.

\- Stock-price simulation.

\- Save schema.

\- Menu view models.

\- Activity definitions.

\- Cooldowns and caps.

\- Test cases.



\## What cannot be responsibly prepared now



\- GTA VI native hashes.

\- GTA VI entity classes.

\- GTA VI memory offsets.

\- Script Hook VI function signatures.

\- Leonida coordinates derived from leaked files.

\- Asset names extracted from console archives.

\- Executable patches.

\- Compatibility-layer hooks.



\## Correct abstraction example



```csharp

public interface IEconomyService

{

&#x20;   int GetCash();

&#x20;   bool TrySpend(int amount);

&#x20;   void Credit(int amount);

}

```



The interface expresses a gameplay need. It does not pretend to know how a future game implements money.



A future adapter must earn its place through verified documentation and tests—not through a speculative dictionary of renamed GTA V natives.



\---



\# 16. Risk register



| Risk | Severity | Mitigation |

|---|---:|---|

| Treating PortWatch as an official PC launch | Critical | Separate official evidence from community claims |

| Using leaked console material | Critical | Prohibit it entirely |

| Inventing GTA VI APIs | Critical | No future adapter until verified |

| Cross-title asset import | High | Original mechanics and host-game assets only |

| Story/mission modification | High | Optional sandbox systems; avoid official continuity changes |

| Online interference | Critical | Story Mode only; no online features |

| SHVDN/game-build mismatch | High | Record exact build and use compatible runtime |

| Mixing SHVDN forks | High | Use one distribution consistently |

| Nightly API instability | Medium | Compile against stable 3.6.0 APIs |

| Entity bloat | High | Ownership registry and cleanup tests |

| Save corruption | High | Versioning, backup, atomic replacement |

| Duplicate handlers after reload | High | Abort unsubscribe and reload tests |

| Fake zone detection | High | No arbitrary radius treated as a real interior test |

| Overpromising advanced mechanics | Medium | Research gates before commitment |

| Unsupported “tested” claims | Medium | Separate source review, compilation, and runtime evidence |



\---



\# 17. Immediate next actions



Do these in order:



1\. \*\*Create the repository and docs skeleton.\*\*

2\. \*\*Record your GTA V edition and exact executable version.\*\*

3\. \*\*Create the .NET Framework 4.8 x64 class library.\*\*

4\. \*\*Add SHVDN3 3.6.0 and LemonUI.SHVDN3 2.2.0 compile references.\*\*

5\. \*\*Install a mutually compatible ScriptHookV/SHVDN runtime.\*\*

6\. \*\*Install the matching LemonUI SHVDN3 runtime files.\*\*

7\. \*\*Add `WeaponAndZoneUnlocker.cs`.\*\*

8\. \*\*Build.\*\*

9\. \*\*If compilation fails, capture the exact error code, line, and referenced package versions.\*\*

10\. \*\*Run the smoke tests before adding any more features.\*\*



After Module 1 passes, the next engineering package should be:



```text

PersistenceService.cs

EconomyService.cs

PropertyOwnershipModule.cs

ClassicSandbox.ini / profile schema

Manual save/reload test suite

```



That builds the actual classic sandbox loop rather than jumping prematurely into custom physics or an unverifiable GTA VI port.



\*\*Bottom line:\*\* the project is viable as a GTA V single-player overhaul with portable gameplay logic. The evidence does not support treating the PortWatch race as an imminent official PC modding platform. The corrected plan keeps that boundary explicit—and the first module now reflects real APIs instead of fabricated ones.



