# KYOKA — ULTIMATE MASTER PROJECT

[![KYOKA GitHub Repository](https://img.shields.io/badge/GitHub-kyoka--game-blue.svg)](https://github.com/ruthupavangoud2009-cmd/kyoka-game)
[![Engine](https://img.shields.io/badge/Engine-Unity%206-black.svg)](https://unity.com/)
[![Render Pipeline](https://img.shields.io/badge/Render%20Pipeline-URP-orange.svg)](https://unity.com/srp/Universal-Render-Pipeline)
[![Platforms](https://img.shields.io/badge/Platforms-Windows%20%7C%20Android-green.svg)]()
[![License](https://img.shields.io/badge/Budget-₹0%20(Free%20Tier)-brightgreen.svg)]()

**KYOKA** is an original multiplayer open-world action game combining elements of urban city life, battle royale combat, sandbox crafting, multi-class vehicles, and anime-inspired elemental powers. Designed from the ground up for Windows PC and Android mobile devices, with iOS architecture compatibility.

---

## ⚡ ONE-CLICK PLAY (HIGHEST PRIORITY)

### Option 1: Play Immediately
1. Clone or download this repository:
   ```bash
   git clone https://github.com/ruthupavangoud2009-cmd/kyoka-game.git
   cd kyoka-game
   ```
2. **Double-click `PLAY_KYOKA.bat` in the root folder!**
   - If the pre-compiled binary (`Builds/Windows/KYOKA.exe`) exists, the game launches instantly.
   - If Unity 6 is installed on your PC, the launcher offers to compile the build automatically with one keystroke (`Y`).
   - If Unity is not yet installed, clear setup instructions are displayed.

---

## 🛠️ ONE-CLICK BUILD & TOOLS

Three automated batch tools are provided directly in the root directory:

| Launcher File | Purpose | What it Does |
|---|---|---|
| **`PLAY_KYOKA.bat`** | **One-Click Play** | Detects standalone Windows build and launches `KYOKA.exe` immediately. |
| **`BUILD_KYOKA.bat`** | **One-Click Build** | Runs project validator, detects Unity 6, compiles Windows standalone build via batchmode, and generates `Builds/Windows/KYOKA.exe`. |
| **`PLAY_KYOKA_ANDROID.bat`** | **Android Deployer** | Detects connected Android phone via ADB, validates ARM64 compatibility, installs `Builds/Android/KYOKA.apk`, and launches the game on device. |

---

## 🗺️ The Six Major World Regions

KYOKA features 6 diverse regions coordinated by `KyokaWorldManager` and procedurally populated by `KyokaWorldBootstrap`:

1. **KYOKA City**: Urban district with high-rises, neon billboards, street road network, traffic AI, vehicle garage, shops, and NPC citizens.
2. **Wildlands**: Dense pine forests, mountain ranges, harvestable timber and stone nodes, and survival exploration.
3. **Ocean Realm**: Vast ocean waters, coral archipelago, lighthouse, docks, and boat traversal.
4. **Anime Realm**: Floating celestial sky island, 4 elemental shrines, and the "Celestial Sovereign" boss arena.
5. **Battle Islands**: Dedicated Battle Royale combat map, military bunkers, watchtowers, supply depots, shrinking safe zone, and loot crates.
6. **Creative World**: Clean 3D construction grid arena, coordinate pillars, building permissions, and custom block palette.

---

## 🎮 Complete Gameplay Systems

- **Player Traversal**: Walk, run, sprint, jump, crouch, swim, and climb.
- **Dual Camera**: First-person and third-person camera switching (`V` on PC or touch button on Android) with obstacle collision spherecast.
- **Character Customization**: Anime/realistic hybrid styling with customizable hair, eye color, skin tone, and outfit palettes.
- **Combat & Arsenal**: 3-hit melee combo string with finisher scaling, ranged firearm shooting with projectile physics and ammo reloading, guard stance reducing damage by 65%.
- **Anime Elemental Powers (5 Elements)**:
  - *Tempest Dash (Wind)*: Forward dash with slashing wind blades.
  - *Flame Surge (Fire)*: Explosive fireball projectile.
  - *Thunder Strike (Lightning)*: Ground lightning strike pillar.
  - *Void Barrier (Void)*: Cosmic energy shield.
  - *Tide Blossom (Water)*: Healing spring area-of-effect.
  - *Kyoka Awakening (Transformation)*: Supercharged state boosting speed by 50% and damage by 75% with a glowing aura.
- **Vehicles**: Real physics for Cars, Motorcycles, Boats, and Aircraft with passenger seating, health, collision damage, and fuel.
- **Economy & Survival**: Kyoka Credits and Aether Shards with anti-exploit validation, shops, and harvest-to-craft loop.
- **Missions & NPCs**: Story quests, side trials, daily bounties, NPC dialogues, and 0-5 stars wanted law enforcement system.
- **Audio Synthesis**: Built-in procedural audio engine providing SFX and regional ambient musical themes at ₹0 cost.
- **Social & Party**: Player profiles, party invites, multi-channel chat (Global, Party, Local), and player moderation.
- **Battle Royale**: Safe zone circle shrinking, storm damage, alive counter, and match rank rewards.
- **Creative Mode**: Block placement, color palette, and blueprint saving to JSON.

---

## ⌨️ Controls Reference

### PC Keyboard & Mouse
| Action | Key / Input |
|---|---|
| Move | `W` `A` `S` `D` |
| Look / Aim | Mouse Movement |
| Sprint | `Left Shift` |
| Jump | `Space` |
| Crouch | `C` |
| Attack / Shoot | `Left Mouse Button` |
| Guard / Defend | `Right Mouse Button` |
| Ability 1 (Wind Dash) | `Q` |
| Ability 2 (Fire Surge) | `F` |
| Ability 3 (Thunder Strike) | `R` |
| Awakening Transformation | `T` |
| Interact / Drive Vehicle | `E` |
| Toggle 1st / 3rd Person | `V` |
| Place Creative Block | `B` |
| Inventory | `Tab` / `I` |
| World Map & Teleport | `M` |
| Pause / Menu | `Escape` |

### Android Touchscreen Controls
- **Left Side**: Floating analog virtual joystick for 360° walking and running.
- **Right Side**: Swipe area for camera rotation orbit, plus dedicated on-screen action buttons:
  - `JUMP`, `SPRINT`, `ATTACK`, `ABILITY 1`, `ABILITY 2`, `INTERACT`, `CAM 1P/3P`.
- **Top Bar**: Mode selector, Region map, Inventory, Settings, and Save.

---

## 🌐 Real Multiplayer Architecture

- **Development LAN Mode**: Connect two devices (PC and Android, or two PCs) over local Wi-Fi / LAN using port `7777` without paid hosting fees.
- **Relay & Lobby**: Built-in integration hooks for Unity Gaming Services (UGS) Relay and Lobby.
- **Backend Database**: Full PostgreSQL schema provided at `Database/schema.sql` covering users, profiles, inventories, and transactions.

---

## 🏗️ Development & Building from Unity

1. Open **Unity Hub**.
2. Click **Add > Add project from disk** and select this directory.
3. Open with **Unity 6 (6000.0.x)**.
4. Open scene: `Assets/KYOKA/Scenes/KYOKA_Bootstrap.unity`.
5. Press **Play**!
6. Build via Unity top menu:
   - `KYOKA > Build > Windows (KYOKA.exe)`
   - `KYOKA > Build > Android APK (KYOKA.apk)`
   - `KYOKA > Build > Validate Project`
