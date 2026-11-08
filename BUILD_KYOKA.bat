@echo off
setlocal enabledelayedexpansion
title KYOKA - Windows Build Automation
color 0E

echo =======================================================================
echo                    KYOKA AUTOMATED BUILD SYSTEM (WINDOWS)              
echo =======================================================================
echo.

set "SCRIPT_DIR=%~dp0"
cd /d "%SCRIPT_DIR%"

:: Step 1: Validate Project Integrity
echo [STEP 1/3] Running Project Validator...
powershell -ExecutionPolicy Bypass -File "Development\ValidateProject.ps1"
if %ERRORLEVEL% neq 0 (
    echo.
    echo [ERROR] Project validation failed! Please fix issues above before building.
    echo.
    pause
    exit /b 1
)

:: Step 2: Locate Unity Installation
echo.
echo [STEP 2/3] Checking Unity 6 Installation...

set "UNITY_PATH="
for /d %%U in ("C:\Program Files\Unity\Hub\Editor\6000.*") do (
    if exist "%%U\Editor\Unity.exe" (
        set "UNITY_PATH=%%U\Editor\Unity.exe"
    )
)
if not defined UNITY_PATH (
    for /d %%U in ("C:\Program Files\Unity\Hub\Editor\*") do (
        if exist "%%U\Editor\Unity.exe" (
            set "UNITY_PATH=%%U\Editor\Unity.exe"
        )
    )
)

if not defined UNITY_PATH (
    echo.
    echo [ERROR] Unity Editor was not found in standard paths!
    echo         Target Engine: Unity 6 (6000.0.x)
    echo.
    echo Please install Unity 6 via Unity Hub:
    echo   https://unity.com/download
    echo Or specify the custom path in Development\BuildWindows.ps1
    echo.
    pause
    exit /b 1
)

echo [OK] Using Unity at: !UNITY_PATH!
echo.

:: Step 3: Run Batchmode Build
echo [STEP 3/3] Compiling standalone Windows 64-bit build...
powershell -ExecutionPolicy Bypass -File "Development\BuildWindows.ps1" -UnityPath "!UNITY_PATH!"

if exist "Builds\Windows\KYOKA.exe" (
    echo.
    echo =======================================================================
    echo [SUCCESS] KYOKA.exe was successfully built!
    echo Location: Builds\Windows\KYOKA.exe
    echo You can now double-click PLAY_KYOKA.bat to play!
    echo =======================================================================
    echo.
) else (
    echo.
    echo [ERROR] Build command completed but Builds\Windows\KYOKA.exe was not created.
    echo Please inspect Builds\Windows\build_windows.log for compilation details.
    echo.
)

pause
