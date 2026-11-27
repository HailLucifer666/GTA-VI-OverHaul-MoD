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
Write-Host "=== Step 2: Running 17 Scripted /kee Sabotage Probes ===" -ForegroundColor Cyan

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
        "ClassicGTA_Assets_"" + PersistenceCodec.SanitizeSlotId(_slotFingerprint) + "".dat" `
        "ClassicGTA_Assets_shared.dat" `
        "99_Host_TwoSlots_KeepSeparateWalletAndAssetFilesWithoutOverwrite"

    Invoke-Probe "P06_Module1DelayedStripRestore" `
        "src\ClassicGTA.Core\Module1\WeaponAndZoneController.cs" `
        "_interiorRestoreRetriesRemaining = 8;" `
        "_interiorRestoreRetriesRemaining = 1;" `
        "91_Module1_StripHappeningOnTickNPlus1_StillRestores"

    Invoke-Probe "P07_OutdoorsNoCombatOverride" `
        "src\ClassicGTA.Core\Module1\WeaponAndZoneController.cs" `
        "if (!_wasInInterior && _weapons.GetPlayerInteriorId() == 0)" `
        "if (false && !_wasInInterior && _weapons.GetPlayerInteriorId() == 0)" `
        "104_Module1_OutdoorsFastTickNeverOverridesControls_AndF10NeverLeavesZoneFlagSet"

    Invoke-Probe "P08_StrictPersistenceRejectsTruncated" `
        "src\ClassicGTA.Contracts\PersistenceCodec.cs" `
        "return TryParseStrict(raw, expectedHeader, recordValidator, out records);" `
        "records = null; return records == null && !string.IsNullOrWhiteSpace(raw) && raw.Trim().StartsWith(expectedHeader, StringComparison.Ordinal);" `
        "106_Persistence_TruncatedBak1RejectedAndCompleteBak2Recovered_All7Engines"

    Invoke-Probe "P09_UiRecoveryNonThrowing" `
        "src\ClassicGTA.Adapter.GTAV\UiLoopGuard.cs" `
        "SafeInvoke(hideAllAction);" `
        "if (hideAllAction != null) hideAllAction();" `
        "109_UiLoopGuard_RecoveryExceptionNeverEscapes_AndTripsCircuitBreaker"

    Invoke-Probe "P10_RoadSpawnMinDistance" `
        "src\ClassicGTA.Adapter.GTAV\RoadSpawnValidator.cs" `
        "return actualDist >= minDistance && actualDist <= maxDistance;" `
        "return actualDist >= minDistance * 0.5f && actualDist <= maxDistance;" `
        "111_Dispatch_RoadSpawnRejectsBelowMin_AndFireEvaluatorHonestOutcome"

    Invoke-Probe "P11_BlackjackOnlineVoidNotPush" `
        "src\ClassicGTA.Core\Activities\BlackjackEngine.cs" `
        "_outcome = BlackjackOutcome.VoidedOnline;" `
        "_outcome = BlackjackOutcome.Push;" `
        "112_Blackjack_OnlineVoidReturnsVoidedOnlineAndRefundsOnStoryResume"

    Invoke-Probe "P12_KrakenDryLandRejected" `
        "src\ClassicGTA.Adapter.GTAV\KrakenDeliveryController.cs" `
        "if (!_adapter.IsSuitableWaterLocation())" `
        "if (false && !_adapter.IsSuitableWaterLocation())" `
        "108_Kraken_RejectsDryLandCapsAtOneEnforcesBudgetAndCleansUpOnAbortAndOnline"

    Invoke-Probe "P13_CarTheftOnlineGuard" `
        "src\ClassicGTA.Core\Arcade\ClassicCarTheftController.cs" `
        "if (_context.IsOnlineSession) return false;" `
        "// PROBE: disabled CarTheft Online guard" `
        "118_CarTheft_InOnlineSession_PerformsZeroAdapterCalls"

    Invoke-Probe "P14_WantedBribeInsufficientFundsGuard" `
        "src\ClassicGTA.Core\Arcade\ClassicWantedController.cs" `
        "if (_bribeCost > 0 && (_economy.GetCash() < _bribeCost || !_economy.TrySpend(_bribeCost, ""PayNSprayBribe"")))" `
        "if (false && _bribeCost > 0 && (_economy.GetCash() < _bribeCost || !_economy.TrySpend(_bribeCost, ""PayNSprayBribe"")))" `
        "123_Wanted_PayNSprayBribe_InsufficientFundsRejectedStarsUnchanged"

    Invoke-Probe "P15_ArcadeFreedomOnlineGuard" `
        "src\ClassicGTA.Core\Arcade\ArcadeFreedomController.cs" `
        "if (_context.IsOnlineSession) return;" `
        "// PROBE: disabled ArcadeFreedom Online guard" `
        "126_ArcadeFreedom_InOnlineSession_PerformsZeroAdapterCalls"

    Invoke-Probe "P16_CheatEngineOnlineGuard" `
        "src\ClassicGTA.Core\Arcade\ClassicCheatEngine.cs" `
        "if (_context.IsOnlineSession) return true;" `
        "// PROBE: disabled CheatEngine Online guard" `
        "129_Cheats_InOnlineSession_HardBlocksEveryCheatActionAndTick"

    Invoke-Probe "P17_TurfWarPersistRollbackGuard" `
        "src\ClassicGTA.Core\Arcade\TerritoryWarController.cs" `
        "_capturedZones.Remove(capturedZone);" `
        "// PROBE: skipped turf rollback on persist failure" `
        "134_TurfWar_OnlineTransitionAbortsWithoutDeleting_AndPersistFailureRollsBack"
}
finally {
    Write-Host ""
    Write-Host "=== Step 3: Final Restore & Full Suite Verification ===" -ForegroundColor Cyan
    & "$RepoRoot\build.ps1"
}
