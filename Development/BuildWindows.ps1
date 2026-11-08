<#
.SYNOPSIS
    KYOKA Windows PC Build Automation Script
    Builds the standalone 64-bit Windows executable (KYOKA.exe) via Unity batchmode.
#>

param(
    [string]$Configuration = "Release",
    [string]$UnityPath = "",
    [switch]$Force = $false
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "            KYOKA WINDOWS BUILD AUTOMATION               " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$projectDir = Resolve-Path "$PSScriptRoot\.."
Set-Location $projectDir

# 1. Locate Unity
if (-not $UnityPath) {
    $unityCandidates = @(
        "C:\Program Files\Unity\Hub\Editor\6000.0.*\Editor\Unity.exe",
        "C:\Program Files\Unity\Hub\Editor\*\Editor\Unity.exe",
        "C:\Program Files (x86)\Unity\Hub\Editor\*\Editor\Unity.exe",
        "D:\Unity\*\Editor\Unity.exe"
    )
    $found = Resolve-Path $unityCandidates -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($found) { $UnityPath = $found.Path }
}

if (-not $UnityPath -or -not (Test-Path $UnityPath)) {
    Write-Host "`n[ERROR] Unity Editor executable was not found!" -ForegroundColor Red
    Write-Host "Please install Unity 6 (6000.0.x) via Unity Hub or specify -UnityPath 'path\to\Unity.exe'`n" -ForegroundColor Yellow
    exit 1
}

Write-Host "Using Unity Editor at: $UnityPath" -ForegroundColor Green

# 2. Output Paths
$outputDir = "$projectDir\Builds\Windows"
$exePath = "$outputDir\KYOKA.exe"
$logFile = "$projectDir\Builds\Windows\build_windows.log"

if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

Write-Host "Building Configuration: $Configuration" -ForegroundColor White
Write-Host "Target Binary: $exePath" -ForegroundColor White
Write-Host "Unity Log: $logFile`n" -ForegroundColor DarkGray

# 3. Execute Unity Batchmode Build
$buildStartTime = Get-Date

$args = @(
    "-quit",
    "-batchmode",
    "-projectPath", "`"$projectDir`"",
    "-executeMethod", "Kyoka.Editor.KyokaBuildPipeline.BuildWindows",
    "-logFile", "`"$logFile`""
)

Write-Host "Executing Unity batchmode compiler..." -ForegroundColor Cyan
$process = Start-Process -FilePath $UnityPath -ArgumentList $args -NoNewWindow -PassThru -Wait

$buildDuration = (Get-Date) - $buildStartTime

if ($process.ExitCode -eq 0 -and (Test-Path $exePath)) {
    Write-Host "`n[SUCCESS] Windows build generated successfully in $($buildDuration.TotalSeconds.ToString("F1"))s!" -ForegroundColor Green
    Write-Host "Executable: $exePath" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`n[FAILED] Windows build failed with exit code $($process.ExitCode)." -ForegroundColor Red
    if (Test-Path $logFile) {
        Write-Host "Last 20 lines of build log:" -ForegroundColor Yellow
        Get-Content $logFile -Tail 20
    }
    exit 1
}

