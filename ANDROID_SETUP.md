# KYOKA — Android Setup & Deployment Guide

This guide details the complete Android setup, requirements, build settings, permissions, touchscreen control layout, and installation process.

---

## 1. Android Build Requirements
- **Unity Module**: Android Build Support (installed via Unity Hub).
- **Android SDK & NDK**: Installed automatically with Unity Hub or via Android Studio:
  - Minimum API Level: **Android 7.0 'Nougat' (API Level 24)**.
  - Target API Level: **Android 14 (API Level 34)**.
  - Scripting Backend: **IL2CPP**.
  - Target Architectures: **ARM64 (arm64-v8a)**.
  - Screen Orientation: **Landscape Left & Landscape Right**.

---

## 2. Android Touchscreen Controls Layout

```
+-------------------------------------------------------------------------+
| [KYOKA LOGO]  [MODES] [REGIONS] [INVENTORY] [SETTINGS] [SAVE] [MENU]     |
|                                                                         |
|                                                                         |
|                                                                         |
|       (CAM 1P/3P)                                                       |
|                                                                         |
|                                                                         |
|                                                 [ABILITY 2]             |
|                                                                         |
|     +--------+                         [ABILITY 1]       [SPRINT]       |
|     |   /\   |                                                          |
|     |  /  \  |                                                          |
|     |< MOVE >|                 [INTERACT]       [ATTACK]       [JUMP]   |
|     |  \  /  |                                                          |
|     |   \/   |                                                          |
|     +--------+                                                          |
+-------------------------------------------------------------------------+
```

### Controls Operation:
- **Left Side Analog Virtual Joystick**: Drag finger anywhere in the bottom-left quadrant to smoothly control player walking/running direction.
- **Right Side Camera Orbit**: Drag finger anywhere in the right half of the screen (away from buttons) to smoothly rotate camera pitch and yaw.
- **Action Buttons**:
  - `JUMP`: Jump upwards or swim upwards when submerged.
  - `SPRINT`: Toggles high-speed sprinting on/off.
  - `ATTACK`: Triggers weapon melee combo or fires ranged weapon.
  - `ABILITY 1`: Casts equipped primary power (e.g. Tempest Dash).
  - `ABILITY 2`: Casts equipped secondary power (e.g. Flame Surge).
  - `INTERACT`: Boards nearby vehicles or opens supply loot crates.
  - `CAM 1P/3P`: Toggles between first-person and third-person camera.

---

## 3. Required Android Permissions
Configured in AndroidManifest / PlayerSettings:
- `android.permission.INTERNET`: Required for LAN sockets, Relay, and backend connectivity.
- `android.permission.ACCESS_NETWORK_STATE`: Required to monitor network connectivity status.
- `android.permission.VIBRATE`: Optional haptic feedback for weapon impacts and ability firing.

---

## 4. Building & Deploying the APK

### Step 1: Build the APK
In Unity: Select `KYOKA > Build > Android APK (KYOKA.apk)`
Or in PowerShell:
```powershell
.\Development\BuildAndroid.ps1
```
Output is created at `Builds/Android/KYOKA.apk`.

### Step 2: Install via ADB
```powershell
.\Development\InstallAndroid.ps1 -ConfirmInstall
```

### Step 3: Manual Installation (Without ADB)
1. Copy `Builds/Android/KYOKA.apk` to your phone's storage via USB, Google Drive, or Bluetooth.
2. On your phone, open **Files / File Manager**.
3. Tap on `KYOKA.apk`.
4. If prompted, allow "Install Unknown Apps" for Files.
5. Tap **Install**, then tap **Open** to play KYOKA!

