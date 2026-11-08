# KYOKA — Windows PC Build Guide

Instructions for configuring, generating, running, and verifying the Windows PC 64-bit standalone build of KYOKA.

---

## 1. Specifications
- **Platform**: Windows Standalone (x86_64).
- **Target Executable**: `Builds/Windows/KYOKA.exe`.
- **Engine**: Unity 6 (6000.0.x).
- **Render Pipeline**: Universal Render Pipeline (URP).
- **Default Resolution**: 1920x1080 Fullscreen / Windowed toggleable.
- **Controls**: Full Keyboard & Mouse with mouse lock/unlock support.

---

## 2. Generating the Windows Build

### Option A: From Unity Editor Menu
1. Open the project in Unity 6.
2. Click **KYOKA** in the top menu bar.
3. Click **Build > Windows (KYOKA.exe)**.
4. Unity will compile all C# assemblies, shaders, and scenes into `Builds/Windows/`.

### Option B: Automated via PowerShell
Run from PowerShell in the project directory:
```powershell
.\Development\BuildWindows.ps1 -Configuration "Release"
```
Or for development builds with profiling:
```powershell
.\Development\BuildWindows.ps1 -Configuration "Development"
```

---

## 3. Running & Verifying the Build
1. Navigate to `Builds/Windows/`.
2. Double-click `KYOKA.exe`.
3. The game starts directly in `KYOKA_Bootstrap` scene:
   - Full graphics and audio initialize.
   - Mouse cursor locks to center for look control.
   - Press **Escape** anytime to unlock the cursor and access the pause menu.
   - Press **Save** to verify that JSON save data is written to `%USERPROFILE%\AppData\LocalLow\KYOKA Studios\KYOKA\kyoka_save.json`.

