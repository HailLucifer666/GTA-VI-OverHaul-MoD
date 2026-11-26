



Below is the complete project engineering package for your Classic GTA Overhaul Mod. It contains your Product Requirements Document (PRD), Minimum Viable Product (MVP) scope, Zero-Cost Tech Stack, Software Architecture, and a step-by-step execution guide—ready to save into your project repository or feed directly into your AI coding workflow.



Document 1: Product Requirements Document (PRD)

1.1 Product Vision

Classic GTA Overhaul is a modular, open-source single-player modification for Grand Theft Auto (prototyped in GTA V on PC and designed for seamless porting to GTA VI on PC). It restores classic sandbox mechanics, side activities, economy systems, and combat freedoms removed from modern entries, giving players full open-world agency.

1.2 Target Audience

• Single-player GTA fans who miss classic sandbox mechanics (property ownership, emergency side jobs, unrestricted combat, and pocket arsenals).

• PC modders seeking a clean, modular, configuration-driven overhaul that does not break story progression.

1.3 Core Principles

• Zero-Cost Development: Built exclusively with free, open-source tools and community libraries.

• Portability First: All game-specific memory calls and engine hashes are isolated inside an abstraction layer (GameBridge.cs) so porting from GTA V to GTA VI requires updating only one file.

• Non-Destructive: Operates strictly in offline Story Mode via runtime memory scripting without modifying base game .rpf archives.

1.4 Functional Requirements (Prioritized Backlog)

Priority	Module ID	Feature Name	Functional Requirement

P0 (MVP)	MOD-01	Zone \& Weapon Unlocker	Overrides interior and safe-zone weapon locks so firearms remain usable in clubs and around non-hostile NPCs.

P0 (MVP)	MOD-02	Pocket Arsenal	Hotkey toggle (F10) granting all available weapons with max ammo, bypassing trunk/inventory carry limits.

P0 (MVP)	MOD-03	Emergency Dispatch	Menu/hotkey (F9) to summon Police, Ambulance (heals player), or Fire Truck (extinguishes fires) to player GPS coordinates.

P1 (V1.1)	MOD-04	Classic Taxi Jobs	State-driven passenger pickup and drop-off system inside any cab, awarding distance-based cash payouts.

P1 (V1.1)	MOD-05	Property \& Vehicle Market	World markers + LemonUI menus to purchase safehouses, passive-income businesses, aircraft, boats, and submarines, persisted via .ini.

P2 (V1.2)	MOD-06	Vigilante \& Rampages	Randomized criminal pursuit missions in police vehicles and timed weapon-specific kill challenges.

P2 (V1.2)	MOD-07	Classic Camera \& Stats	Free-roam first-person perspective toggle and persistent RPG skill bars (Stamina, Driving, Flying, Shooting).

P3 (V2.0)	MOD-08	In-Game Stock Exchange	Simulated stock ticker with dynamic market swings and mission-linked price manipulation saved to .ini.



Document 2: Minimum Viable Product (MVP) Specification (v0.1.0)

To avoid burnout and get a playable mod running within 7 days, the MVP is strictly scoped to 3 core modules plus a master configuration file.

MVP Scope

1\. Master Menu (F8): A native-style LemonUI menu allowing the player to toggle each MVP module ON/OFF in real time.

2\. MOD-01 Safe-Zone Weapon Unlocker: Automatically enables weapon wheel access and firing controls inside interiors (strip clubs, bars, safehouses) and clears CanBeDamaged = false flags on non-story ambient crowd NPCs.

3\. MOD-02 Pocket Arsenal: Pressing F10 (or toggling via menu) equips all standard weapons with full ammo and prevents the engine from stripping heavy weapons when exiting vehicles.

4\. MOD-03 Emergency Services Dispatch: Selecting Police, Paramedics, or Fire Department in the F8 menu spawns the corresponding emergency vehicle 120–150m away on a valid road node and drives it to the player's location.

5\. Persistence (ClassicGTA.ini): Saves user hotkey bindings and module toggle states across game sessions.

MVP Definition of Done (Acceptance Criteria)

• Compiles with 0 errors in Visual Studio Community (.NET Framework 4.8).

• Hot-reloads cleanly in-game using the Insert key without crashing ScriptHookVDotNet.

• Maintains a locked frame rate with < 0.5ms script execution time per tick.

• Cleans up all spawned emergency vehicles and blips when dismissed or when the script unloads.



Document 3: Zero-Cost Tech Stack \& Architecture

3.1 Approved $0 Tech Stack

Layer	Tool / Library	Cost	Purpose

IDE \& Compiler	Visual Studio Community 2022+	Free	Writing and compiling C# code into .dll assemblies

Runtime Framework	.NET Framework 4.8	Free	Required runtime target for RAGE .NET script hooks

Core C++ Hook	ScriptHookV (Alexander Blade)	Free	Injects native C++ engine calls into offline Story Mode

C# Scripting Bridge	ScriptHookVDotNet3 (SHVDN3)	Free	Exposes RAGE engine functions to C# scripts + live Insert reload

