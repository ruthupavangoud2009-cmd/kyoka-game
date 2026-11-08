# KYOKA — Complete Setup Guide

This guide walks you through setting up, opening, developing, testing, and building KYOKA on Windows PC and Android.

---

## 1. Prerequisites
- **Operating System**: Windows 10 / 11 (64-bit).
- **Unity Engine**: Unity 6 (6000.0.30f1 or newer recommended).
  - Include **Windows Build Support (IL2CPP / Mono)**.
  - Include **Android Build Support** (with Android SDK & NDK Tools, OpenJDK).
- **Unity Hub**: Latest version from [unity.com](https://unity.com/download).
- **Git**: Installed from [git-scm.com](https://git-scm.com).
- **ADB (Android Debug Bridge)**: Included with Android SDK or Unity Android build tools.

---

## 2. Opening the Project
1. Launch **Unity Hub**.
2. Click the **Add** button (or dropdown: **Add project from disk**).
3. Browse to and select:
   `D:\bots updated\KYOKA_Full_Unity_Project\KYOKA_Full_Unity_Project`
4. Set the Editor Version to **Unity 6 (6000.0.x)**.
5. Click on the project name to open it.
6. Unity will resolve packages from `Packages/manifest.json`.

---

## 3. Running in Editor (Play Mode)
1. In the Project window, navigate to `Assets/KYOKA/Scenes/`.
2. Double-click `KYOKA_Bootstrap.unity` to open the scene.
3. Click the **Play** button at the top of the Unity Editor.
4. The game initializes automatically:
   - Terrain and landmarks spawn for KYOKA City.
   - Player capsule avatar spawns with first/third person camera.
   - 4 vehicle types spawn on the road (Car, Bike, Boat, Plane).
   - HUD renders health, aether, weapon ammo, and active mission.
   - Press **V** to switch between first-person and third-person view.
   - Press **E** near any vehicle to board and drive.
   - Press **Q** for Wind Dash, **F** for Fire Surge, **R** for Thunder Strike, **T** for Awakening.
   - Click **Save** in the top HUD to persist player state to disk.

---

## 4. Building the Game

### Building for Windows PC
Inside Unity Editor:
- Go to menu: `KYOKA > Build > Windows (KYOKA.exe)`
Or via PowerShell:
```powershell
.\Development\BuildWindows.ps1
```
Output: `Builds/Windows/KYOKA.exe`

### Building for Android (APK)
Inside Unity Editor:
- Go to menu: `KYOKA > Build > Android APK (KYOKA.apk)`
Or via PowerShell:
```powershell
.\Development\BuildAndroid.ps1
```
Output: `Builds/Android/KYOKA.apk`

---

## 5. Installing on Android Phone
1. Connect your Android phone to PC via USB cable.
2. Ensure **Developer Options** and **USB Debugging** are enabled on your phone.
3. Run the installer script:
```powershell
.\Development\InstallAndroid.ps1 -ConfirmInstall
```
4. The script detects your phone, checks ARM64 architecture, installs `KYOKA.apk`, and launches the game directly!

