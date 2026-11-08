# KYOKA — Known Issues & Limitations

This document tracks all acknowledged technical limitations, environmental constraints, and architectural boundaries in accordance with project safety guidelines.

---

## 1. Environment & Build Constraints
- **Host Unity Installation**: Unity 6 Editor executable is not currently installed in standard system paths on this machine (`C:\Program Files\Unity\Hub\Editor\`, etc.). The project files, scenes, and automated build scripts (`BuildWindows.ps1`, `BuildAndroid.ps1`) are fully prepped and validated. When opened on a machine with Unity 6 installed, builds can be generated immediately.
- **Android Device Connection**: No physical Android device was detected during `adb devices` scan. The deployment script `Development/InstallAndroid.ps1` successfully locates ADB at `D:\Iriun Webcam\adb.exe` and is ready to deploy as soon as a device is plugged in with USB Debugging enabled.

## 2. Multiplayer Architecture Boundaries
- **Zero-Budget Starting Constraint**: As mandated by project rules (₹0 budget), no paid cloud hosting (AWS, Multiplay, Azure, or paid UGS tiers) has been enabled.
- **LAN Socket Mode**: The real networking implementation operates over local TCP sockets (Host & Client) on port 7777, enabling two devices on the same Wi-Fi network to test multiplayer without cloud cost.
- **Relay & Lobby**: UGS Relay allocation methods are implemented as integration stubs ready to activate once a project ID is linked in Unity Dashboard.
- **No Client Authority**: In production competitive matches, physics and transaction validation must run exclusively on dedicated headless servers (`-batchmode -nographics`).

## 3. Visuals & Art Assets
- **Procedural Geometry**: Current environment landmarks, towers, skyscrapers, vehicles, and block geometry are constructed using procedural primitives with color materials. Production 3D meshes created in Blender/Krita should be imported into `Assets/KYOKA/Art/` to replace placeholder models.
- **Anime Shaders**: Shaders currently utilize Universal Render Pipeline Lit/Unlit materials. Anime cel-shading can be layered via URP custom renderer features.

## 4. Input System Configuration
- Both Unity Input System and legacy Input Manager are enabled via `activeInputHandler: 2` in `ProjectSettings.asset`. `KyokaInputBridge.cs` unifies all input events so that gameplay logic does not rely directly on raw keyboard polling.

