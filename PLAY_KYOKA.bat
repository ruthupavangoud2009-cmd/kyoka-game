@echo off
setlocal enabledelayedexpansion
title KYOKA - Launch Game
color 0B

echo =======================================================================
echo                         KYOKA ONE-CLICK LAUNCHER                       
echo =======================================================================
echo.

set "SCRIPT_DIR=%~dp0"
cd /d "%SCRIPT_DIR%"

:: Check if KYOKA.exe exists in local directory or in Builds\Windows\
if exist "KYOKA.exe" (
    echo [OK] Found KYOKA executable in current directory.
    echo Launching KYOKA...
    start "" "KYOKA.exe"
    exit /b 0
)

if exist "Builds\Windows\KYOKA.exe" (
    echo [OK] Found KYOKA standalone build at Builds\Windows\KYOKA.exe.
    echo Launching KYOKA...
    start "" "Builds\Windows\KYOKA.exe"
    exit /b 0
)

:: If build not found, detect environment and provide clear instructions
echo [INFO] Pre-compiled Windows build not found at:
echo        Builds\Windows\KYOKA.exe
echo.

:: Check if Unity 6 is installed on this PC
set "UNITY_FOUND="
for /d %%U in ("C:\Program Files\Unity\Hub\Editor\6000.*") do (
    if exist "%%U\Editor\Unity.exe" (
        set "UNITY_FOUND=%%U\Editor\Unity.exe"
    )
)
if not defined UNITY_FOUND (
    for /d %%U in ("C:\Program Files\Unity\Hub\Editor\*") do (
        if exist "%%U\Editor\Unity.exe" (
            set "UNITY_FOUND=%%U\Editor\Unity.exe"
        )
    )
)

if defined UNITY_FOUND (
    echo [DETECTED] Unity Editor found at:
    echo            !UNITY_FOUND!
    echo.
    echo Would you like to automatically compile and build KYOKA now?
    set /p BUILD_CHOICE="Enter 'Y' to build, or 'N' to cancel [Y/N]: "
    if /i "!BUILD_CHOICE!"=="Y" (
        echo.
        echo Starting build process...
        call BUILD_KYOKA.bat
        if exist "Builds\Windows\KYOKA.exe" (
            echo.
            echo [SUCCESS] Build complete! Launching KYOKA...
            start "" "Builds\Windows\KYOKA.exe"
            exit /b 0
        )
    )
    goto :END
)

:: Unity not detected
echo [REQUIRED] To build KYOKA from source:
echo  1. Install Unity 6 (6000.0.x) from: https://unity.com/download
echo  2. Open Unity Hub, click 'Add > Add project from disk', and select this folder.
echo  3. Run BUILD_KYOKA.bat or use menu 'KYOKA > Build > Windows' inside Unity.
echo.
echo [DOWNLOAD PRE-BUILT] Pre-compiled binaries can also be downloaded from:
echo  https://github.com/ruthupavangoud2009-cmd/kyoka-game/releases
echo.

:END
echo Press any key to exit launcher...
pause >nul
