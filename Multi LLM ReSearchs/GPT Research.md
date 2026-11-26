Classic GTA Overhaul — Complete Engineering \& Production Blueprint

Project planning initiated

GTA V prototype → GTA VI PC

Modular architecture

The objective is to build a comprehensive Classic GTA Overhaul: a single-player mod that brings back the freedom, activities, progression systems and sandbox mechanics that made older GTA games memorable.

I would approach this as a real game-engineering project, not a collection of disconnected scripts.

The central idea:

&#x09;• GTA V is our development and validation environment.

&#x09;• GTA VI is our eventual target platform.

&#x09;• Every feature is an independent module.

&#x09;• Every module has requirements, acceptance criteria, tests and a documented compatibility boundary.

&#x09;• AI coding agents can implement modules, but nothing enters the stable release without compilation, testing and adversarial review.

One important architectural decision: we should not assume that GTA V code, native hashes, entity models, coordinates or ScriptHookVDotNet APIs will transfer to GTA VI. We will build a game-independent gameplay layer and separate game-specific adapters. A GTA VI adapter will only be implemented when its legitimate PC modding interfaces become available.

Also, your 26-feature list will be our proposed restoration backlog, not an unquestioned list of confirmed GTA VI omissions. Each feature will have a verification status so we don't build around inaccurate claims.

1\. Current technical reality — November 26, 2026

There is one important change to the project assumptions.

GTA VI platform status

Console launch

November 19, 2026

PC release

Not announced

GTA VI PC scripting SDK

Not established

GTA VI Script Hook

Not established

Rockstar's official launch announcement confirms PS5 and Xbox Series X|S availability, not a PC release. 



Rockstar Games+2

This means we should retain your GTA V prototype strategy, but add a formal GTA VI Readiness Gate.

We can build and test the gameplay systems now. We cannot honestly claim that a GTA VI-compatible implementation exists until a legitimate PC build and compatible scripting interface are available.

There is another important legal consideration. Rockstar's current single-player mod guidance and creator policies must be reviewed before distribution. We should design for offline, non-commercial, third-party-IP-free modding, and avoid multiplayer functionality, game asset redistribution and story/map expansions. 



Rockstar Games Customer Support+2

2\. The engineering philosophy

Think of this as building a small game framework inside GTA V, rather than writing 26 independent cheats.

Classic GTA Overhaul architecture

Classic GTA Core

Configuration · Save data · Events · Economy · Module lifecycle

Gameplay systems

Taxi, vigilante, rampages

World systems

Properties, businesses, vehicles

Player systems

Inventory, stats, camera

Social systems

Pets, hangouts, services

Economy systems

Cash, stocks, income

Activity systems

Darts, bowling, casino

Game Adapter Interface

Player · World · Vehicles · Weapons · UI · Persistence

GTA V Adapter

Build now

SHVDN3 + LemonUI

GTA VI Adapter

Future gate

Await legitimate PC tooling

Critical rule: gameplay modules must never directly call game-specific native hashes. They call the adapter. This is what makes a future port manageable.

3\. Complete feature strategy

I recommend splitting the 26 requested features into six product systems and implementing them in dependency order.

System	Features	Priority

Inventory \& combat	Weapon arsenal, interior weapon permissions, prone, dual wield	P0

Economy \& ownership	Properties, businesses, vehicles, submarines, stocks	P1

Emergency \& jobs	Taxi, vigilante, ambulance, firefighter, rampages	P1

Player progression	Skills, first-person, mission replay	P2

Recreation	Darts, bowling, tennis, casino, cinema, amusement rides, skateboarding	P2

Social \& world	Hangouts, pets, emergency calls, relationship interactions	P3

Dependency-first implementation order

The order matters more than the difficulty ranking in the original handoff.

&#x09;1. Foundation and developer tooling

Module loader, configuration, logging, event bus, persistence, game adapter.

&#x09;2. Inventory and player controls

Pocket arsenal, weapon restrictions, stats and camera systems.

&#x09;3. World interaction framework

Markers, interaction prompts, GPS routes, entity lifecycle management.

&#x09;4. Economy and persistence

Cash ledger, ownership registry, passive income, stock simulation.

&#x09;5. Activities and jobs

