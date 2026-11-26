# "Classic GTA" Overhaul — Phase 2, Step 1: Research Record + Module 1 Plan

**Status:** design only. No code written, no build attempted.
**Date:** 2026-11-26
**Target:** GTA V (PC), ScriptHookVDotNet 3, C#, .NET Framework 4.8
**Intended audience:** the project owner and any external model asked to review this plan.

**Purpose of this file.** Everything below is either marked `VERIFIED` with the source it was read from, or `UNVERIFIED` and explicitly flagged. The point of the marking is that a second model can audit the claims instead of inheriting them. Where a popular assumption turned out to be false, it is recorded in §3 so it does not get re-introduced.

---

## 1. Status and blockers

### 1.1 No GTA V installation on this machine

Reconnaissance across C:, D:, E: and F: found no `GTA5.exe`:

| Check | Result |
|---|---|
| `C:\Program Files\Rockstar Games\Grand Theft Auto V` | absent (parent folder exists but is empty) |
| Steam libraries (`E:\Steam\steam`, `F:\SteamLibrary`) | 67 folders, app id 271590 (GTA V) not installed |
| Epic manifests | only GOG GALAXY |
| `F:\EPIC\GTAV` | **Rockstar Launcher only** — `Launcher.exe` 1.0.106.2879, no game archives, no `GTA5.exe` |

Consequence: **no in-game verification is possible today.** Any claim that a gameplay feature "works" before this is resolved is a guess. The plan therefore separates *provable offline* (compiles, logic tests) from *only provable in-game* (does the interior unlock fire).

### 1.2 Toolchain is half-installed

| Component | State |
|---|---|
| MSBuild | present: `C:\Program Files\Microsoft Visual Studio\18\Insiders\MSBuild\Current\Bin\MSBuild.exe`, also `F:\VS 2026\...` and `F:\VS C++\...` |
| .NET Framework 4.8 **targeting pack** | **missing** — reference assemblies only exist for v3.0 and v3.5 |
| .NET Framework 4.8 **runtime** | present — `C:\Windows\Microsoft.NET\Framework64\v4.0.30319` |
| .NET SDK | **missing** — `C:\Program Files\dotnet` has runtimes 6.0.36/7.0.20/8.0.34/9.0.23 but no `sdk` folder |
| NuGet cache | **missing** — `C:\Users\acarg\.nuget` does not exist |

Two concrete consequences:

