@echo off
title KYOKA
echo Launching KYOKA...
if exist "KYOKA.exe" (
    start "" "KYOKA.exe"
) else (
    echo Error: KYOKA.exe not found in this directory.
    pause
)