Taxi, emergency dispatch, vigilante and rampage state machines.

&#x09;6. Advanced world systems

Properties, vehicles, pets, hangouts, mini-games and custom mechanics.

&#x09;7. Integration and release

Compatibility testing, performance profiling, save migration and packaging.

4\. Technical stack — $0 development budget

These are the recommended tools, with GTA V-specific dependencies kept separate from the core architecture.

Layer	Technology	Purpose

Language	C#	Gameplay modules

IDE	Visual Studio Community 2022	Development

Runtime	.NET Framework 4.8	GTA V scripts

Game API	ScriptHookVDotNet 3	GTA V integration

UI	LemonUI.SHVDN3 2.2.0	Native-style menus

Configuration	INI	User settings

Persistence	JSON	Save data and ownership

Version control	Git + GitHub	Source control

Testing	NUnit	Core logic tests

Static analysis	Roslyn analyzers	Code quality

Documentation	Markdown	Engineering handoff

CI	GitHub Actions	Automated builds and tests

SHVDN's official repository confirms the .NET Framework 4.8 requirement, the matching API assemblies and the need to keep its runtime files version-compatible. LemonUI's NuGet package confirms SHVDN3 and .NET Framework 4.8 compatibility. 



GitHub+1

Official technical references:

&#x09;• ScriptHookVDotNet repository

&#x09;• SHVDN user guides

&#x09;• LemonUI NuGet

&#x09;• LemonUI developer guide

&#x09;• Script Hook V

5\. Repository structure

This is the structure I want us to maintain from the beginning.

ClassicGTAOverhaul/

│

├── README.md

├── CHANGELOG.md

├── LICENSE-NOTICE.md

│

├── docs/

│   ├── PRD.md

│   ├── MVP.md

│   ├── ARCHITECTURE.md

│   ├── TECH\_STACK.md

│   ├── ROADMAP.md

│   ├── FEATURE\_BACKLOG.md

│   ├── TEST\_PLAN.md

│   ├── RED\_TEAM\_PROTOCOL.md

│   ├── SECURITY\_AND\_LEGAL.md

│   ├── ADR/

│   └── modules/

│

├── src/

│   ├── ClassicGTA.Core/

│   ├── ClassicGTA.Contracts/

│   ├── ClassicGTA.Infrastructure/

│   ├── ClassicGTA.GTA5/

│   ├── ClassicGTA.UI/

│   └── ClassicGTA.Modules/

│       ├── Inventory/

│       ├── Economy/

│       ├── Properties/

│       ├── Vehicles/

│       ├── Emergency/

│       ├── Taxi/

│       ├── Activities/

│       ├── Progression/

│       └── Social/

│

├── tests/

│   ├── Core.Tests/

│   ├── Economy.Tests/

│   ├── Module.Tests/

│   └── Integration.Tests/

│

├── config/

│   └── ClassicGTA.ini

│

├── saves/

│

└── tools/

&#x20;   ├── Build.ps1

&#x20;   └── Package.ps1

The key separation is:

&#x09;• Core: rules and gameplay logic.

&#x09;• Contracts: interfaces all modules must implement.

&#x09;• Infrastructure: persistence, logging and configuration.

&#x09;• GTA5: actual SHVDN3 game integration.

&#x09;• Modules: individual features.

&#x09;• GTA6: a future adapter, not an assumed working implementation.

6\. Five-minute Visual Studio setup

Recommended starting configuration

Setting	Exact value

Project type	Class Library (.NET Framework)

Language	C#

Target framework	.NET Framework 4.8

Platform	x64

Project name	ClassicGTA.GTA5

SHVDN API	ScriptHookVDotNet3

UI package	LemonUI.SHVDN3 2.2.0

Deployment	GTA V/scripts/

Installation checklist

&#x09;1. Install Visual Studio Community 2022 with the .NET desktop development workload.

&#x09;2. Create a C# Class Library targeting .NET Framework 4.8.

&#x09;3. Set platform target to x64.

&#x09;4. Add ScriptHookVDotNet3.dll from your installed SHVDN distribution as a reference. Set Copy Local to False.

&#x09;5. Install LemonUI.SHVDN3 version 2.2.0 through NuGet when we start building menus. It is not required for Module 01.

