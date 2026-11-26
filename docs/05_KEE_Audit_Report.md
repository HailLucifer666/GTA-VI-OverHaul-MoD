# `/kee` Adversarial Audit Report — PRD, MVP, Tech Stack & Module 1

> [!CAUTION]
> **Audit Verdict: HOLD — DO NOT SHIP OR BUILD AS WRITTEN.**
> Running Scope Gates A–F and `/kee` defect classes against our own four artifacts (`01_PRD`, `02_MVP`, `03_Tech_Stack`, `04_VS_Setup_and_Module1_Code`) exposes **3 CRITICAL** and **4 IMPORTANT** structural defects. The largest is a **Gate A premise collapse**: targeting GTA VI PC directly invalidates the SHVDN3/GTA V runtime assumption, and our "100% compile-ready" claim on Module 1 was uncompiled and therefore a **Class 14 False Green**.

---

## 1. Scope Gates (A–F) Ruling

| Gate | Verdict | Evidence / Finding |
| :--- | :---: | :--- |
| **A. Measure the Premise** | **FAIL** | The plan assumed GTA V PC prototyping, whereas the user intends to target **GTA VI PC directly** (unreleased as of `2026-11-26`; Script Hook VI and LemonUI-VI do not exist). Furthermore, `WeaponAndZoneUnlocker.cs` was claimed "compile-ready" without being compiled against an actual `ScriptHookVDotNet3.dll` binary. |
| **B. Derive Set from Reason** | **FAIL** | Tier 3 features (#4 Submarines, #7 Furnishing, #11 Tennis, #16 Skateboarding, #21 Prone, #22 Dual-Wield) require 3D model/animation/physics tooling (Blender + RAGE 9 asset importer), which was completely omitted from `03_Tech_Stack`. |
| **C. State Matrix** | **FAIL** | `WeaponAndZoneUnlocker.cs` missed 4 live game states: (1) Cutscenes/Pause menus, (2) Protagonist switch (Jason↔Lucia / Michael↔Franklin↔Trevor), (3) `Game.GameTime` reset on save-load freezing `OnSlowTick`, (4) Melee/special weapons with `MaxAmmo <= 0`. |
| **D. No Hand-Kept Lists** | **FAIL** | `GameNatives.ClassicPocketArsenalWeapons` is a hand-typed list of 55 `WeaponHash` enums, unverified against the actual SHVDN3 enum surface (risk of compile errors on mismatched enum names like `SwitchBlade` vs `Switchblade` or missing DLC weapons). |
| **E. Ask the Right Question** | **FAIL** | "Compiles against SHVDN3" is a proxy, not the question we care about ("does core mod logic work independently of GTA V and survive a GTA VI adapter swap?"). |
| **F. What Became Unreachable** | **PASS (N/A)** | No upstream guard yet; must be enforced once `ClassicGTA.Core` unit tests are written. |

---

## 2. Numbered Findings

### 1. **CRITICAL** — Premise Collapse & Wrong-Target Evidence (`01_PRD`, `02_MVP`, `03_Tech_Stack`) — **CONFIRMED**
- **Defect Class:** Gate A (Measure the Premise) + Non-Negotiable (*"Evidence from the wrong target is not evidence"*).
- **Mechanism:** Targeting GTA VI PC directly means there is **no runtime, no Script Hook VI, no NativeDB, and no LemonUI-VI** available today (`2026-11-26`). Building directly against GTA V's `ScriptHookVDotNet3.dll` and `.NET Framework 4.8` produces code that cannot be tested without GTA V and may be completely incompatible if GTA VI's script hook targets C++ ASI or modern `.NET 8/9` instead of `.NET Framework 4.8`.
- **Fix:** Split the solution into two strictly decoupled layers:
  1. **`ClassicGTA.Core` (`.NET Standard 2.0` — zero game DLL dependencies):** Pure C# domain engines (Stock Market, Blackjack/Poker, Taxi 5-state machine, Property/Business economy, RPG XP curves, Rampage timer/wave/medal logic, `IGameBridge` interface, atomic JSON persistence) with **100% runnable xUnit tests today** via `dotnet test`.
  2. **`ClassicGTA.Adapter.*` (Thin Engine Bridge):** `Adapter.GTAVI` written only once Script Hook VI ships (or `Adapter.GTAV` only if you choose to test in GTA V).

### 2. **CRITICAL** — Abstraction Layer Overclaim (`04_VS_Setup_and_Module1_Code.md:L112-L420`) — **CONFIRMED**
- **Defect Class:** Class 10 (The Confident Comment That Is Wrong).
- **Mechanism:** `GameNatives.cs` claims *"swap only these hashes and weapon tables"* to port to GTA VI. In reality, `WeaponAndZoneUnlocker.cs` directly couples to `GTA.Script`, `GTA.Game`, `GTA.Ped`, `GTA.WeaponHash`, `GTA.UI.Notification`, and `LemonUI.*` across dozens of lines. Swapping `GameNatives.cs` leaves Module 1 completely uncompilable on GTA VI.
- **Fix:** Move all module state and decisions into `ClassicGTA.Core` talking strictly to `IGameBridge` and `IMenuBridge` interfaces. No `using GTA;` or `using LemonUI;` inside any module's core logic.

### 3. **CRITICAL** — Uncompiled "Compile-Ready" Claim & Hand-Typed Enum List (`04_VS_Setup_and_Module1_Code.md:L140-L235`) — **CONFIRMED**
- **Defect Class:** Class 14 (False Green) + Gate D (Hand-Kept List) + Prime Rule (no proof without execution).
- **Mechanism:** `ClassicPocketArsenalWeapons` hand-lists 55 `WeaponHash` members without compiling against `ScriptHookVDotNet3.dll`. Even a single casing mismatch (e.g., `SwitchBlade` vs `Switchblade`, `KnuckleDuster` vs `Knuckle`) breaks the build immediately for a beginner.
- **Fix:** Never claim code is "compile-ready" without running `dotnet build` / `csc` against the real target binaries. For engine-agnostic core code, compile and run `dotnet test` locally inside `E:\Classic GTA overhaul mod for PC\`.

### 4. **IMPORTANT** — Four Missing Runtime States in Module 1 (`04_VS_Setup_and_Module1_Code.md:L310-L390`) — **PLAUSIBLE → CONFIRMED BY INSPECTION**
- **Defect Class:** Gate C (State Matrix) + Class 1 (TOCTOU).
- **Mechanism:**
  1. `OnFastTick()` enables `Control.Attack` during cutscenes and pause menus (`Game.IsCutsceneActive || Game.IsPaused` was never checked).
  2. `now - _lastSlowTickMs >= 250` breaks if `Game.GameTime` resets on save-load (`now < _lastSlowTickMs` freezes `OnSlowTick` indefinitely).
  3. Protagonist switching changes `Game.Player.Character.Handle`; `_lastEquippedWeapon` leaks across characters.
  4. Post-build event hardcoded `C:\Program Files (x86)\Steam\...`, violating the user's strict **`E:\Classic GTA overhaul mod for PC\` only (no `C:\`)** workspace rule.
- **Fix:** Guard `OnFastTick` with `!IsInCutscene && !IsPaused`; guard timer wrap with `now < _lastSlowTickMs || now - _lastSlowTickMs >= 250`; track `_lastPlayerHandle` to reset state on character switch; keep all tools/packages/builds strictly inside `E:\Classic GTA overhaul mod for PC\`.

### 5. **IMPORTANT** — Flat `.ini` Persistence Will Break on Tier 2/3 Data (`01_PRD`, `03_Tech_Stack`) — **CONFIRMED**
- **Defect Class:** Class 20 (Teardown & Write Races) + Gate B (Under-Scoped Design).
- **Mechanism:** `GTA.ScriptSettings` (`.ini`) is flat key-value text with no atomic write guarantees. Storing 50+ placed furniture transforms (#7), stock transaction histories (#5), and replay medals (#17) in a single `.ini` risks partial writes on crash and painful parsing.
- **Fix:** Use structured JSON (`ClassicGTA_Save.json`) written atomically via unique temp file + `File.Replace` inside `ClassicGTA.Core`, keeping `.ini` strictly for simple user-editable toggles/hotkeys.

### 6. **IMPORTANT** — Missing 3D/Animation Pipeline for Tier 3 (`03_Tech_Stack`) — **CONFIRMED**
- **Defect Class:** Gate B (Under-Scoped Scope).
- **Mechanism:** Features #4 (Submarines if absent from GTA VI base game), #16 (Skateboarding), #21 (Prone Crawling), and #22 (Dual-Wielding) cannot be built with C# scripts alone if native models/animations don't exist in GTA VI.
- **Fix:** Explicitly mark which of the 26 features are **100% Scriptable (C# Only)** vs. **Asset/Animation-Dependent (Blocked until RAGE 9 model/anim tools exist)**.

### 7. **ADVISORY** — Honest Scope Labeling for Substituted Features (`01_PRD`) — **CONFIRMED**
- **Defect Class:** Class 16 (Vocabulary Mismatch / Overclaiming).
- **Mechanism:** Features #6 (Web Browser → menu), #13 (Movie Theaters → native TV), and #17 (Story Mission Replay → custom sandbox contracts) are *substitutes*, not 1:1 restorations.
- **Fix:** Tag them explicitly in the PRD as **`[SANDBOX SUBSTITUTE]`** so expectations are 100% honest.

---

## 3. Triage Summary

| Bucket | Items |
| :--- | :--- |
| **FIX IMMEDIATELY** | #1 (Decouple `ClassicGTA.Core` from game DLLs so real unit tests run today), #2 (Remove `GTA`/`LemonUI` coupling from module logic), #4 (Fix workspace path to `E:\` only + state-matrix bugs) |
| **FIX BEFORE CODING MODULE 1** | #3 (Real `dotnet build` + `dotnet test` proof — zero uncompiled claims), #5 (Atomic JSON save layer + `.ini` config split) |
| **UPDATE IN PRD** | #6 (Flag Asset/Anim-dependent Tier 3 features honestly), #7 (Tag `#6, #13, #17` as `[SANDBOX SUBSTITUTE]`) |
