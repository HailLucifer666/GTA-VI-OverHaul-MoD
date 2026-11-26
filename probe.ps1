<#
.SYNOPSIS
  Repeatable /kee mutation (sabotage) probe runner for ClassicGTAOverhaul.
  Mutates real source invariants one at a time, confirms the test suite goes RED
  with the expected failing test, restores the file, and verifies final GREEN.
#>
$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $RepoRoot

function Invoke-Probe($name, $relPath, $find, $replace, $expectedFailPrefix) {
    $file = Join-Path $RepoRoot $relPath
    $orig = Get-Content $file -Raw
    if (-not $orig.Contains($find)) {
        throw "Probe $name failed: target snippet not found in $relPath"
    }
    try {
        Set-Content -Path $file -Value ($orig.Replace($find, $replace)) -NoNewline
        $null = & "$RepoRoot\build.ps1" -SkipTests 2>&1
        $out = & "$RepoRoot\tests\ClassicGTA.Tests\bin\Release\ClassicGTA.Tests.exe" 2>&1
        $exit = $LASTEXITCODE
        $matched = $out | Where-Object { $_ -like "*[FAIL]*$expectedFailPrefix*" }
        if ($exit -ne 0 -and $matched) {
            Write-Host "[RED OK] $name -> $expectedFailPrefix" -ForegroundColor Green
        } else {
            throw "Probe $name DID NOT FAIL as expected ($expectedFailPrefix)! exit=$exit"
        }
    }
    finally {
        Set-Content -Path $file -Value $orig -NoNewline
    }
}

Write-Host "=== Running Scripted /kee Sabotage Probes ===" -ForegroundColor Cyan

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
    "_interiorRestoreRetriesRemaining = 0;" `
    "91_Module1_StripHappeningOnTickNPlus1_StillRestores"

Write-Host ""
Write-Host "=== Restoring & Verifying Full Suite GREEN ===" -ForegroundColor Cyan
& "$RepoRoot\build.ps1"