&#x09;6. Add the WeaponAndZoneUnlocker.cs source file.

&#x09;7. Configure the post-build copy to your GTA V scripts folder.

&#x09;8. Build Release.

&#x09;9. Launch GTA V Story Mode and check the SHVDN log.

Important version warning: SHVDN's current repository documents compatibility issues with certain stable releases on newer GTA V builds. Pin the compatible runtime and use matching SHVDN files rather than blindly downloading the latest individual DLLs. 



GitHub+2

Post-build command

if not exist "$(GTA5\_DIR)\\scripts" mkdir "$(GTA5\_DIR)\\scripts"

copy /Y "$(TargetPath)" "$(GTA5\_DIR)\\scripts\\"

Set GTA5\_DIR to your GTA V installation directory.

7\. Council review — Module 01

The Architect

Design approved

&#x09;• Independent inventory module.

&#x09;• No dependency on the economy or job systems.

&#x09;• GTA V API isolated from future gameplay contracts.

&#x09;• Explicit enable/disable lifecycle.

&#x09;• No unnecessary entity spawning.

The RAGE Engine Specialist

API signatures checked

The implementation uses documented SHVDN3 concepts:

&#x09;• Game.Player.Character

&#x09;• WeaponHash

&#x09;• Weapons.IsWeaponValid()

&#x09;• Weapons.HasWeapon()

&#x09;• Weapons.Give()

&#x09;• Weapons.Remove()

&#x09;• Script.KeyDown and Script.Aborted

&#x09;• Screen.ShowSubtitle()

The weapon enumeration and inventory method signatures are documented in the SHVDN3 API references. 



GitHub+2

The Red Team

One feature deferred

Findings:

&#x09;• Do not blindly force weapons in mission-restricted interiors.

&#x09;• Track only inventory additions owned by the module.

&#x09;• Avoid deleting player-owned entities.

&#x09;• Handle script reloads and player death.

&#x09;• Test invalid weapon entries and weapon ownership.

&#x09;• Do not execute heavy operations every frame.

Additional limitation: GTA V does not expose a simple ownership distinction between a weapon granted by this module and the same weapon acquired independently while the module is enabled. The first prototype therefore needs a pickup/ownership regression test before being treated as a polished release.

Module 01 deliverable

WeaponAndZoneUnlocker.cs

Pocket Arsenal implementation

Press F10 to enable or disable the expanded inventory.

Action	Result

F10 ON	Grants valid available weapons

F10 OFF	Removes tracked module additions

Existing weapons	Preserved

Notifications	Displayed on screen

Mission restrictions	Not overridden

Online functionality	None

The safe-zone/interior unlock is deliberately separated. It needs its own verified implementation and mission-safety testing. I would rather give you a smaller working module than a supposedly complete one that silently breaks missions.

8\. Your complete engineering document package

ClassicGTAOverhaul Engineering Pack

19 files · ZIP archive · Version 0.1.0

Created

Download the complete engineering package

Included documents

PRD

Product requirements

MVP

First playable release

Architecture

System design

Tech Stack

Dependencies and tools

Roadmap

Phased implementation

Feature Backlog

All restoration candidates

Test Plan

QA and release gates

Red Team

Adversarial review

Security \& Legal

Distribution safeguards

Agent Workflow

AI coding protocol

Setup Guide

Visual Studio installation

Module 01

Pocket Arsenal specification

Also included:

&#x09;• Initial GTA V C# project file.

&#x09;• Module 01 source code.

&#x09;• Starter INI configuration.

&#x09;• Architecture Decision Record.

&#x09;• Research references.

&#x09;• Project manifest.

The package is a planning and source-code handoff. It has not yet been compiled or tested against your local GTA V installation.

9\. Execution schedule and milestones

These are planning estimates, assuming AI-assisted implementation and regular testing.

Milestone	Deliverable	Estimate

M0	Environment and repository	1 day

M1	Core framework and module loader	2–4 days

M2	Pocket Arsenal	1 day

M3	World interaction framework	2–4 days

M4	Economy and persistence	3–5 days

M5	Taxi and emergency jobs	4–7 days

M6	Property and vehicle ownership	4–7 days

