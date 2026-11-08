@echo off
setlocal enabledelayedexpansion
title KYOKA - Android Deployment Launcher
color 0A

echo =======================================================================
echo                 KYOKA ANDROID DEPLOYMENT & LAUNCHER                   
echo =======================================================================
echo.

set "SCRIPT_DIR=%~dp0"
cd /d "%SCRIPT_DIR%"

:: Check if APK exists
if not exist "Builds\Android\KYOKA.apk" (
    echo [INFO] Builds\Android\KYOKA.apk not found.
    echo.
    echo To build the Android APK:
    echo  1. Open the project in Unity 6 with Android Build Support installed.
    echo  2. Use top menu: 'KYOKA > Build > Android APK' or run Development\BuildAndroid.ps1
    echo.
    echo Searching for connected Android devices anyway...
    echo.
)

:: Run Android Installer with confirmation
powershell -ExecutionPolicy Bypass -File "Development\InstallAndroid.ps1" -ConfirmInstall

echo.
pause
