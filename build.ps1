param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

$msbuildCandidates = @(
    "C:\Program Files\Microsoft Visual Studio\18\Insiders\MSBuild\Current\Bin\MSBuild.exe",
    "F:\VS 2026\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
)

$msbuild = $msbuildCandidates | Where-Object {
    (Test-Path $_) -and (
        (Test-Path (Join-Path (Split-Path $_) "Roslyn\Microsoft.CSharp.Core.targets")) -or
        ($_ -like "*Framework64*")
    )
} | Select-Object -First 1
if (-not $msbuild) {
    throw "MSBuild.exe not found in expected Visual Studio paths."
}

$frameworkPath = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319"
if (-not (Test-Path $frameworkPath)) {
    throw ".NET Framework 4.8 runtime assemblies not found at $frameworkPath"
}

Write-Host "=== Building ClassicGTAOverhaul ($Configuration | x64) ===" -ForegroundColor Cyan
Write-Host "MSBuild: $msbuild"

& $msbuild "$RepoRoot\ClassicGTAOverhaul.sln" `
    /t:Rebuild `
    /p:Configuration=$Configuration `
    /p:Platform=x64 `
    /p:FrameworkPathOverride="$frameworkPath" `
    /p:TreatWarningsAsErrors=true `
    /verbosity:minimal

if ($LASTEXITCODE -ne 0) {
    throw "MSBuild failed with exit code $LASTEXITCODE"
}

if (-not $SkipTests) {
    $testExe = Join-Path $RepoRoot "tests\ClassicGTA.Tests\bin\$Configuration\ClassicGTA.Tests.exe"
    if (-not (Test-Path $testExe)) {
        throw "Test runner not found at $testExe"
    }

    Write-Host ""
    Write-Host "=== Running ClassicGTA.Tests ===" -ForegroundColor Cyan
    & $testExe
    if ($LASTEXITCODE -ne 0) {
        throw "Tests failed with exit code $LASTEXITCODE"
    }
}

Write-Host ""
Write-Host "=== BUILD & TESTS GREEN ===" -ForegroundColor Green