M7	Progression and activities	5–10 days

M8	Advanced mechanics	Research-dependent

M9	QA and stable GTA V release	3–5 days

M10	GTA VI compatibility investigation	After PC/tooling availability

Do not interpret these as guaranteed delivery dates. Advanced animations, physics and engine integration can take considerably longer.

10\. How we will work together

For every feature, use this implementation cycle:

Feature development pipeline

&#x09;1. Research

Verify the desired mechanic, APIs, game behavior and restrictions.

&#x09;2. Specification

Write the module requirements, state machine and acceptance criteria.

&#x09;3. Architecture

Identify interfaces, dependencies and persistence changes.

&#x09;4. Implementation

Generate complete C# source and tests.

&#x09;5. Red-team review

Audit APIs, lifecycle, performance, security and compatibility.

&#x09;6. Build and test

Compile, install and test in GTA V Story Mode.

&#x09;7. Integration

Merge into the main project and update documentation.

Rules for our AI coding agents

I recommend using separate agents for:

&#x09;• Architect: design and dependencies.

&#x09;• API researcher: verify SHVDN methods and game behavior.

&#x09;• Coder: implement complete modules.

&#x09;• Red Team: independently challenge the implementation.

&#x09;• QA/integrator: build, test and document.

No agent gets to declare its own code production-ready without independent verification.

11\. What I want you to do first

Let's avoid building everything simultaneously.

Phase 2 — Step 1: Environment verification

Setup progress

0/8

Install Visual Studio Community 2022 and .NET Framework 4.8 Developer Pack.

Confirm GTA V PC launches successfully in Story Mode.

Install a compatible Script Hook V and SHVDN3 release.

Extract the engineering package.

Open ClassicGTA.GTA5.csproj.

Configure GTA5\_DIR.

Build Release and inspect the build output.

Test F10 ON and F10 OFF in Story Mode.

My recommendation: Do not start the taxi or property modules yet. First establish that the project builds, loads, responds to F10 and survives script reloads. That gives us a reliable foundation for the remaining systems.

One final thing: before we eventually distribute anything, we will re-check Rockstar's live Mod Guidelines and single-player mod policy. Offline development does not automatically mean every form of distribution is permitted.

Your next step: What GTA V PC version are you currently using — Legacy or Enhanced? That determines which SHVDN compatibility setup we should pin before the first build.













GTA V development environment — exact installation and configuration

Since you own both GTA V Legacy and Enhanced, I recommend building one shared codebase and validating it against both editions.

There is an important correction to our original setup: for your dual-edition environment, I recommend Script Hook V .NET Enhanced (SHVDNE) rather than installing the original SHVDN3 runtime independently. SHVDNE is an extended implementation specifically designed to support both Legacy and Enhanced, while retaining the SHVDN3 API used by our C# scripts. 



GitHub+1

Your recommended configuration

GTA V Legacy

Target A

GTA V Enhanced

Target B

Component	Selection

IDE	Visual Studio Community 2022

Language	C#

Framework	.NET Framework 4.8

Architecture	x64

Hook	Script Hook V

.NET runtime	SHVDNE

API	SHVDN3-compatible API

UI	LemonUI.SHVDN3

Codebase	One shared codebase

Deployment	Separate GTA V folders

Step 1. Install Visual Studio Community 2022

File:Visual Studio Icon 2022.svg - Wikimedia Commons



Visual Studio Community 2022

Free C# development environment

Download Visual Studio Community

&#x09;1. Download the Community installer.

&#x09;2. Open Visual Studio Installer.

&#x09;3. Select the following workload:

.NET desktop development

Get started with WebView2 in WPF apps - Microsoft Edge Developer documentation | Microsoft Learn



learn.microsoft.com

This installs the C# desktop development tools and MSBuild components.

4\. Open Individual Components and ensure the following are selected:

&#x09;• .NET Framework 4.8 targeting pack.

&#x09;• .NET Framework 4.8 SDK, if offered.

&#x09;• MSBuild.

&#x09;• NuGet package manager.

&#x09;• Windows 10/11 SDK.

5\. Install Visual Studio.

Important: .NET Framework 4.8 is our target, even if your Windows installation already includes 4.8.1. The targeting pack is what lets Visual Studio compile against the correct framework. 



