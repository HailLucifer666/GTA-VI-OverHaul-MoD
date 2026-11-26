<#
.SYNOPSIS
  Repeatable /kee mutation (sabotage) probe runner for ClassicGTAOverhaul.
  1. Verifies a GREEN baseline first (never trusts probes over an already-red suite).
  2. Mutates real source invariants one at a time, asserting an EXACT literal "[FAIL] <testName>" prefix.
  3. Restores byte-identical UTF-8 content in finally and verifies final GREEN.
#>
$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $RepoRoot

Write-Host "=== Step 1: Verifying GREEN Baseline ===" -ForegroundColor Cyan
& "$RepoRoot\build.ps1"

function Invoke-Probe($name, $relPath, $find, $replace, $expectedFailPrefix) {
    $file = Join-Path $RepoRoot $relPath
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    $orig = [System.IO.File]::ReadAllText($file, $utf8NoBom)
    if (-not $orig.Contains($find)) {
        throw "Probe $name failed: target snippet not found in $relPath"
    }
    try {
        [System.IO.File]::WriteAllText($file, $orig.Replace($find, $replace), $utf8NoBom)
        $null = & "$RepoRoot\build.ps1" -SkipTests 2>&1
        $out = & "$RepoRoot\tests\ClassicGTA.Tests\bin\Release\ClassicGTA.Tests.exe" 2>&1
        $exit = $LASTEXITCODE
        $literalPrefix = "[FAIL] " + $expectedFailPrefix
        $matched = $out | Where-Object {
            ($_ -is [string]) -and $_.StartsWith($literalPrefix, [System.StringComparison]::Ordinal)
        }
        if ($exit -ne 0 -and $matched) {
            Write-Host "[RED OK] $name -> $literalPrefix" -ForegroundColor Green
        } else {
            throw "Probe $name DID NOT FAIL as expected ($literalPrefix)! exit=$exit"
        }
    }
    finally {
        [System.IO.File]::WriteAllText($file, $orig, $utf8NoBom)
    }
}

Write-Host ""
Write-Host "=== Step 2: Running Scripted /kee Sabotage Probes ===" -ForegroundColor Cyan

try {
    Invoke-Probe "P01_OnlineSafetyGuard" `
        "src\ClassicGTA.Core\Module1\WeaponAndZoneController.cs" `
        "if (_context.IsOnlineSession) return false;" `
        "// PROBE: disabled Online guard" `
        "04_FastTick_InOnlineSession_NeverActs"

    Invoke-Probe "P02_BorrowedVehicleProtected" `
        "src\ClassicGTA.Infrastructure\EntityOwnershipRegistry.cs" `
        "// EntityOwnership.Borrowed: NEVER delete or release player/world entities." `
        "else if (rec.Ownership == EntityOwnership.Borrowed) { _adapter.DeleteEntity(rec.Handle); }" `
        "17_EntityRegistry_OwnedDeleted_BorrowedNeverTouched_DetachedReleased"

    Invoke-Probe "P03_BlackjackDealerDrawLoop" `
        "src\ClassicGTA.Core\Activities\BlackjackEngine.cs" `
        "while (DealerTotal < 17)" `
        "while (false && DealerTotal < 17)" `
        "83_Blackjack_DealerDrawsUntil17OnStand"

    Invoke-Probe "P04_BlackjackHoleCardHidden" `
        "src\ClassicGTA.Core\Activities\BlackjackEngine.cs" `
        "return CalculateHandValue(new[] { _dealerHand[0] });" `
        "return DealerTotal;" `
        "84_Blackjack_HidesHoleCardUntilSettled"

    Invoke-Probe "P05_AssetSlotIsolation" `
        "src\ClassicGTA.Core\Economy\AssetOwnershipEngine.cs" `
        "if (!string.Equals(loadedSlot, _slotFingerprint, StringComparison.OrdinalIgnoreCase))" `
        "if (false && !string.Equals(loadedSlot, _slotFingerprint, StringComparison.OrdinalIgnoreCase))" `
        "88_Assets_DifferentSaveSlotFingerprint_Isolated"

    Invoke-Probe "P06_Module1DelayedStripRestore" `
        "src\ClassicGTA.Core\Module1\WeaponAndZoneController.cs" `
        "_interiorRestoreRetriesRemaining = 8;" `
        "_interiorRestoreRetriesRemaining = 1;" `
        "91_Module1_StripHappeningOnTickNPlus1_StillRestores"
}
finally {
    Write-Host ""
    Write-Host "=== Step 3: Final Restore & Full Suite Verification ===" -ForegroundColor Cyan
    & "$RepoRoot\build.ps1"
}
