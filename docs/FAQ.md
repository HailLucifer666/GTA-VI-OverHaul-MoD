# FAQ

### Is this a GTA VI mod?

Not yet. It is a **GTA V Legacy** mod today, built so that the gameplay layer can move to GTA VI when a public PC scripting interface exists. The core systems never reference the game's own APIs, so only the adapter would change.

### Then why does the art say GTA VI?

Because that is the destination. The art is a fan-made vision of the project, not a claim that a GTA VI build exists. If you want the technical truth, it is in [`FEATURES.md`](FEATURES.md): everything runs against GTA V right now.

### Does it work in GTA Online?

No. The mod hard-blocks itself in multiplayer and does nothing there. It is built for Story Mode only, and that guard is not configurable.

### Is it free?

Yes. GPL-3.0, no paywalls, no locked features, no paid tier. If someone tries to sell you this mod, they are not us.

### Will it corrupt my save?

The mod keeps its own wallet, ledger and log under `ClassicGTA_Data\`, and never edits your GTA save file. Money changes happen in-game the same way ordinary spending does. Still, back up your save before running any mod — that advice applies to every mod, not just this one.

### Which GTA V edition?

**Legacy** (the original PC release). The Enhanced Edition uses a different script hook and is not supported yet.

### Nothing happened when I pressed the buttons.

Two possibilities. First, while first live load on GTA V Legacy (`1.0.3889.0`) is verified, full feature soak testing is still underway in Phase 6B. Second, if the session detector cannot read the game's network state, the mod fails closed and stays inert on purpose. Check `ClassicGTA_Data\ClassicGTA.log`.

### Why are three features hidden?

Because they are not real enough to show yet. The public repo carries the code and an honest status; the remaining design detail lives privately. Nothing hidden is promised, and nothing shown is faked.

### Can I use it with other mods?

Usually yes, but two mods that both override combat controls or weapon loadouts will fight. If something behaves strangely, disable other script mods and retest.

### How do I contribute?

Pick a `good first issue`, or open one describing what you want to build. Read [`CONTRIBUTING.md`](../CONTRIBUTING.md) first — the short version is: one change per branch, a test with every fix, both gates green before the PR.

### Does it phone home?

No. There is no telemetry, no analytics, no network call at runtime. The only network access in the repo is the build script downloading two pinned NuGet packages.

### Can I stream or make videos about it?

Yes, please. Credit the project and link back. Do not present it as an official Rockstar product.

### Is it legal?

It is an unofficial fan project: no game assets are redistributed, no pre-release material is used, and it does not run online. It is not affiliated with or endorsed by Rockstar Games or Take-Two Interactive. If you need legal advice, ask a lawyer — not a README.