Microsoft Learn

Step 2. Install the required runtime dependencies

Inicio | Morning Value



.NET Framework 4.8

Required runtime and targeting support

Microsoft .NET Framework 4.8 download

Giải thích Microsoft Visual C++ Redistributable là gì



Microsoft Visual C++ Redistributable x64

Native runtime dependency

Download Microsoft VC++ Redistributable x64

Install the Visual C++ x64 package and restart Windows if the installers request it.

Step 3. Identify your two GTA V installation directories

We need two independent game roots.

For example, yours might look like:

C:\\Games\\GTA V Legacy\\

&#x20;   GTA5.exe

C:\\Games\\GTA V Enhanced\\

&#x20;   GTA5.exe

These are examples, not assumed paths.

To find the real paths:

&#x09;1. Open Steam or Rockstar Games Launcher.

&#x09;2. Find GTA V Legacy.

&#x09;3. Open its installation folder.

&#x09;4. Record the location containing GTA5.exe.

&#x09;5. Repeat for Enhanced.

Do not install both editions' mod runtimes into a shared directory. Each game must have its own files.

Step 4. Install Script Hook V

Script Hook V » Menyoo GTA V MODS



Script Hook V

Native GTA V scripting bridge

Official developer download page

The developer's published July 2026 release lists support for:

&#x09;• Legacy: 1.0.3889.0

&#x09;• Enhanced: 1.0.1158.13

These are the builds listed in that release, not a guarantee that every future game update will remain compatible. Check the supported patch numbers against your installed executables before installing. 



AB Software Development

Installation:

&#x09;1. Download Script Hook V from the developer's page.

&#x09;2. Extract the archive.

&#x09;3. Locate bin.

&#x09;4. Copy the required runtime files into each GTA V installation root.

Typically:

GTA5.exe

ScriptHookV.dll

dinput8.dll

5\. Repeat for the other edition using the compatible runtime.

Do not copy the Native Trainer unless you specifically want it.

Step 5. Install Script Hook V .NET Enhanced

Script Hook V .Net Enhanced 1.0.0.2-beta for GTA 5



Script Hook V .NET Enhanced (SHVDNE)

Recommended for both editions

Download official GitHub releases

The published Enhanced release 1.1.0.6, dated July 14, 2026, includes a fix for the newer Enhanced game build listed above. 



GTA5-Mods+1

Installation procedure

Download the release archive and extract it.

The runtime files to install include:

ScriptHookVDotNet.asi

ScriptHookVDotNet2.dll

ScriptHookVDotNet3.dll

ScriptHookVDotNet.ini

MinHook.x64.dll

MinHook.x64.dll is required by SHVDNE starting with version 1.1.0.0.

Copy the files into the root directory of GTA V Legacy, then repeat for GTA V Enhanced.

Your folders should look approximately like this:

GTA V Legacy

GTA V Legacy/

GTA5.exe

ScriptHookV.dll

dinput8.dll

ScriptHookVDotNet.asi

ScriptHookVDotNet2.dll

ScriptHookVDotNet3.dll

ScriptHookVDotNet.ini

MinHook.x64.dll

scripts/

GTA V Enhanced

Same runtime structure in its own installation directory.

Do not mix files from different SHVDNE releases. Its maintainers explicitly recommend using the ASI and API DLLs from the same release archive. 



GitHub+1

Enhanced-specific note: The SHVDNE distribution recommends considering DirectStorageFix for GTA V Enhanced. Treat it as an optional troubleshooting dependency, not part of our initial mod framework.

Step 6. Verify the runtime before writing code

We need to establish that the game loads the scripting runtime.

Test Enhanced first:

&#x09;1. Launch GTA V Enhanced.

&#x09;2. Enter Story Mode.

&#x09;3. Wait for the game to finish loading.

&#x09;4. Check that it does not crash.

&#x09;5. Exit normally.

&#x09;6. Inspect the SHVDN log files in the game directory.

Repeat the same test in Legacy.

Do not proceed to the C# implementation if the runtime fails in either edition.

The first milestone is:

Runtime acceptance criteria

&#x09;• Enhanced starts without a Script Hook error.