1. `dotnet build` is not available. Builds must go through MSBuild.
2. An SDK-style `.csproj` (`<Project Sdk="Microsoft.NET.Sdk">`) cannot be built without an SDK. **Use a classic (non-SDK) csproj** so MSBuild alone can build it.
3. net48 cannot compile without reference assemblies. Either install the targeting pack via the VS Installer, or add `Microsoft.NETFramework.ReferenceAssemblies` 1.0.3 (`PrivateAssets=all`), with `nuget.config` redirecting the package cache into `.packages\` inside the project so nothing is written to `C:\Users\...`.

### 1.3 Deliberate exclusion

`E:\GTA VI BUILD Help\` was **not inspected**. The project rule is zero interaction with leaked console builds, so that folder is treated as off-limits rather than interesting.

---

## 2. Verified API surface

All of the following was read from ScriptHookVDotNet source at tag `v3.6.0` (the version of the official NuGet package), or `main` where explicitly noted. Module 1 is designed to use **only** the members in this table.

| Area | Member | Notes |
|---|---|---|
| Base class | `GTA.Script` | `public abstract class Script`; events `Tick`, `Aborted` (`EventHandler`), `KeyDown`, `KeyUp` (`WinForms.KeyEventHandler`); `protected int Interval`; `public static void Wait(int ms)`; `public static void Yield()` |
| Config | `GTA.ScriptSettings` | `Load(string)`, `GetValue<T>(string section, string name, T defaultValue)`, `SetValue<T>` |
| Player | `Game.Player.Character` | type `Ped`. There is no `.Ped` property |
| Ped | `Exists()`, `IsAlive`, `IsOnFoot`, `IsInVehicle()`, `CurrentVehicle`, `IsShooting` | `IsAlive`/`IsDead`/`MarkAsNoLongerNeeded()` are inherited from `Entity` |
| Weapons | `Ped.Weapons` → `WeaponCollection` | `public WeaponCollection Weapons => _weapons ?? (_weapons = new WeaponCollection(this));` |
| Weapons | `Give(WeaponHash, int ammoCount, bool equipNow, bool isAmmoLoaded)` → `Weapon` | returns `Weapon`, not `void` |
| Weapons | `Select(WeaponHash, bool equipNow)` → `bool`, `Current`, `BestWeapon`, `HasWeapon(WeaponHash)`, `IsWeaponValid(WeaponHash)`, `GetAllWeaponHashes()` | `SelectBest()` does **not** exist; `Current` returns `Weapon`, not `WeaponHash` |
| Weapon instance | `Hash`, `Ammo`, `AmmoInClip`, `MaxAmmo`, `InfiniteAmmo` (set-only), `InfiniteAmmoClip` (set-only) | `public sealed class Weapon`; lets us read the real ammo cap instead of hardcoding 9999 |
| Timing | `Game.GameTime` → `int` | `Function.Call<int>(Hash.GET_GAME_TIMER)`; use for throttling, never `Script.Wait` inside `Tick` |
| WeaponHash | `public enum WeaponHash : uint` in `GTA` | explicit hex values; `VehicleWeaponHash` is a **separate** enum |
| Controls | `Game.EnableControlThisFrame(Control)`, `DisableControlThisFrame(Control)`, `IsControlPressed`, `IsControlJustPressed`, `IsControlJustReleased`, `IsControlEnabled` | present and **not obsolete** in v3.6.0 |
| Control enum | `Attack`, `Attack2`, `Aim`, `AccurateAim`, `SelectWeapon`, `NextWeapon`, `PrevWeapon`, `SelectNextWeapon`, `SelectPrevWeapon` | namespace `GTA`, plain `int` enum, not `[Flags]` |
| Game state | `IsCutsceneActive`, `IsLoading`, `IsPaused`, `IsMissionActive` | backed by `IS_CUTSCENE_ACTIVE`, `GET_IS_LOADING_SCREEN_ACTIVE`, `IS_PAUSE_MENU_ACTIVE` |
| Notification | `GTA.UI.Notification.Show(string message, bool blinking = false)` → `int` | **`PostTicker` does not exist in v3.6.0** |
| Subtitles | `GTA.UI.Screen.ShowSubtitle(string, int duration = 2500)`, `ShowHelpText(string, int = -1, bool = true, bool = false)` | |
| World | `World.CreateVehicle(Model, Vector3, float heading = 0f)`, `CreatePed`, `CreateBlip(Vector3)` | needed from Module 2+, not Module 1 |
| Natives | `Function.Call(Hash.X, ...)` and `Function.Call<T>(Hash.X, ...)` | `Hash` is `GTA.Native.Hash : ulong` |

### 2.1 Natives proven to exist (because SHVDN3's own code calls them)

`SET_CURRENT_PED_WEAPON`, `GIVE_WEAPON_TO_PED`, `HAS_PED_GOT_WEAPON`, `GET_CURRENT_PED_WEAPON`, `GET_BEST_PED_WEAPON`, `IS_WEAPON_VALID`, `SET_PED_DROPS_WEAPON`, `REMOVE_WEAPON_FROM_PED`, `REMOVE_ALL_PED_WEAPONS`, `ENABLE_CONTROL_ACTION`, `DISABLE_CONTROL_ACTION`, `IS_DISABLED_CONTROL_PRESSED`, `IS_DISABLED_CONTROL_JUST_PRESSED`, `IS_CONTROL_ENABLED`, `PLAYER_ID`, `IS_CUTSCENE_ACTIVE`, `IS_PAUSE_MENU_ACTIVE`, `GET_IS_LOADING_SCREEN_ACTIVE`, `GET_ENTITY_HEALTH`, `SET_ENTITY_HEALTH`, `IS_PED_IN_ANY_VEHICLE`, `IS_PED_ON_FOOT`, `IS_PED_SHOOTING`.

`SET_PED_AMMO` is **not** used by `WeaponCollection.Give` — ammo travels as a parameter of `GIVE_WEAPON_TO_PED`.

### 2.2 Why the "proven by call site" method was necessary

The generated `NativeHashes.cs` is ~1 MB and every fetch of it truncated before reaching the `P*`/`W*` sections, so direct enumeration of names was impossible. Reading the SHVDN3 implementation that *calls* a native is a stronger proof anyway: if the call site compiles in the shipped library, the `Hash` member exists in that version.

---

## 3. Version traps

These are wrong in most tutorials and in most AI-generated mod code. Each was checked against v3.6.0 source.

| Assumption | Reality |
|---|---|
| `Notification.PostTicker(msg, true, false)` | Does not exist in v3.6.0. Exists on `main` as `FeedPost PostTicker(string, bool, bool = true)`. Use `Notification.Show(string, bool)` |
| `Game.DisableControlThisFrame` is current API | `[Obsolete]` on `main`, redirected to `GTA.Input.Controls.*`. That namespace **does not exist** in v3.6.0. Obsolete-but-present is the correct choice here |
| `Weapons.SelectBest(hash)` | No such method. Use the `BestWeapon` property |
| `WeaponHash.VehicleWeaponMine` | Not in `WeaponHash`. Vehicle weapons live in `VehicleWeaponHash` |
| LemonUI `MenuPool` / `ItemToggle` / `ItemCheckbox` | LemonUI 2.x renamed: `ObjectPool`, `NativeCheckboxItem`, `NativeItem`, `NativeSubmenuItem` |
| `Script.Interval` is public | It is `protected` |
| `Script.DisableKey(...)` | No such member. Game keys can only be suppressed per-frame via `DisableControlThisFrame` |
| NuGet is current | `ScriptHookVDotNet3` NuGet is frozen at **3.6.0, 29 Dec 2022** |
| `SET_PLAYER_FIRING_CAN_BE_INTERRUPTED` | No evidence it exists. Treat as hallucinated |

### 3.1 The nightly problem

For game builds **1.0.3258.0 or later**, the v3.6.0 release notes require **nightly.89 or later**; 3.6.0 and 3.5.1 have a compatibility issue (crash on console open / instant reload). So the runtime DLL in the game folder may be newer than the NuGet reference assembly.

Design rule: **compile against NuGet 3.6.0 and use only members that exist there.** Never reference `GTA.Input.Controls`, `PostTicker`, or anything else that only exists on `main`.

---

## 4. Adversarial (Red Team) findings

Ranked by how much they change the plan.

**4.1 "Unlocks weapons in all interiors" is not a provable claim.**
No authoritative documentation of how GTA V enforces interior weapon locks was found. Searches for a public C# mod that implements this returned nothing; GitHub code search requires auth and every search engine returned CAPTCHA pages. The closest real source found (`TitanTrainer`) is event-driven, uses only `SET_CURRENT_PED_WEAPON` and `GIVE_WEAPON_TO_PED`, has no per-tick loop and does not mention interiors.
Working hypothesis, **unverified**: the owning script re-applies the block every frame (disable firing/attack controls, force `WEAPON_UNARMED`). This is consistent with the `...ThisFrame` native family, which is proven frame-scoped.
Product consequence: the module **restores** what the game removes rather than "unlocking" anything, and its effect per interior must be measured in-game.

**4.2 Per-frame work is mandatory, not an optimisation.**
Control disables are frame-scoped, so re-enabling must happen every tick. The weapon re-select is throttled to 200 ms. `Script.Wait()` must not be called inside `Tick` — it stalls the tick thread.

**4.3 A destructive "toggle" is a data-loss bug.**
The brief specifies F10 as a toggle. If "off" removes the weapons it granted, it wipes the player's loadout mid-mission. F10 is therefore **give-all, one-shot, idempotent, non-destructive**; the master on/off lives on a separate key and never removes anything.

**4.4 Restore must never re-grant.**
Re-giving a weapon the game intentionally removed would break mission loadouts (police arrest, scripted unarmed sections). The module only re-selects a weapon the player still owns.

**4.5 Enumerating `WeaponHash` beats hardcoding a list.**
Hardcoding 30 weapon names means 30 unverified identifiers. `Enum.GetValues(typeof(WeaponHash))` + `Weapons.IsWeaponValid(hash)` + skip `Unarmed` needs zero invented names. This is why the "curated whitelist" question disappears.

**4.6 ScriptHookV must match the exact game build.**
A fresh 2026 install may post-date the current ScriptHookV (v3889.0 / 1158.13, 15 Jul 2026), which blocks modding until it updates. This is a Phase 2 risk, not a footnote — and it is the most likely reason a correctly built mod appears to do nothing.

**4.7 The hot-reload key is not documented.**
`Insert` is widely repeated but is not stated in the repo README or release notes. Verify against the shipped `ScriptHookVDotNet.ini` before relying on it.

**4.8 Entity-leak discipline (for Module 2+, stated now).**
Every `World.Create*` pairs with `MarkAsNoLongerNeeded()`, every `Blip` with `Delete()`, and cleanup runs on `Aborted`. Module 1 spawns nothing, so it cannot leak by construction.

**4.9 The GTA VI seam stays thin.**
Put every native call and hash in one file (`ScriptHookBridge.cs`) so a future swap is a single-file edit. Do **not** build an interface hierarchy for a target that may never expose a scripting API. No verified Script Hook VI exists (§7).

---

## 5. Open questions — do not inherit as fact

| Question | State |
|---|---|
| How does GTA V enforce interior/safe-zone weapon locks? | **UNKNOWN.** Four hypotheses (unequip, control disable, interior flag, script check); none sourced |
| Which interiors override successfully? | **UNKNOWN.** Requires in-game measurement |
| Is `Insert` the reload key? | **UNVERIFIED.** Not in README or release notes |
| Does any nightly add `PostTicker` / `GTA.Input.Controls`? | **UNVERIFIED.** Exists on `main`; which nightly unknown |
| What is the real per-weapon ammo cap? | **UNVERIFIED.** "9999" is community lore; may be a UI cap |
| Exact LemonUI namespaces and required DLLs | **UNVERIFIED.** Only package id/version confirmed |
| Is `dinput8.dll` redistribution permitted? | **UNVERIFIED.** Reference, never redistribute |
| Rockstar's exact current policy wording | **UNVERIFIED.** Support article timed out; PCGamingWiki used instead |

---

## 6. Phase 2 install order (blocked until the game exists)

0. **Settle Legacy vs Enhanced first.** GTA V PC exists as two editions, and which scripting runtime loads depends on which one is installed. Nothing in the external research corpus resolved this, and one document's version numbers are unverified. Determine the edition *before* downloading any hook — this is now the largest risk to Phase 2, ahead of the interior question, because it decides whether the mod loads at all.
1. Install GTA V (own copy). Launch Story Mode once, quit.
2. Record the exact game build (`GTA5.exe` product version) **and** the edition.
3. Install **ScriptHookV matching that build** — `dinput8.dll` + `ScriptHookV.dll` at the game root.
4. Install **SHVDN3 nightly.89 or later** if the build is ≥ 1.0.3258.0. Stable 3.6.0 is only correct for older builds.
5. Verify the loader: the SHVDN3 console appears on `F4`.
6. Copy `ClassicGTA.dll` + `ClassicGTA.ini` into the game's `scripts\` folder.
7. Run Story Mode only.

**Never place `ScriptHookVDotNet3.dll` inside `scripts\`.** It belongs beside `ScriptHookV.dll` in the game root, and a second copy in `scripts\` is a known cause of "wrong SHVDN version" crashes. Set `Private=False` / `Copy Local = False` on that reference so the build cannot do it by accident.

**BattlEye (verified via PCGamingWiki, not Rockstar directly):** added 2024-09-17; disable in Rockstar Launcher settings or with `-nobattleye` (Story Mode only); mods are detected by anti-tamper and block Online until removed; `dinput8.dll` must be removed before any Online session.

---

## 7. GTA VI reality check

- Official ScriptHookV page lists **GTA V only** — v3889.0 / 1158.13, released 2026-07-15. No GTA VI build, no VI mention.
- **No verified public "Script Hook VI" exists.** GitHub repo search returns four claimed hooks (`modpapi/scripthook-vi`, `AcelezXYZ/ScripthookVI`, `larpingvibecoder/gta-vi-scripthook`, `jamubc/ScriptHookVI`), all created 2026-11-23 or later, zero stars, uninspected. Metadata only; treat as untrustworthy and do not run them.
- Therefore: "swap the hashes when Script Hook VI launches" is a design intention, not a plan with a date. The abstraction is one file, so the cost of being wrong is low — but no portability promise should be made to users.

---

## 8. Module 1 design — `WeaponAndZoneUnlocker`

### 8.1 Behaviours

| Key | Action |
|---|---|
| `F8` | Master on/off for restoration. Never removes weapons |
| `F10` | Pocket Arsenal: give all valid weapons, max ammo. Idempotent, non-destructive |
| `F9` | Reserved for the emergency-services module — do not bind |

**Restoration, every tick:** enable `Attack`, `Attack2`, `Aim`, `AccurateAim`, `SelectWeapon`, `NextWeapon`, `PrevWeapon`.
**Every 200 ms:** if `Current == Unarmed` and the player still owns `lastEquipped`, re-select `lastEquipped`.
**Pocket Arsenal:** iterate `Enum.GetValues(typeof(WeaponHash))`; skip `Unarmed`; skip `!Weapons.IsWeaponValid(hash)`; `Weapon w = Give(hash, 0, false, true); w.Ammo = w.MaxAmmo;` (or `w.InfiniteAmmo = true` when configured); then re-select the weapon held before the call. **No hardcoded ammo number** — the game reports its own cap through `MaxAmmo`.

**Guards:** skip when the ped is missing, dead, in a vehicle, `Game.IsLoading`, `Game.IsCutsceneActive`, or `Game.IsPaused`.

**Notifications:** `Notification.Show(...)`.

**Rejected layer (recorded so it is not re-added by accident):** `SET_CAN_ATTACK_FRIENDLY` was proposed by one external document as part of the interior fix. It is a *friendly-fire* flag, so forcing it would let the player shoot allies and mission NPCs — a worse failure than the original restriction. Not used. `SET_PED_CAN_SWITCH_WEAPON` is a plausible second layer but unverified against the `Hash` enum, so it stays out until a compile proves it exists.

### 8.2 The testable seam

`IGameBridge` exposes only what the module needs:

```
bool     IsReady { get; }
WeaponHash CurrentWeapon { get; }
bool     HasWeapon(WeaponHash hash);
bool     SelectWeapon(WeaponHash hash);
void     EnableCombatControls();
int      GiveAllWeapons();      // returns count granted
void     Notify(string message);
void     Log(string message);
```

`ScriptHookBridge` is the only type that touches `GTA.*` or `Hash.*`. The module itself (`WeaponAndZoneUnlocker`) takes `IGameBridge` and a `nowMs` parameter, so it is pure logic with no engine dependency.

### 8.3 Tests (console runner, PASS/FAIL, non-zero exit on failure)

1. `UnarmedForOneFrame_DoesNotRestore`
2. `UnarmedPersists_RestoresLastWeapon`
3. `LastWeaponNotOwned_DoesNotRestore`
4. `Disabled_DoesNotTouchControls`
5. `Cutscene_DoesNotAct`
6. `GiveAll_SkipsUnarmedAndInvalid`
7. `GiveAll_IsIdempotent`
8. `RestoreThrottle_RespectsInterval`

### 8.4 `ClassicGTA.ini` defaults

```ini
[General]
Enabled=true
RestoreIntervalMs=200
LogLevel=Info

