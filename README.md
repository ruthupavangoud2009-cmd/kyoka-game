# KYOKA — ULTIMATE MASTER PROJECT

**KYOKA** is an original multiplayer open-world action game combining elements of urban city life, battle royale combat, sandbox crafting, vehicle traversal, and anime-inspired elemental powers. Designed from the ground up for Windows PC and Android mobile devices, with iOS architecture compatibility.

---

## Quick Start Guide

### 1. Requirements
- **Unity 6** (6000.0.30f1 or newer recommended).
- **Universal Render Pipeline (URP)**.
- **Unity Input System**.
- **Android Build Support** (with Android NDK & OpenJDK) for Android APK builds.
- **Windows Standalone Build Support** for Windows PC builds.

### 2. Opening in Unity
1. Open **Unity Hub**.
2. Click **Add > Add project from disk**.
3. Select this folder: `KYOKA_Full_Unity_Project`.
4. Open the project with **Unity 6**.
5. Packages (`manifest.json`) will be imported automatically.
6. Open the startup scene: `Assets/KYOKA/Scenes/KYOKA_Bootstrap.unity`.
7. Press **Play** in the Unity Editor to play immediately!

---

## Six Major World Regions
1. **KYOKA City**: Urban high-rises, commercial district, central plaza, neon street signs, vehicle garage, and NPC citizens.
2. **Wildlands**: Dense pine forests, mountain ranges, harvestable timber and stone nodes, and survival exploration.
3. **Ocean Realm**: Vast ocean waters, central coral archipelago, ocean docks, lighthouse, and boat navigation.
4. **Anime Realm**: Floating celestial sky island, 4 elemental shrines (Fire, Water, Wind, Lightning), and celestial boss arena.
5. **Battle Islands**: Dedicated Battle Royale map with military bunker, watchtowers, supply depots, shrinking safe zone, and loot crates.
6. **Creative World**: Sandbox construction arena with building permissions, coordinate pillars, and custom block saving.

---

## Six Game Modes
- **Open World**: Freely traverse all 6 regions, drive vehicles, accept NPC missions, and trade.
- **Battle Royale**: Solo/squad parachute drop, loot collection, shrinking storm circle, and match ranking.
- **Anime RPG**: Quests, elemental abilities, energy costs, and boss fights.
- **Survival**: Harvest natural resources, craft consumables, and manage durability.
- **Creative**: Unlimited block placement, blueprint saving, and sandbox experimentation.
- **Roleplay**: City jobs, citizen interactions, and wanted/police response system.

---

## Controls Reference

### PC Keyboard & Mouse
| Action | Key / Input |
|---|---|
| Move | `W` `A` `S` `D` |
| Look / Aim | Mouse Move |
| Sprint | `Left Shift` |
| Jump | `Space` |
| Crouch | `C` |
| Attack / Fire | `Left Mouse Button` |
| Guard / Defend | `Right Mouse Button` |
| Primary Ability (Wind Dash) | `Q` |
| Secondary Ability (Fire Surge) | `F` |
| Ultimate Ability (Thunder Strike) | `R` |
| Awakening Transformation | `T` |
| Interact / Vehicle Enter-Exit | `E` |
| Camera Switch (1st / 3rd Person) | `V` |
| Creative Block Place | `B` |
| Hotbar Slots | `1` - `5` |
| Inventory | `Tab` / `I` |
| World Map & Teleport | `M` |
| Pause / Settings | `Escape` |

### Android Mobile Touchscreen
- **Left Side**: Floating virtual joystick for analog movement.
- **Right Side**: Touch swipe for camera orbit and dedicated on-screen touch buttons:
  - `JUMP`, `SPRINT`, `ATTACK`, `ABILITY 1`, `ABILITY 2`, `INTERACT`, `CAM 1P/3P`.
- **Top Bar**: Mode selector, Region map, Inventory, Settings, and Save.

---

## Automated Build & Validation Scripts

Run these from PowerShell in the project directory:

```powershell
# Validate project integrity
.\Development\ValidateProject.ps1

# Build Windows PC Standalone (Builds/Windows/KYOKA.exe)
.\Development\BuildWindows.ps1

# Build Android ARM64 APK (Builds/Android/KYOKA.apk)
.\Development\BuildAndroid.ps1

# Install APK to connected Android phone via ADB
.\Development\InstallAndroid.ps1 -ConfirmInstall
```

Also accessible inside Unity Editor from the top menu:
- `KYOKA > Build > Windows (KYOKA.exe)`
- `KYOKA > Build > Android APK (KYOKA.apk)`
- `KYOKA > Build > Validate Project`
- `KYOKA > Build > Open Build Folder`