&#x09;• Legacy starts without a Script Hook error.

&#x09;• SHVDNE initializes.

&#x09;• SHVDN3 API is available.

&#x09;• No online/multiplayer interaction is involved.

Step 7. Create the actual Visual Studio project

Now we establish the codebase.

Open Visual Studio Community 2022.

Choose:

Create a new project → Class Library (.NET Framework)

Use these exact settings:

Setting	Value

Project name	ClassicGTA.GTA5

Solution name	ClassicGTAOverhaul

Language	C#

Framework	.NET Framework 4.8

Platform	x64

Output type	Class Library

Assembly name	ClassicGTA.GTA5

Our first solution should look like:

ClassicGTAOverhaul/

│

├── ClassicGTAOverhaul.sln

│

├── src/

│   ├── ClassicGTA.Contracts/

│   ├── ClassicGTA.Core/

│   ├── ClassicGTA.Infrastructure/

│   ├── ClassicGTA.GTA5/

│   ├── ClassicGTA.UI/

│   └── ClassicGTA.Modules/

│

├── tests/

│   ├── ClassicGTA.Core.Tests/

│   └── ClassicGTA.Modules.Tests/

│

├── docs/

├── config/

└── README.md

For the first build, create only ClassicGTA.GTA5. We will introduce the other projects incrementally.

Step 8. Configure the SHVDNE reference correctly

This is particularly important.

In Visual Studio:

&#x09;1. Right-click References.

&#x09;2. Select Add Reference.

&#x09;3. Browse to your installed SHVDNE runtime.

&#x09;4. Select ScriptHookVDotNet3.dll.

&#x09;5. Select the reference.

&#x09;6. Open Properties.

&#x09;7. Set Copy Local = False.

Do not add the original standard SHVDN3 runtime DLL alongside the Enhanced API DLL.

Our code targets the SHVDN3-compatible API, but the assembly reference should come from the exact SHVDNE release we install and test.

Recommended project configuration

The following is the configuration baseline for our GTA V adapter project:

<PropertyGroup>

&#x20;   <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>

&#x20;   <OutputType>Library</OutputType>

&#x20;   <PlatformTarget>x64</PlatformTarget>

&#x20;   <LangVersion>7.3</LangVersion>

&#x20;   <Prefer32Bit>false</Prefer32Bit>

</PropertyGroup>

<ItemGroup>

&#x20;   <Reference Include="ScriptHookVDotNet3">

&#x20;       <HintPath>

&#x20;           $(SHVDNE\_DIR)\\ScriptHookVDotNet3.dll

&#x20;       </HintPath>

&#x20;       <Private>False</Private>

&#x20;   </Reference>

<Reference Include="System" />

&#x20;   <Reference Include="System.Core" />

&#x20;   <Reference Include="System.Windows.Forms" />

</ItemGroup>

Create a Windows environment variable:

SHVDNE\_DIR

Its value should point to the folder containing the SHVDNE ScriptHookVDotNet3.dll.

This avoids hardcoding your game installation path into the source project.

Step 9. Configure LemonUI

LemonUI: Open Source UI Library - GTA5-Mods.com



LemonUI.SHVDN3

Menu framework for our future gameplay interfaces

LemonUI NuGet package

Use version 2.2.0.

Once the project has a working basic script:

&#x09;1. Right-click the project.

&#x09;2. Select Manage NuGet Packages.

&#x09;3. Search LemonUI.SHVDN3.

&#x09;4. Select version 2.2.0.

&#x09;5. Install.

&#x09;6. Check that the SHVDN3 API reference isn't accidentally replaced by an incompatible runtime reference.

&#x09;7. Deploy the appropriate LemonUI SHVDN3 runtime files to the scripts directory.

The package targets .NET Framework 4.8 and declares an SHVDN3 dependency. 



LemonUI.SHVDN3 2.2.0

We will not use LemonUI for Module 01. It becomes important when we build the central Classic GTA menu.

Step 10. One codebase, two deployment targets

This is how we will avoid maintaining two separate mods.

&#x20;                   CLASSIC GTA OVERHAUL

&#x20;                           |

&#x20;                    Shared C# Source

&#x20;                           |

&#x20;                 SHVDN3-compatible API