[Keys]
ToggleKey=F8
PocketArsenalKey=F10
```

### 8.5 Reload and safety rules

- Unsubscribe `Tick`, `KeyDown` and `Aborted` in `Dispose`. Duplicate subscriptions across `Insert` reloads are a real leak class — the handler fires twice, then three times.
- **Never remove granted weapons on abort or reload.** The player may have legitimately used or modified the inventory in the meantime; deleting on unload is how a convenience mod destroys a save's loadout.
- Never delete a *borrowed* entity (the player's car, the taxi they flagged). Only entities this mod spawned are ours to delete.
- For Tier 2 and later: track `Blip` separately from `Entity` (a blip is not an entity), restore `World.RenderingCamera` before deleting a script camera, and flush the ini on a timer rather than every tick.

### 8.6 Measurement protocol — the only route to a real claim

The zone feature cannot be proven offline. Every intervention writes one line to `ClassicGTA.log`:

```
[ts] RESTORE ped=0x.. zone=<name> dropped=<hash> took=<ms> frameControls=on
```

The first in-game session then answers, with data instead of opinion: which interiors strip the weapon, whether the control re-enable survives, how long the game takes to re-strip, and whether the restore ever fires outside interiors (a false-positive signal). Until that log exists, the honest description is "restores aggressively and records what it sees".


---

## 9. Setup checklist (adapted to this machine)

1. VS Installer → Modify → **.NET desktop development** → include **.NET Framework 4.8 targeting pack**.
2. Probe `MSBuild -version` at `C:\Program Files\Microsoft Visual Studio\18\Insiders\MSBuild\Current\Bin\`. Use MSBuild for all builds; `dotnet build` is unavailable.
3. If step 1 is refused: add `Microsoft.NETFramework.ReferenceAssemblies` 1.0.3 with `PrivateAssets=all`, plus `nuget.config` setting `globalPackagesFolder` to `.packages\`.
4. Project: **classic (non-SDK) csproj**, `net48`, Platform **x64**, Class Library, assembly name `ClassicGTA`. x64 is practice, not documented policy — the host is x64 and it avoids `BadImageFormatException`.
5. References: `System.Windows.Forms` (required for `Keys`/`KeyEventHandler`), `ScriptHookVDotNet3` 3.6.0.
6. **No LemonUI in Module 1.** It is a runtime dependency with no Module 1 benefit; a missing referenced assembly can prevent the script loading. Add it with the first menu (Tier 2).
7. Post-build copy guarded by `Condition="Exists('$(GTAVScriptsDir)')"` so the build never fails while the game is absent.
8. Build Release. Gate: **0 errors, 0 warnings.**

---

## 10. Verification gates

| Gate | Proves | Status |
|---|---|---|
| `MSBuild /p:Configuration=Release` → 0 errors, 0 warnings | every API member and native name is real | runnable now |
| `ClassicGTA.Tests.exe` → 8/8 PASS | restore logic, throttle, guards | runnable now |
| Build-time grep for `PostTicker`, `MenuPool`, `SelectBest`, `GTA.Input.Controls` | version drift cannot creep back in | runnable now |
| In-game: `F4` console, `F10` grants weapons, log shows restoration inside a restricted interior | the actual feature | **blocked — no GTA V** |

The fourth gate is the only real proof of the core feature. Until it runs, the honest description of Module 1 is "restores aggressively and logs every intervention so the first session tells us the truth".

---

## 11. Non-claims

- No promise that weapons work in any specific interior.
- No menu, no ownership persistence, no emergency dispatch, no taxi — all Tier 2.
- Story Mode only. Never Online. `dinput8.dll` out before any Online session.
- GTA VI portability is a seam, not a guarantee.

---

## 12. Questions for an external reviewer

If you are reviewing this plan, attack these specifically:

1. Is there a **documented** mechanism for interior weapon locks that this plan missed? If so, the module's design changes.
2. Is `SET_CURRENT_PED_WEAPON` the right restore primitive, or does it lose weapon state (tint, ammo-in-clip, component attachments) on re-select?
3. Does enabling `Control.Attack` every frame have side effects — melee timing, parachute, vehicle boarding, cinematic skips?
4. Is 200 ms the right restore interval, or does it produce visible flicker?
5. Does `WeaponCollection.Give` with 9999 ammo clamp, overflow, or silently fail? What is the real cap?
6. Is the `IGameBridge` seam justified, or is it over-engineering for a single module?

---

## Appendix — source URLs used

- SHVDN3 releases: `https://api.github.com/repos/scripthookvdotnet/scripthookvdotnet/releases/latest`
- SHVDN3 `v3.6.0` source files: `source/scripting_v3/GTA/{Script,Game,Control,Player,World}.cs`, `.../GTA/Weapons/{WeaponCollection,WeaponHash}.cs`, `.../GTA/Entities/Peds/Ped.cs`, `.../GTA.UI/{Notification,Screen}.cs`, `.../GTA.Native/NativeHashes.cs`
- NuGet SHVDN3: `https://www.nuget.org/packages/ScriptHookVDotNet3`
- NuGet LemonUI: `https://www.nuget.org/packages/LemonUI.SHVDN3`
- LemonUI wiki: `https://github.com/justalemon/LemonUI/wiki/Quick-Start`
- NuGet ref assemblies: `https://www.nuget.org/packages/Microsoft.NETFramework.ReferenceAssemblies`
- ScriptHookV: `https://www.dev-c.com/gtav/scripthookv/`
- BattlEye / mod policy: `https://www.pcgamingwiki.com/wiki/Grand_Theft_Auto_V`