UI Library	LemonUI.SHVDN3	Free	Renders native Rockstar-style interaction menus

Version Control	Git + GitHub (Private Repo)	Free	Tracks code versions and stores .ini configs

AI Co-Pilot	Claude / ChatGPT / Gemini (Free Tiers)	Free	Generates C# modules and debugs compiler/log errors

3.2 Folder \& Codebase Architecture







Document 4: Step-by-Step Execution Plan

Step 1: Environment \& Safety Setup (Day 1)

1\. Create a Safe Modding Copy: Copy your legitimate GTA V installation folder to a separate directory (e.g., D:\\Games\\GTAV\_Modding\\) so your primary install remains untouched for online play.

2\. Disable Anti-Cheat for Story Mode: Open the Rockstar Games Launcher -> Settings, and uncheck BattlEye (required for ScriptHookV to load in single-player).

3\. Install Base Hooks:

• Download ScriptHookV and extract ScriptHookV.dll and dinput8.dll into D:\\Games\\GTAV\_Modding\\.

• Download ScriptHookVDotNet (v3.6.0+) and extract ScriptHookVDotNet.asi, ScriptHookVDotNet2.dll, and ScriptHookVDotNet3.dll into the same root folder.

• Create a new subfolder named scripts inside D:\\Games\\GTAV\_Modding\\.

• Download LemonUI and place LemonUI.SHVDN3.dll inside D:\\Games\\GTAV\_Modding\\scripts\\.

Step 2: Visual Studio Project Configuration (Day 1)

1\. Open Visual Studio Community -> Create a new project -> Select Class Library (.NET Framework) (C#).

2\. Name the project ClassicGTAOverhaul and set Framework to .NET Framework 4.8.

3\. In Solution Explorer, right-click References -> Add Reference -> Browse, and select:

• ScriptHookVDotNet3.dll

• LemonUI.SHVDN3.dll

• Also check System.Windows.Forms under Assemblies -> Framework.

4\. Right-click your project -> Properties -> Build Events -> Post-build event command line, and paste:

bat

copy /Y "$(TargetPath)" "D:\\Games\\GTAV\_Modding\\scripts\\"



Use code with caution.



(Update the path to match your exact modding folder. Now every time you press Ctrl + Shift + B, Visual Studio automatically updates the mod inside your game folder.)

Step 3: Vibe-Coding the Core \& MVP Modules (Days 2–5)

Feed the architecture above to your AI assistant one file at a time in this exact order:

1\. Prompt 1 (ConfigManager.cs \& IModModule.cs): Ask for a clean module interface and an INI reader using SHVDN3's built-in ScriptSettings.Load("scripts\\\\ClassicGTA.ini").

2\. Prompt 2 (GameBridge.cs): Ask for a static helper class wrapping Native calls (SET\_CURRENT\_PED\_WEAPON, ENABLE\_CONTROL\_ACTION, GET\_CLOSEST\_VEHICLE\_NODE) so game-specific hashes never clutter your gameplay logic.

3\. Prompt 3 (ZoneUnlockerModule.cs): Build MOD-01 using a 250ms throttled check for interior locks and a 0ms frame check for firing control overrides.

4\. Prompt 4 (PocketArsenalModule.cs): Build MOD-02 looping through Enum.GetValues(typeof(WeaponHash)) to equip all valid weapons with max ammo.

5\. Prompt 5 (DispatchModule.cs \& MainController.cs): Build MOD-03 and connect all three modules to a LemonUI NativeMenu bound to F8.

Step 4: Testing, Profiling \& Hardening (Days 6–7)

1\. Launch GTAV.exe in Story Mode.

2\. Press F8 to verify the Classic GTA Overhaul menu renders cleanly.

3\. Walk into the Vanilla Unicorn strip club and verify you can draw and fire weapons (MOD-01).

4\. Press F10 and verify all weapons appear on your wheel (MOD-02).

5\. Trigger an Ambulance dispatch via the menu, verify it drives to your location, heals you, and cleans up its map blip afterward (MOD-03).

6\. Inspect ScriptHookVDotNet.log in your game root folder to verify zero exceptions or memory warnings.

Step 5: Porting to GTA VI PC (Launch Protocol)

1\. Install ScriptHookVI and ScriptHookVIDotNet into your GTA VI PC scripts/ directory upon community release.

2\. Open GameBridge.cs—the only file containing engine-specific hashes—and replace the GTA V Native hashes and vehicle/weapon enum names with the newly dumped GTA VI NativeDB values.

3\. Recompile (Ctrl + Shift + B) and test module-by-module.



Document 5: Starter ClassicGTA.ini Configuration Template

Save this file directly inside your scripts/ folder alongside your compiled .dll:

ini

\[General]

MenuKey=F8

ShowStartupNotification=true

\[ZoneUnlocker]

Enabled=true

UnlockInteriors=true

UnlockAmbientNPCs=true

\[PocketArsenal]

Enabled=true

GiveAllWeaponsKey=F10

InfiniteAmmo=false

\[EmergencyDispatch]

Enabled=true

DispatchDistanceMeters=140.0

Use code with caution.







