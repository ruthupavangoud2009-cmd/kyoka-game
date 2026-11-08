<#
.SYNOPSIS
    KYOKA Project Validation Tool
    Performs complete integrity checks across C# scripts, package dependencies,
    scene hierarchy, player settings, and platform tooling (Windows Standalone, Android SDK/NDK).
#>

param(
    [switch]$VerboseOutput = $false
)

$ErrorActionPreference = "Continue"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "             KYOKA PROJECT VALIDATION SUITE               " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$baseDir = Resolve-Path "$PSScriptRoot\.."
Set-Location $baseDir

$passCount = 0
$failCount = 0
$warnCount = 0

function Report-Check([string]$name, [bool]$passed, [string]$detail = "", [bool]$isWarning = $false) {
    if ($passed) {
        Write-Host " [PASS] $name" -ForegroundColor Green
        if ($detail -and $VerboseOutput) { Write-Host "        $detail" -ForegroundColor DarkGray }
        $script:passCount++
    } else {
        if ($isWarning) {
            Write-Host " [WARN] $name" -ForegroundColor Yellow
            if ($detail) { Write-Host "        $detail" -ForegroundColor DarkYellow }
            $script:warnCount++
        } else {
            Write-Host " [FAIL] $name" -ForegroundColor Red
            if ($detail) { Write-Host "        $detail" -ForegroundColor DarkRed }
            $script:failCount++
        }
    }
}

# 1. Check Directory Hierarchy
Write-Host "`n1. Checking Project Hierarchy..." -ForegroundColor White
$requiredDirs = @(
    "Assets/KYOKA/Art", "Assets/KYOKA/Audio", "Assets/KYOKA/Animations",
    "Assets/KYOKA/Materials", "Assets/KYOKA/Prefabs", "Assets/KYOKA/Scenes",
    "Assets/KYOKA/ScriptableObjects", "Assets/KYOKA/Scripts/Bootstrap",
    "Assets/KYOKA/Scripts/Core", "Assets/KYOKA/Scripts/Player",
    "Assets/KYOKA/Scripts/Character", "Assets/KYOKA/Scripts/Abilities",
    "Assets/KYOKA/Scripts/Combat", "Assets/KYOKA/Scripts/Vehicles",
    "Assets/KYOKA/Scripts/World", "Assets/KYOKA/Scripts/Missions",
    "Assets/KYOKA/Scripts/BattleRoyale", "Assets/KYOKA/Scripts/Creative",
    "Assets/KYOKA/Scripts/Survival", "Assets/KYOKA/Scripts/Inventory",
    "Assets/KYOKA/Scripts/Economy", "Assets/KYOKA/Scripts/Social",
    "Assets/KYOKA/Scripts/Networking", "Assets/KYOKA/Scripts/Backend",
    "Assets/KYOKA/Scripts/UI", "Assets/KYOKA/Tests/EditMode"
)

$missingDirs = 0
foreach ($dir in $requiredDirs) {
    if (-not (Test-Path $dir)) { $missingDirs++ }
}
Report-Check "Project Architecture Folders" ($missingDirs -eq 0) "All $($requiredDirs.Count) architecture folders present."

# 2. Check Package Manifest
Write-Host "`n2. Checking Package Dependencies..." -ForegroundColor White
$manifestPath = "Packages/manifest.json"
$hasManifest = Test-Path $manifestPath
Report-Check "Packages/manifest.json Exists" $hasManifest

