# KYOKA — Changelog

All notable changes across development phases are documented in this file.

---

## [1.2.0-CROSS-PLATFORM-EXPERIENCE] — 2026-11-08

### Added
- **Cinematic Title & Authentication Screen**:
  - `KyokaTitleScreenUI.cs`: Free Fire-style animated glowing logo, Aether Protocol subtitle, guest authentication, and tap-to-start transition.
- **3D Character Customizer Showroom**:
  - `KyokaCharacterCustomizerUI.cs`: 3D showroom pedestal, 360° mouse drag rotation, scroll zoom, and normalized non-overlapping vertical UI panel.
  - Symmetrical procedural anime face with eyes, eyebrows, blush, and smile texture generation.
  - 4 hairstyle selections (Spiky Anime with scalp base and framing bangs, Cyber Fade, Samurai Ponytail, Tactical Buzzcut), gender options, skin tones, hair colors, outfit presets, and random name generator.
- **3D Hangar Main Lobby**:
  - `KyokaLobbyUI.cs`: Cyber hangar environment with glowing pillars, character weapon stance, rank/currency header (Kaito, Level 1, Gold II, Coins 500, Gems 10), mode selection, Wardrobe, Armory, Vehicles, and Shop modals.
- **Battle Royale Combat & Island Drop**:
  - `KyokaBattleRoyaleExperience.cs`: Glider drop sequence from 140m altitude with banking physics, 400m x 400m island terrain, procedural military compounds, watchtowers, and glowing floor loot beams (Assault Rifle, Shotgun, Sniper, Katana, Medkit, Armor).
  - Over-the-shoulder ADS shooting with recoil, muzzle flash, bullet tracers, hitmarkers, and headshots.
  - Hostile AI bots with patrol, line-of-sight acquisition, and combat shooting.
  - Dynamic storm circle constriction with radioactive storm barrier and damage ticks.
  - Minimap radar, real-time kill feed, and BOOYAH! victory fanfare screen with return-to-lobby loop.
- **Procedural Art & Audio Engine**:
  - `KyokaMaterialFactory.cs`: Bulletproof material and shader factory with robust fallback chains preventing null shaders in standalone builds.
  - `KyokaTextureGenerator.cs`: Runtime procedural generation for cyber grids, 9-sliced panels, buttons, crosshairs, anime faces, and glider skins.
  - `KyokaSoundBank.cs`: Procedural audio synthesizer generating lobby synthwave music, combat drum tracks, gunshots (Rifle, Shotgun, Sniper), Katana slashes, reloads, hitmarkers, headshots, loot pickups, glider wind, and BOOYAH! victory fanfare at ₹0 cost.
  - `KyokaProceduralAnimator.cs`: Joint animation providing idle breathing, run/walk cycles, ADS aiming pose, firing recoil kick, and skydiving glide pose.
- **Cross-Platform Compilation & Verification**:
  - Windows 64-bit standalone build compiled and verified: `Builds/Windows/KYOKA.exe` (and release package `Builds/KYOKA-Windows-v1.0.0.zip`).
  - Android ARM64 APK compiled via Unity 6 IL2CPP + Android NDK r23b: `Builds/Android/KYOKA.apk` (and release package `Builds/KYOKA-Android-v1.0.0.apk`, 33.08 MB).
  - Standalone multi-screen automated capture with verified screenshots in `Builds/Screenshots/`.
  - Zero errors / zero exceptions verified in `Player.log`.

---

## [1.1.0-RELEASE-CANDIDATE] — 2026-11-08

### Added
- **One-Click Play & Build System**:
  - `PLAY_KYOKA.bat`: Instant launcher detecting `Builds/Windows/KYOKA.exe` and offering automated build fallback.
  - `BUILD_KYOKA.bat`: Automated validation and Unity batchmode Windows standalone builder.
  - `PLAY_KYOKA_ANDROID.bat`: One-click ADB detector, APK installer, and device launcher.
- **Dedicated Regional Scenes**:
  - `KYOKA_MainMenu.unity`, `KYOKA_City.unity`, `KYOKA_Wildlands.unity`, `KYOKA_OceanRealm.unity`, `KYOKA_AnimeRealm.unity`, `KYOKA_BattleIslands.unity`, `KYOKA_CreativeWorld.unity`.
  - Configured `EditorBuildSettings.asset` with all 8 scenes in exact build index order.
- **Audio Subsystem & Procedural Synthesis**:
  - `KyokaAudioManager.cs`: Real-time audio tone synthesizer operating via `OnAudioFilterRead` to produce footstep thuds, sword slash sweeps, projectile blasts, and regional melodic chords at ₹0 asset cost.
- **Autonomous Enemy AI & Boss Combat**:
  - `KyokaEnemyAI.cs`: State machine (Idle, Patrol, Chase, Attack, Defeated), vision cone detection, and reward granting for Melee Grunts, Ranged Sentries, and Elite Brutes.
  - `KyokaBossController.cs`: Multi-phase Anime Realm boss ("The Celestial Sovereign") with Ground Slam shockwaves, elemental projectile storms, and Awakened Void phase with top-screen boss health bar.
- **Urban Traffic & Law Enforcement**:
  - `KyokaTrafficSystem.cs`: Spawns and manages civilian traffic cars navigating road network waypoints with obstacle collision avoidance.
  - `KyokaPoliceAI.cs`: Law enforcement pursuit cruiser responding to wanted stars with red/blue flashing siren lights.
- **GitHub Repository Synchronization**:
  - Linked remote `origin` to `https://github.com/ruthupavangoud2009-cmd/kyoka-game.git`.
  - Pushed all code, assets, scenes, settings, and documentation to branch `main`.

---

## [1.0.0-PROD-FOUNDATION] — 2026-11-08

### Added
- Six World Regions architecture (City, Wildlands, Ocean, Anime, Battle, Creative).
- Unified Input Bridge for Keyboard/Mouse, Gamepad, and Touchscreen.
- Mobile Touch Controls (Left Virtual Joystick, Right Touch Look, Action Buttons).
- Combat system with 3-hit melee combos, ranged firearms, and guard defense.
- 5 Anime Elemental Abilities + Awakening Transformation.
- Multi-class vehicle physics (Car, Motorcycle, Boat, Aircraft) and Garage customizer.
- Economy & Inventory with transaction security and craft-from-gathering loop.
- Story and side mission tracker with rewards.
- Social profiles, party management, and multi-channel chat.
- Real multiplayer LAN socket server/client mode on port 7777.
- Complete PostgreSQL database schema (`Database/schema.sql`).
- In-game HUD and modal pause, settings, inventory, map, and mode menus.
- Build automation scripts (`ValidateProject.ps1`, `BuildWindows.ps1`, `BuildAndroid.ps1`, `InstallAndroid.ps1`).

### Fixed
- Fixed critical syntax error in `KyokaRuntimeBootstrap.cs` (literal `\n` characters in using directives).
- Fixed critical syntax error in `KyokaPlayerBootstrap.cs` (literal `\n` characters in using directives).
- Fixed camera audio listener duplication: ensured exactly one active `AudioListener` is retained at runtime.
- Updated `Packages/manifest.json` with pinned versions for Universal Render Pipeline, Input System, Netcode for GameObjects, and Transport.