&#x20;                           |

&#x20;                 ---------------------

&#x20;                 |                   |

&#x20;            GTA V Legacy       GTA V Enhanced

&#x20;                 |                   |

&#x20;            Legacy Adapter     Enhanced Adapter

&#x20;                 |                   |

&#x20;            Legacy/scripts     Enhanced/scripts

We will maintain:

&#x09;• One Git repository.

&#x09;• One shared gameplay implementation.

&#x09;• One SHVDN3-compatible API reference.

&#x09;• Two deployment directories.

&#x09;• Separate compatibility test results.

We should not assume every native or memory-dependent feature works identically across both editions. The adapter can branch on edition-specific behavior when necessary.

Post-build deployment configuration

For now, don't automatically deploy to both installations.

First create two Visual Studio build configurations:

Configuration	Destination

Debug	No automatic deployment

ReleaseLegacy	Legacy/scripts

ReleaseEnhanced	Enhanced/scripts

Use the appropriate installation path for each configuration.

For example:

<Target Name="DeployLegacy"

&#x20;       AfterTargets="Build"

&#x20;       Condition="'$(Configuration)' == 'ReleaseLegacy'">

<MakeDir Directories="$(GTA5\_LEGACY\_DIR)\\scripts" />

<Copy SourceFiles="$(TargetPath)"

&#x20;         DestinationFolder="$(GTA5\_LEGACY\_DIR)\\scripts" />

</Target>

Create a corresponding DeployEnhanced target using GTA5\_ENHANCED\_DIR.

This is safer than copying files into both game directories on every build.

12\. First implementation sequence

After completing the installation, we will follow this exact order.

Step	Deliverable	Status

1	Install development environment	Pending

2	Install both game runtimes	Pending

3	Verify SHVDNE	Pending

4	Create Visual Studio solution	Pending

5	Configure API references	Pending

6	Create module loader	Pending

7	Build Pocket Arsenal	Pending

8	Test Legacy	Pending

9	Test Enhanced	Pending

10	Begin Core Framework expansion	Pending

13\. Our first proper engineering milestone

I would actually change our original implementation order slightly.

Before adding all 26 gameplay mechanics, let's create these five foundational components:

Classic GTA Core Framework — v0.1

01\. ModuleManager

Loads, starts, stops and isolates gameplay modules.

02\. GameAdapter

Separates game-specific operations from gameplay rules.

03\. ConfigurationManager

Loads settings and keybinds from ClassicGTA.ini.

04\. SaveManager

Handles versioned JSON data, backups and migrations.

05\. Logger

Records module errors, lifecycle events and diagnostics.

Then we build Pocket Arsenal as the first module running on this framework, rather than as a standalone script that we later have to rewrite.

14\. Your installation checklist

Installation progress

0%

Development tools

Download Visual Studio Community 2022

Install .NET desktop development workload

Install .NET Framework 4.8 targeting pack

Install VC++ Redistributable x64

GTA V dependencies

Locate Legacy installation directory

Locate Enhanced installation directory

Download compatible Script Hook V

Download SHVDNE release 1.1.0.6

Install matching runtime files into both game roots

Runtime validation

Launch Enhanced Story Mode

Launch Legacy Story Mode

Verify SHVDNE logs and initialization

Project creation

Create ClassicGTAOverhaul solution

Create ClassicGTA.GTA5 project

Configure .NET Framework 4.8 and x64

Reference SHVDNE ScriptHookVDotNet3.dll

Build the first DLL

15\. What I need from you next

Don't download everything blindly. Let's establish the two installation paths first, because they determine our build and deployment configuration.

Where are your GTA V installations located?

For example:

Legacy:

D:\\SteamLibrary\\steamapps\\common\\Grand Theft Auto V\\

Enhanced:

D:\\SteamLibrary\\steamapps\\common\\Grand Theft Auto V Enhanced\\

You can paste the two paths here, or send screenshots of the installation folders.

Once I have them, I'll give you the exact project configuration for your machine, including the corrected .csproj, environment variables, dual-edition deployment targets and the complete Core Framework implementation.

Our target is one Classic GTA Overhaul codebase that works on both GTA V editions, with GTA VI portability designed in from day one.







