# Adjudication of Six External Research Documents

**Date:** 2026-11-26
**Inputs:** `Multi LLM ReSearchs\` — 7 files, ~285 KB (Qwen 4 Max.md is 0 bytes, so six are readable)
**Method:** each document was read in full by a separate reader; every load-bearing API claim was then re-checked against ScriptHookVDotNet `v3.6.0` source. Claims below are marked **CHECKED** (I read the source myself) or **CLAIMED** (reported by a reader, not independently verified).

> Read this before trusting any of the six documents. Three separate reader assertions turned out to be *wrong in the opposite direction* — they accused working code of not compiling. A research corpus that errs toward false alarms is less dangerous than one that errs toward false confidence, but both cost time.

---

## 1. The corpus at a glance

| ID | Document | Size | What it actually is | Verdict |
|---|---|---|---|---|
| S1 | (Claude Sonnet 5.5) Master Plan | 360 lines | Strategy, no code, zero citations | **Best judgement**, thinnest detail |
| S2 | Restoring the Classic Sandbox | 444 lines | Strategy + a "complete, audited" module | **Do not build from this** |
| S3 | GPT Engineering Pack | 19 files / 1,135 lines | Real repo: csproj, code, ADRs, docs | **Best artifact** |
| S4 | GPT Research | 1,370 lines | Two chat turns stapled together | Useful details, unreliable provenance |
| G1 | Gemini.md | 233 lines | PRD + MVP + 7-day plan | Useful structure, wrong diagnosis |
| G2 | GEmini Argon Research | 86 KB / ~28k-char lines | Council + Red Team, 26-feature matrix | Most ambitious, most invented |
| — | Kimi K 3.1 Pro | 4,458 lines | Full engineering plan, zero citations | Strongest epistemics, weakest sourcing |

---

## 2. Where all six agree — treat as settled

These survived six independent attempts and my own source checks. They are the plan's foundation, not up for debate:

1. **.NET Framework 4.8 class library.** Unanimous, and correct (`ScriptHookVDotNet3` NuGet is `net48`).
2. **Mods derive from `GTA.Script`**, subscribing `Tick` / `KeyDown` / `Aborted`.
3. **`Enum.GetValues(typeof(WeaponHash))` + `IsWeaponValid` + skip `Unarmed`** for the arsenal. **CHECKED**: `IsWeaponValid(WeaponHash)` and `Give(WeaponHash, int, bool, bool)` both exist.
4. **Control re-enabling must run every frame; heavy work must be throttled.** The native family is literally `...ThisFrame`, so this is forced, not stylistic.
5. **`Script.Wait()` must not be used to throttle a tick.** Use a `Game.GameTime` deadline. **CHECKED**: `Game.GameTime` is `public static int` returning `GET_GAME_TIMER`.
6. **Entities need `MarkAsNoLongerNeeded()`; blips are separate and need `Delete()`; clean up on `Aborted`.**
7. **No hash-swap portability promise.** Four of six reject it outright; two assert it without evidence.
8. **The interior lock is not a single switch.** Four of six refuse or defer the feature for that reason. **Zero of six name a native that enforces it.**

---

## 3. The hallucination ledger

Every entry below would have cost real debugging time. This is the table to keep.

| Claim | Where | Verdict | Evidence |
|---|---|---|---|
| `CanBeDamaged` is a ped property to clear on ambient NPCs | G1 | **FALSE** | **CHECKED** `Entity` has `IsInvincible` + `IsOnlyDamagedByPlayer`. No `CanBeDamaged` member exists |
| `Game.GameTimeDelta` | S2 | **FALSE** | **CHECKED** no such member; real ones are `GameTime` (int ms), `LastFrameTime`, `FrameCount`, `FPS` |
| `Game.IsContentUpdateAvailable` is a legitimate native | S2 | **FALSE** | No such member; and S2's own "audit" cites it as proof of correctness |
| `Output.WriteLine(...)` | S2 | **FALSE** | No `GTA.Output` type in v3 |
| `UI.Notify(...)` | S2 | **FALSE** | SHVDN2 API. v3 surface is `GTA.UI.Notification.Show(string, bool)` — **CHECKED** |
| `Enum.GetValues<WeaponHash>()` | S2 | **FALSE on net48** | Generic overload is .NET 5+. Use `Enum.GetValues(typeof(WeaponHash))` |
| `Keys.F10` without `using System.Windows.Forms;` | S2 | **Won't compile** | CS0103 |
| `ScriptHookVIDotNet.dll` | G2 | **INVENTED** | No such product. Spellings vary three ways within one document |
| "community GTA VI Native DB" | G2 | **NOT ESTABLISHED** | Asserted as a 30-minute port input |
| `VCPD_Cruiser`, "Vice-Dale" | G2 | **INVENTED** | Conflates Vice City and Leonida |
| `SET_PLAYER_FIRING_CAN_BE_INTERRUPTED` | (my own earlier candidate) | **NO EVIDENCE** | Excluded from the design |
| `Vector3.Around(radius)` | G2 | **LIKELY FALSE** | Real family is `RandomXY/RandomXZ/RandomXYZ(float radius)` |
| `Player.PedGroup` | G2 | **LIKELY FALSE** | Real member is `Player.Group` |
| "Fully compliant with NexusMods / GTA5-Mods ToS" | G2 | **UNVERIFIABLE** | Policy claim with no citation |
| `Weapons.Current` returns `WeaponHash` | reader report on G2 | **FALSE — reader was wrong** | **CHECKED** `public Weapon Current`; `Weapon` is `public sealed class Weapon` with `public WeaponHash Hash`. G2's `currentWeapon.Hash` compiles |
| Unqualified `KeyEventArgs` is ambiguous (CS0104) | reader report on S3 | **FALSE — reader was wrong** | **CHECKED** v3.6.0 aliases `WinForms = System.Windows.Forms` and declares `KeyDown` as `WinForms.KeyEventHandler`. No `GTA.KeyEventArgs` exists, so no ambiguity |
| `GET_CLOSEST_VEHICLE_NODE` is the right dispatch primitive | G1 | **Wrong primitive** | **CHECKED** `World.GetNextPositionOnStreet(Vector3, bool)` wraps `GET_NTH_CLOSEST_VEHICLE_NODE` |

**S2 is the one document to quarantine.** It labels its own code "the complete, audited C# source code" and "deemed complete and ready for integration", while the code contains a comment saying the key native is a placeholder. Its "Council audit" states "No hallucinated natives were found" — about a file that never calls a native for the feature it claims to implement. It carries 71 citation markers whose targets include a RimWorld wiki, a Facebook post and a Nairaland forum thread. Its Module 1 implementation is a 100-metre radius around `(253.22, -589.83, 44.85)` plus handing the player a baseball bat — while its own allow-list declares the bat illegal.

---

## 4. Conflicts, resolved

### 4.1 NuGet vs manual DLL reference
S2 insists DLLs are "not standard NuGet packages". That is **false**: the official `ScriptHookVDotNet3` 3.6.0 package exists (published by the project's own owners), and `LemonUI.SHVDN3` 2.2.0 depends on it. S3 and S4 use HintPath-to-game-folder instead, which is also valid but **requires the game to be installed** — and it isn't.

**Resolution:** NuGet for compile now; runtime DLLs always come from the game folder at play time. Never ship `ScriptHookVDotNet3.dll` inside `scripts/` (S3's `Private=False` rule is correct).

### 4.2 The ammo number
Every document hardcodes `9999`. All six are guessing.
**Resolution:** **CHECKED** `Weapon` exposes `MaxAmmo`, `Ammo`, `AmmoInClip`, plus set-only `InfiniteAmmo` and `InfiniteAmmoClip`. Give the weapon, then set ammo from the game's own `MaxAmmo`. No magic number, no clamp to reason about, and `SET_PED_INFINITE_AMMO_CLIP` becomes unnecessary.

### 4.3 What enforces the interior lock
Two documents assert specific mechanisms, with no citation:
- **G1:** clearing `CanBeDamaged` on ambient peds — false property, wrong diagnosis.
- **G2:** per-frame control disables *plus* `SET_PED_CAN_SWITCH_WEAPON=false` and `SET_CAN_ATTACK_FRIENDLY=false`.

`SET_CAN_ATTACK_FRIENDLY` is a **friendly-fire** flag. Forcing it looks like it would let the player shoot allies and mission NPCs, which is a worse failure than the original restriction. **Rejected** for Module 1.

**Resolution:** keep the verified core (re-enable combat controls per frame; re-select a weapon the player still owns). Treat the flag hypothesis as a *candidate experiment to run in-game*, never as shipped behaviour. This is also S3's recommendation, and it is the best-reasoned position in the corpus: inventory the restriction before overriding it.

### 4.4 GTA VI
Four documents correctly refuse to promise anything. G2 invents an assembly name, a native database and a model name, and promises "zero rewriting".
**Resolution:** unchanged from our plan — one thin seam file, no portability claim. S1 phrases it best: "the portable part is the game logic; the non-portable part is the bridge."

### 4.5 A fork nobody in the corpus resolves
S4 raises **GTA V Legacy vs Enhanced** as two editions needing two deploy targets and a possibly different scripting runtime (`SHVDNE`). No document verifies which SHVDN build supports which edition, and S4's version numbers are unverified. This is now the **largest open risk in Phase 2** — bigger than the interior question, because it decides whether the mod loads at all.

---

## 5. Ideas worth stealing (ranked)

1. **S3's refusal + test-first path.** Don't override a restriction you have not measured. Converts an unprovable feature into an inventory procedure.
2. **S1's premise critique:** GTA V already has first-person, properties, stocks, rampages and all-weapons. So for GTA V this mod is a *convenience/cheat* layer; genuine "restoration" only starts at VI. That reframes the whole backlog and should be said out loud to users.
3. **S3's `Exists('$(GTA5Dir)\GTA5.exe')` gate** on the post-build copy — a build that cannot silently mis-deploy. Matches our conditional copy.
4. **S4's dual deploy configurations** (`ReleaseLegacy` / `ReleaseEnhanced`) with an env-var path — the right shape once the edition question is settled.
5. **Blips tracked separately from entities; restore the render camera before deleting it** (G2). A camera left bound to a deleted object is a black-screen bug.
6. **`Model.MarkAsNoLongerNeeded()` immediately after spawn** (G2) — streaming refcount hygiene.
7. **INI write throttling** (G2): persisting every tick causes micro-stutter; flush on a timer and on unload.
8. **Menu visibility is a property toggle, not a rebuild** (G2) — avoids leaking NativeMenu objects across reloads.
9. **Three-way entity ownership — owned / borrowed / detached** (Kimi). The "borrowed" class (player's taxi, player's car) is the one most mods delete by accident.
10. **Don't delete granted weapons on abort** (S3): the player may have legitimately used them since.
11. **Unsubscribe `KeyDown`/`Aborted` in `Dispose`** (S3) — duplicate subscriptions across reloads are a real SHVDN leak class.
12. **Evidence ladder E0–E4** (Kimi) and S3's review contract: never write "compiled", "tested" or "safe" unless that action happened.

---

## 6. Changes to our plan

| # | Change | Reason |
|---|---|---|
| 1 | Replace hardcoded `9999` with `Weapon.MaxAmmo`; expose `InfiniteAmmo` as a config option | Removes the corpus's most-repeated invented number |
| 2 | Add `Game.GameTime` scheduling; never `Script.Wait` in `Tick` | Verified member; six-way consensus |
| 3 | Reject `SET_CAN_ATTACK_FRIENDLY`; keep `SET_PED_CAN_SWITCH_WEAPON` as a compile-verified *candidate* only | Different semantics; mission risk |
| 4 | Rename the module's user-facing description: it **restores**, it does not **unlock** | S3's discipline; keeps us honest |
| 5 | Add a measurement protocol: log every intervention with ped/zone/weapon, so the first in-game session produces evidence | Turns the unprovable claim into data |
| 6 | Phase 2 gains a **first** step: determine Legacy vs Enhanced before installing anything | S4's fork; nobody resolved it |
| 7 | Keep NuGet for compile, runtime DLLs from the game folder, `Private=False` | Resolves the NuGet-vs-manual conflict |
| 8 | Add `Dispose` unsubscription + no-weapon-removal-on-abort | S3's reload-safety rules |
| 9 | Note for Tier 2: track blips separately, restore camera first, throttle INI writes, toggle menu visibility | G2 residue worth keeping |

---

## 7. What the corpus still does not answer

- How the interior restriction is enforced. Six documents, zero sources.
- Whether `SET_PED_CAN_SWITCH_WEAPON` / `SET_CAN_ATTACK_FRIENDLY` exist in the `Hash` enum of v3.6.0 — unresolved because the generated enum file is too large to read through a fetch. **The compiler will settle this in seconds**, which is the argument for building early.
- Legacy vs Enhanced support in SHVDN.
- Whether any nightly adds `PostTicker`; whether `Insert` is the reload key.
- Real per-weapon ammo caps — now answerable at runtime via `Weapon.MaxAmmo`.

---

## 8. Scoring

| Source | Compile-safe | Honest about limits | Useful density | Would I build from it |
|---|---|---|---|---|
| S3 GPT Pack | Yes | Yes | High | **Yes**, as a structural reference |
| S1 Sonnet | N/A (no code) | Yes | Medium | Yes, for strategy |
| Kimi | Yes (claimed) | Yes on VI | Medium | Partly — drop the invented framework |
| S4 GPT Research | Yes | Yes | Medium | Partly — two chats, pick one half |
| G2 Gemini Argon | Mostly | No | High but padded | No — harvest ideas only |
| G1 Gemini | Yes | No | Medium | No — wrong diagnosis |
| S2 Restoring | **No** | **No** | Low | **Never** |
