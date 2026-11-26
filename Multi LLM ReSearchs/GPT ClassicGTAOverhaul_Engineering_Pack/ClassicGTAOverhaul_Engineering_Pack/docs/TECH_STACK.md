# Technology Stack & Dependency Policy

## GTA V prototype

| Component | Selection | Notes |
|---|---|---|
| OS | Windows 10/11 64-bit | Supported PC environment |
| IDE | Visual Studio Community 2022 | Free IDE |
| Language | C# | Keep syntax compatible with chosen compiler |
| Target framework | .NET Framework 4.8 | SHVDN3 target |
| Game runtime | GTA V Story Mode | Offline only |
| Hook | Script Hook V | Must match installed GTA V build |
| .NET bridge | ScriptHookVDotNet3 | Use a compatible release |
| UI | LemonUI.SHVDN3 2.2.0 | Optional for MVP |
| Config | INI | Human editable |
| Save data | System.Text.Json or Newtonsoft.Json | Pin version; JSON schema versioned |
| Tests | NUnit | Pure domain logic first |
| Version control | Git | Branch and tag releases |
| CI | GitHub Actions | Build and unit tests |

## Package/version notes

- SHVDN repository currently documents .NET Framework >= 4.8 and requires the Script Hook V dependency.
- SHVDN documentation warns that runtime files should be taken from the same release archive and kept together.
- SHVDN repository notes compatibility differences between stable and nightly builds for newer GTA V game versions. Pin the chosen tested combination; do not casually mix DLLs.
- LemonUI.SHVDN3 2.2.0 targets .NET Framework 4.8 and declares a dependency on ScriptHookVDotNet3 >= 3.6.0.
- The MVP does not require LemonUI, so do not add it until a menu is needed.

## Cost

All selected development tools have free/community options. GTA V itself is not included in the $0 tooling budget. Optional paid assets, cloud services and AI subscriptions are not required.

## Dependency policy

- Pin package versions.
- Commit packages.lock.json where supported.
- Never copy a dependency from an untrusted mirror.
- Check package license and third-party notices.
- Do not redistribute Script Hook, LemonUI or game binaries inside the mod package unless their respective licenses explicitly permit it.
- Document manual dependency installation.
