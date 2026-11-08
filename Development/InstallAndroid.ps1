<#
.SYNOPSIS
    KYOKA Android Deployment & Device Installation Tool
    Detects connected Android devices via ADB, checks ABI compatibility,
    prompts for explicit authorization, and installs KYOKA.apk to the device.
#>

param(
    [string]$AdbPath = "",
    [switch]$ConfirmInstall = $false
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "          KYOKA ANDROID DEPLOYMENT TOOL (ADB)             " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$projectDir = Resolve-Path "$PSScriptRoot\.."
$apkPath = "$projectDir\Builds\Android\KYOKA.apk"

# 1. Locate ADB
if (-not $AdbPath) {
    $adbCandidates = @(
        "D:\Iriun Webcam\adb.exe",
        "$env:LOCALAPPDATA\Android\Sdk\platform-tools\adb.exe",
        "C:\Program Files\Android\Android Studio\platform-tools\adb.exe"
    )
    foreach ($c in $adbCandidates) {
        if (Test-Path $c) { $AdbPath = $c; break }
    }
}

if (-not $AdbPath -or -not (Test-Path $AdbPath)) {
    Write-Host "`n[ERROR] ADB executable was not found!" -ForegroundColor Red
    Write-Host "You can manually install the APK by copying KYOKA.apk to your phone and tapping it in File Manager.`n" -ForegroundColor Yellow
    exit 1
}

Write-Host "Using ADB at: $AdbPath" -ForegroundColor Green

# 2. Check Connected Devices
Write-Host "`nDetecting connected Android devices..." -ForegroundColor White
$deviceOutput = & "$AdbPath" devices
$lines = $deviceOutput | Where-Object { $_ -match "\tdevice$" }

if (-not $lines) {
    Write-Host "[WARNING] No authorized Android devices detected." -ForegroundColor Yellow
    Write-Host "Please ensure:" -ForegroundColor DarkYellow
    Write-Host " 1. Your Android phone is connected via USB cable or wireless ADB."
    Write-Host " 2. Developer Options -> USB Debugging is ENABLED on your phone."
    Write-Host " 3. You tapped 'Allow USB Debugging' on your phone prompt."
    Write-Host "`nManual Installation Option:" -ForegroundColor Cyan
    Write-Host " Copy 'Builds/Android/KYOKA.apk' to your phone's Internal Storage/Downloads and install it via Files app.`n"
    exit 0
}

Write-Host "Found connected device(s):" -ForegroundColor Green
$lines | ForEach-Object { Write-Host " -> $_" -ForegroundColor Green }

# 3. Check Device Architecture & Compatibility
$deviceAbi = (& "$AdbPath" shell getprop ro.product.cpu.abi).Trim()
$deviceModel = (& "$AdbPath" shell getprop ro.product.model).Trim()
$androidVer = (& "$AdbPath" shell getprop ro.build.version.release).Trim()
$sdkVer = (& "$AdbPath" shell getprop ro.build.version.sdk).Trim()

Write-Host "`nDevice Info:" -ForegroundColor White
Write-Host " Model: $deviceModel"
Write-Host " Android Version: $androidVer (API $sdkVer)"
Write-Host " CPU Architecture: $deviceAbi"

if ($deviceAbi -notmatch "arm64") {
    Write-Host "[WARNING] Device ABI is '$deviceAbi'. KYOKA is built for 'arm64-v8a'." -ForegroundColor Yellow
}

# 4. Check APK
if (-not (Test-Path $apkPath)) {
    Write-Host "`n[ERROR] $apkPath not found!" -ForegroundColor Red
    Write-Host "Please run 'Development/BuildAndroid.ps1' or build in Unity Editor first.`n" -ForegroundColor Yellow
    exit 1
}

# 5. Authorization Check
if (-not $ConfirmInstall) {
    Write-Host "`n----------------------------------------------------------" -ForegroundColor Yellow
    Write-Host "AUTHORIZATION REQUIRED: Install KYOKA (com.kyoka.game) onto $deviceModel?" -ForegroundColor Yellow
    Write-Host "Run with -ConfirmInstall to proceed: .\InstallAndroid.ps1 -ConfirmInstall" -ForegroundColor Yellow
    Write-Host "----------------------------------------------------------`n"
    exit 0
}

# 6. Install
Write-Host "`nInstalling KYOKA.apk to $deviceModel..." -ForegroundColor Cyan
$installResult = & "$AdbPath" install -r "$apkPath"

if ($installResult -match "Success") {
    Write-Host "[SUCCESS] KYOKA installed successfully on $deviceModel!" -ForegroundColor Green
    Write-Host "Launching KYOKA on device..." -ForegroundColor Cyan
    & "$AdbPath" shell am start -n com.kyoka.game/com.unity3d.player.UnityPlayerActivity | Out-Null
    Write-Host "Game launched!`n" -ForegroundColor Green
} else {
    Write-Host "[FAILED] Installation failed: $installResult" -ForegroundColor Red
}
