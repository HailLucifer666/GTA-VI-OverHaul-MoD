# GTA V Development Setup — Visual Studio 2022

## Before starting

- Install Visual Studio Community 2022.
- In Visual Studio Installer, select **.NET desktop development**.
- Install .NET Framework 4.8 Developer Pack / targeting pack if missing.
- Install current Microsoft Visual C++ Redistributable (x64).
- Install GTA V PC and confirm Story Mode works without mods.
- Back up the game installation and saves.

## Five-minute project checklist

1. Open Visual Studio Community.
2. Select **Create a new project**.
3. Choose **Class Library (.NET Framework)**, C#.
4. Name it `ClassicGTA.GTA5`.
5. Select **.NET Framework 4.8**.
6. Open NuGet Package Manager and install `LemonUI.SHVDN3` version `2.2.0` only when UI is needed. The MVP source does not use LemonUI.
7. Add a reference to `ScriptHookVDotNet3.dll` from the installed SHVDN release. Set **Copy Local = False** because the game installation provides the runtime assembly.
8. Set platform target to **x64**.
9. Add the source file `WeaponAndZoneUnlocker.cs`.
10. Build Release.

## SHVDN installation

Follow the current SHVDN repository's installation instructions. Important: install the matching Script Hook V and SHVDN release files together. Do not mix the ASI loader and API DLLs from different release archives. Check the current compatibility note for your GTA V build before selecting stable/nightly runtime files.

## Post-build copy

Set the `GTA5_DIR` environment variable to the folder containing `GTA5.exe`, or edit the project post-build event to point to your local path.

Example command:
```bat
if not exist "$(GTA5_DIR)\scripts" mkdir "$(GTA5_DIR)\scripts"
copy /Y "$(TargetPath)" "$(GTA5_DIR)\scripts\"
```

For the provided project file, set `GTA5_DIR` as a Windows environment variable before opening Visual Studio, or edit the project property.

## Runtime files

- Install Script Hook V and SHVDN using their official release instructions.
- Place LemonUI's SHVDN3 runtime files in the `scripts` folder only if a module uses LemonUI.
- Do not bundle these runtime files with your own mod unless their licenses permit it.

## Hot reload

SHVDN user guides document reloading scripts with Insert in supported configurations. Save your work, build, copy the DLL, then use the supported reload behavior. If a reload fails, check SHVDN logs and confirm the assembly is not locked or stale.

## First-run test

1. Launch GTA V Story Mode offline.
2. Confirm SHVDN loaded without errors.
3. Load a save in a safe location.
4. Press F10.
5. Confirm the notification appears and weapons are granted.
6. Press F10 again.
7. Confirm only module-added weapons are removed.
8. Repeat with a pre-existing weapon loadout.
9. Review SHVDN logs after the session.