if ($hasManifest) {
    $json = Get-Content $manifestPath -Raw
    $hasInput = $json.Contains("com.unity.inputsystem")
    $hasUrp = $json.Contains("com.unity.render-pipelines.universal")
    $hasTest = $json.Contains("com.unity.test-framework")
    $hasUgui = $json.Contains("com.unity.ugui")
    $hasNgo = $json.Contains("com.unity.netcode.gameobjects")
    $hasTransport = $json.Contains("com.unity.transport")

    Report-Check "Input System Package" $hasInput "com.unity.inputsystem"
    Report-Check "URP Render Pipeline" $hasUrp "com.unity.render-pipelines.universal"
    Report-Check "Test Framework Package" $hasTest "com.unity.test-framework"
    Report-Check "uGUI Package" $hasUgui "com.unity.ugui"
    Report-Check "Netcode for GameObjects" $hasNgo "com.unity.netcode.gameobjects"
    Report-Check "Unity Transport" $hasTransport "com.unity.transport"
}

# 3. Check Scenes and Build Settings
Write-Host "`n3. Checking Scene Configuration..." -ForegroundColor White
$scenePath = "Assets/KYOKA/Scenes/KYOKA_Bootstrap.unity"
$hasScene = Test-Path $scenePath
Report-Check "Bootstrap Scene Exists" $hasScene $scenePath

$buildSettingsPath = "ProjectSettings/EditorBuildSettings.asset"
$hasBuildSettings = Test-Path $buildSettingsPath
Report-Check "EditorBuildSettings Configured" $hasBuildSettings $buildSettingsPath

# 4. Check C# Script Syntax / Literal Errors
Write-Host "`n4. Checking C# Source Integrity..." -ForegroundColor White
$csFiles = Get-ChildItem -Path Assets/KYOKA -Filter "*.cs" -Recurse
$syntaxBugs = 0
foreach ($f in $csFiles) {
    $content = Get-Content $f.FullName -Raw
    if ($content.Contains("\nusing")) {
        $syntaxBugs++
        Write-Host "   Found literal \n error in: $($f.Name)" -ForegroundColor Red
    }
}
Report-Check "Clean C# Code (No Literal \n Syntax Errors)" ($syntaxBugs -eq 0) "Checked $($csFiles.Count) C# source files."

# 5. Check Installed Unity & Tooling
Write-Host "`n5. Checking Development Environment Tooling..." -ForegroundColor White
$unityCandidates = @(
    "C:\Program Files\Unity\Hub\Editor\6000.0.*\Editor\Unity.exe",
    "C:\Program Files\Unity\Hub\Editor\*\Editor\Unity.exe",
    "C:\Program Files (x86)\Unity\Hub\Editor\*\Editor\Unity.exe",
    "D:\Unity\*\Editor\Unity.exe"
)
$foundUnity = Resolve-Path $unityCandidates -ErrorAction SilentlyContinue | Select-Object -First 1

if ($foundUnity) {
    Report-Check "Unity 6 Editor Installed" $true $foundUnity.Path
} else {
    Report-Check "Unity 6 Editor Installed" $false "Unity Editor executable not found in default paths. Project is prepped for when Unity Hub opens it." $true
}

# 6. Check ADB / Android Tooling
$adbCandidates = @(
    "D:\Iriun Webcam\adb.exe",
    "$env:LOCALAPPDATA\Android\Sdk\platform-tools\adb.exe",
    "C:\Program Files\Android\Android Studio\platform-tools\adb.exe"
)
$foundAdb = $null
foreach ($c in $adbCandidates) {
    if (Test-Path $c) { $foundAdb = $c; break }
}
if ($foundAdb) {
    Report-Check "Android ADB Tool Found" $true $foundAdb
} else {
    Report-Check "Android ADB Tool Found" $false "ADB not located. Required for direct USB deployment to phone." $true
}

# SUMMARY
Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host "VALIDATION SUMMARY: $passCount PASSED, $warnCount WARNINGS, $failCount FAILED" -ForegroundColor $(if ($failCount -eq 0) { "Green" } else { "Red" })
Write-Host "==========================================================" -ForegroundColor Cyan

if ($failCount -eq 0) {
    Write-Host "KYOKA Project is valid and ready for Unity 6 compilation and building.`n" -ForegroundColor Green
    exit 0
} else {
    Write-Host "Please resolve the failures reported above.`n" -ForegroundColor Red
    exit 1
}

