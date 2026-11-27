# Features

The honest status of every system in the mod. No dates, no promises — just what exists today.

**Legend**

- ✅ **Wired** — playable through the in-game `F7` hub, with a tested engine behind it.
- 🧪 **Lab** — the engine is built and unit-tested, but it has no in-game adapter yet.
- 🔒 **Classified** — not shown yet. Three systems stay under wraps until the first live session.

> **Live verification:** First live load on **GTA V Legacy (`1.0.3889.0`, ScriptHookV `3889.0`, SHVDN `v3.7.0-nightly.206`)** is verified (`2026-11-27`) — `F7` hub, Story Cash & Mod Wallet grants, asset purchases, and atomic `.dat` + `.bak1–.bak3` persistence ran cleanly with `0` errors. Full 30-reload & 30-minute soak verification (including the new Phase 7 Arcade Pack menus) is in progress under Phase 6B.

## Wired today (8 `F7` submenus · 11 scheduled modules)

| | System | How it behaves |
|:--:|:--|:--|
| 🚗 | **Instant Car Theft** | Unlocks targeted vehicles, disables car alarms, and pacifies drivers without lockpick QTEs. World/player vehicles are never tracked as `Owned` and never deleted. |
| ⭐ | **Classic Wanted & Heat Reset** | Restores classic 20s out-of-sight police LoS cooldown (pause-safe and `GameTimeMs` wrap-around safe) plus a `$500` Pay 'n' Spray bribe via Mod Wallet that clears wanted stars and Criminal Profile Heat. |
| ⛽ | **Arcade Freedom** | Independent toggles for No Refuelling / Engine Top-Up, No Survival Chores (stamina & vitals), and opt-in No Vignette (cleanly restored on `Stop()`/`Dispose()`). |
| 🎮 | **Classic Sandbox Cheats** | Story-Mode-only hub: Health/Armor refill, Clear/Never Wanted, per-frame Super Jump, Vehicle Repair, allowlisted Classic Car Spawner (`cheetah2`, `infernus2`, `turismo2`, etc.), Weather, and Clock `+6h`. Hard-blocked in Online. |
| 🏴 | **Gang Territory / Turf Wars** | 4 districts (`turf.davis`, `turf.del_perro`, `turf.downtown`, `turf.docks`), 3 waves × 3 enemies, `$5,000` capture bonus, and `$2,500`/UTC-day passive turf income (7-day cap, backward-clock safe) persisted atomically with `TURF_SCHEMA=1`. |
| 🚕 | **Taxi Shift** | Five states: idle, searching, pickup, en route, payout. A fare spawns on a sidewalk with a route blip, gets in, and pays on arrival with a par-time tip. If your own cab is borrowed, it is never deleted. |
| 🚨 | **911 Dispatch** | Call police, ambulance or fire. The unit spawns on a road near you, drives over, and acts on arrival: police engage hostiles, ambulance heals you, fire tries to extinguish nearby flames. One call at a time, with a cooldown. |
| 🔫 | **Pocket Arsenal + Safe Zones** | Grants every weapon the game considers valid, at max ammo, without removing anything you own. Inside restricted interiors it restores combat controls and re-selects the weapon you were holding. |
| 📈 | **LCN Stocks** | Three listed companies. Buy and sell at the current price; the market moves on a tick and your holdings persist. |
| 🃏 | **Blackjack** | Full rules: hit, stand, double down, soft-ace handling, dealer stands on 17, 3:2 naturals. Played with story-mode cash. A round that is interrupted is voided, not silently lost. |
| 🐕 | **Guard Dogs** | Buy a dog, summon it, and command follow, sit or guard. It is released — not deleted — when dismissed, and it is cleaned up if it dies. |

## Also wired

| | System | How it behaves |
|:--:|:--|:--|
| 💵 | **Story Cash & Mod Wallet** | Check balances and grant Story Mode cash or Classic GTA Mod Wallet funds (Story Mode only; fail-closed in Online). |
| 🏠 | **Safehouse ownership** | Buy a safehouse through the hub. The ownership ledger is separate from GTA's cash and survives a reload. World delivery is pending. |
| 🏢 | **Passive business income** | Own a business, collect income against elapsed days, with a cap on time away. |
| 🚤 | **Kraken delivery** | Own the submersible, then summon it on open water. One at a time; it will not spawn on a street. |
| 🚑 | **Emergency side-jobs** | A twelve-level Vigilante-style loop that escalates and pays out per wave. |
| 🤝 | **Companion hangouts** | Call a buddy, have them follow and assist in combat, then dismiss them cleanly. |

## In the lab

Built, unit-tested, not yet wired to the game:

- 🖥️ **ClassicNet hub** — an in-game portal that links the other systems.
- 🛋️ **Furniture placement** — buy and place props inside owned safehouses.
- 💀 **Rampages** — timed, weapon-locked waves with medal ratings.
- 🏅 **Mission medals** — best-run tracking for sandbox contracts.
- 🎥 **First-person camera** — head-tracked free-roam view.
- 📊 **RPG skills** — XP, levels and perks that affect gameplay.

## Classified

🔒 Three systems are not shown yet. They will be revealed after the first live session, once they are real enough to demonstrate. No hints, no promises.

## Why some things are held back

The public repo carries the code and the status. The design detail — balance numbers, spawn coordinates, the full remaining roadmap — lives privately. That keeps the project's edge while everything you can actually run stays open under GPL-3.0.

## What is not here

- No GTA Online support. The mod hard-blocks itself in multiplayer.
- No real-money mechanics.
- No redistributed game assets.
