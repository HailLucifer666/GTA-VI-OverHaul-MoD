# Security, Safety & Legal Guardrails

## Operating mode
- Story Mode / offline single-player only.
- Do not invoke multiplayer, online services or competitive features.
- Do not bypass anti-cheat to access online services.
- Do not use leaked console builds, leaked symbols, private SDKs or unauthorized executable reverse engineering.
- Test with a clean backup of the game and saves.

## Intellectual property
- Do not redistribute Rockstar executables, game archives, proprietary textures, audio, models, maps or scripts.
- Do not copy assets from another GTA title into the project without a clear legal basis.
- Prefer original UI, original code and assets created by the project or licensed for redistribution.
- Keep third-party notices for all dependencies.
- Do not use Rockstar trademarks in a way that implies endorsement.
- Do not sell or monetize unless current Rockstar rules and applicable law clearly permit the exact distribution model.
- Do not include story expansions, copied missions, maps or proprietary story content.

## Current policy review
Rockstar's support page updated 2026-09-15 links to its Terms of Service, Creator Platform License Agreement and Mod Guidelines. The project owner must read the live documents before distribution and re-check them before each release. Policy summaries are not a substitute for the actual terms.

## Data and save safety
- Keep mod data separate from native game saves.
- Use schema versioning and backups.
- Never overwrite native save files.
- Validate all loaded values.
- Do not execute code or load assemblies from save data.
- No telemetry or external network requests in the mod.

## Distribution
- Package only original project binaries/source and permitted dependency notices.
- Instruct users to obtain required third-party dependencies from their official sources.
- Do not bundle the game or Script Hook binaries unless the relevant licenses expressly permit it.
- Clearly state supported game and dependency versions.
- Provide uninstall and rollback instructions.

## Disclaimer
This document is a practical engineering risk checklist, not legal advice. The project owner is responsible for reviewing the current Rockstar terms, applicable law and each dependency's license.
