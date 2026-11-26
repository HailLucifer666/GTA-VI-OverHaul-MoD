# Feature Backlog — 26 Candidate Restorations

Status vocabulary:
- **Candidate:** user-requested feature; not independently confirmed absent.
- **Research:** needs in-game verification and feasibility spike.
- **Prototype:** GTA V implementation can be tested.
- **Blocked:** requires unsupported APIs, custom assets/physics or platform availability.

| ID | Feature | System | Priority | Feasibility / notes |
|---|---|---|---|---|
| F01 | Buyable houses/safehouses | Ownership | P1 | GTA V marker/menu + save registry; GTA VI map/door access adapter needed |
| F02 | Passive-income businesses | Economy | P1 | Straightforward domain logic; balance and game-time scheduler |
| F03 | Buy boats/planes/helicopters | Vehicles | P1 | Dealership UI, spawn/ownership; safe spawn points required |
| F04 | Submarines | Vehicles | P3 | Vehicle availability and underwater controls need spike |
| F05 | Stock market | Economy | P2 | Simulated market possible; do not claim real exchange parity |
| F06 | In-game web browser | UI / World | P3 | Avoid embedding real web browsing; consider fictional local bulletin UI |
| F07 | Home furnishing | Property | P3 | Requires permitted props/interior support; high asset burden |
| F08 | Taxi jobs | Activities | P1 | Good first state-machine job |
| F09 | Vigilante/ambulance/firefighter | Activities | P1 | Separate job modules; entity cleanup critical |
| F10 | Rampages | Activities | P2 | Timers, scoring and challenge definitions |
| F11 | Tennis | Activities | P3 | Animation/physics spike; likely expensive |
| F12 | Darts | Activities | P2 | UI + scoring; original UI/art |
| F13 | Bowling | Activities | P2 | Physics/prop interaction spike |
| F14 | Cinema | World | P3 | Avoid redistributing copyrighted clips/audio |
| F15 | Amusement rides | World | P3 | Animation/seat/vehicle support spike |
| F16 | Casino/card games | Activities | P3 | Fictional in-game currency only; policy/legal review |
| F17 | Skateboarding | Player | P3 | Custom physics/animation spike |
| F18 | Mission replay/medals | Missions | P3 | Potential conflict with persistent story choices; do not overwrite native saves |
| F19 | Free-roam first-person | Camera | P2 | Camera mode and vehicle camera adapter |
| F20 | Pocket arsenal | Inventory | P0 | MVP |
| F21 | RPG skill bars | Progression | P2 | Core XP and progression model |
| F22 | Prone crawling | Player | P3 | Requires animation and movement support |
| F23 | Dual-wielding | Combat | P3 | Requires animation, aim and weapon handling support |
| F24 | Casual sex-worker interactions | Social | Excluded pending review | Not an MVP; high content/policy risk; do not reproduce explicit content |
| F25 | Friend hangouts | Social | P3 | Companion AI and activity integration |
| F26 | Emergency phone calls | World services | P2 | Dispatch framework; avoid interfering with native emergency logic |
| F27 | Purchasable pets | Social | P3 | Ownership, companion AI, persistence and cleanup |

Note: the source contains 26 numbered bullets. This backlog has 27 atomic implementation candidates because the single “darts and bowling” bullet is split into two independently scoped activities. Feature verification must be done against the actual target build.
