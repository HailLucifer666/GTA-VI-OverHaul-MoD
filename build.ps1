param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

# 1. Reproducible E:\-only package restore (zero writes to C:\)
$pkgDir = Join-Path $RepoRoot ".packages"
New-Item -ItemType Directory -Path $pkgDir -Force | Out-Null

function Ensure-NuGetPackage($id, $version, $expectedDllRelative) {
    $destDir = Join-Path $pkgDir "$id.$version"
    $expectedDll = Join-Path $destDir $expectedDllRelative
    if (-not (Test-Path $expectedDll)) {
        $lower = $id.ToLowerInvariant()
        $url = "https://api.nuget.org/v3-flatcontainer/$lower/$version/$lower.$version.nupkg"
        $zipPath = Join-Path $pkgDir "$id.$version.zip"
        Write-Host "Restoring $id $version into .\.packages\ ..." -ForegroundColor Yellow
        Invoke-WebRequest -Uri $url -OutFile $zipPath -UseBasicParsing
        Expand-Archive -Path $zipPath -DestinationPath $destDir -Force
        Remove-Item $zipPath -Force
    }
}

Ensure-NuGetPackage "ScriptHookVDotNet3" "3.6.0" "lib\net48\ScriptHookVDotNet3.dll"
Ensure-NuGetPackage "LemonUI.SHVDN3" "2.2.0" "lib\net48\LemonUI.SHVDN3.dll"

# 2. Locate Roslyn-capable MSBuild (honors MSBUILD_PATH override, vswhere, or known VS installs)
$msbuildCandidates = @()
if ($env:MSBUILD_PATH) { $msbuildCandidates += $env:MSBUILD_PATH }

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
if (Test-Path $vswhere) {
    $found = & $vswhere -latest -prerelease -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" 2>$null
    if ($found) { $msbuildCandidates += $found }
}

$msbuildCandidates += @(
    "C:\Program Files\Microsoft Visual Studio\18\Insiders\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
    "F:\VS 2026\MSBuild\Current\Bin\MSBuild.exe"
)

$msbuild = $msbuildCandidates | Where-Object {
    $_ -and (Test-Path $_) -and (Test-Path (Join-Path (Split-Path $_) "Roslyn\Microsoft.CSharp.Core.targets"))
} | Select-Object -First 1

if (-not $msbuild) {
    throw "Roslyn-capable MSBuild.exe not found. Install Visual Studio Community (.NET desktop workload) or set MSBUILD_PATH."
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
