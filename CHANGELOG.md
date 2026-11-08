# KYOKA — Changelog

All notable changes across development phases are documented in this file.

---

## [1.0.0-PROD-FOUNDATION] — 2026-11-08

### Added
- **Six World Regions Architecture**:
  - `Region_KYOKACity`: Downtown high-rises, neon signs, commercial shops, garage, and citizen NPCs.
  - `Region_Wildlands`: Mountain ranges, pine forests, harvestable timber and granite nodes.
  - `Region_OceanRealm`: Ocean water plane, coral archipelago, lighthouse, and docks.
  - `Region_AnimeRealm`: Floating Aether sky island, 4 elemental shrines, and celestial boss arena.
  - `Region_BattleIslands`: Military bunker, watchtowers, supply depots, and loot spawners.
  - `Region_CreativeWorld`: Clean sandbox construction grid with coordinate pillars.
- **Unified Cross-Platform Input**:
  - `KyokaInputBridge.cs`: Combines desktop WASD, mouse aim, gamepad, and mobile virtual touch controls seamlessly.
  - `KyokaTouchControls.cs`: Touch virtual joystick and on-screen action buttons (Jump, Sprint, Attack, Ability 1/2, Interact, Camera Toggle).
- **Combat & Arsenal System**:
  - `KyokaCombatSystem.cs`: 3-hit melee combo string, ranged firing, defense guard (reduces damage by 65%), ammo and reload.
  - `KyokaWeaponDefinition.cs`: ScriptableObject data model for melee blades and firearms.
  - `KyokaProjectile.cs`: Trajectory calculation, hit detection, and explosive area damage.
- **Anime Powers & Transformation**:
  - `KyokaAbilityLibrary.cs`: 5 original elemental abilities (Tempest Dash, Flame Surge, Thunder Strike, Void Barrier, Tide Blossom) plus Awakening transformation.
  - `KyokaAbilityUser.cs`: Ability casting, cooldown tracking, energy consumption, and visual aura effects.
- **Vehicles Architecture**:
  - `KyokaVehicleController.cs`: Realistic physics for Cars, Motorcycles, Boats, and Aircraft.
  - Multi-passenger seating (driver + passenger seats), vehicle health, collision damage, fuel consumption, and headlights.
  - `KyokaGarageManager.cs`: Vehicle repair, refuel, and paint color customization.
- **Economy & Anti-Exploit**:
  - `KyokaEconomyManager.cs`: Kyoka Credits (soft currency) and Aether Shards (rare currency) with `ITransactionValidator` exploit protection.
  - `KyokaShopManager.cs`: Merchant catalog for buying and selling items.
- **Survival, Gathering & Crafting**:
  - `KyokaCraftingSystem.cs`: Recipe validation and material synthesis.
  - `KyokaResourceNode.cs`: Harvestable timber and stone nodes in the wildlands.
- **Missions & NPC System**:
  - `KyokaMissionManager.cs`: Story missions ("Awakening in Kyoka", "The Spires of Aether"), side missions, and daily bounties.
  - `KyokaNPC.cs`: Ambient citizen wander, dialogue prompts, and quest initiation.
  - `KyokaWantedSystem.cs`: 0-5 stars wanted rating with law enforcement response foundations.
- **Social & Party Framework**:
  - `KyokaSocialManager.cs`: Player profiles, party creation/invites, chat channels (Global/Party/Local), player reporting and blocking.
- **Multiplayer Networking Foundations**:
  - `KyokaNetworkManager.cs`: Real local-area network (LAN) TCP socket server/client mode for zero-cost testing.
  - `KyokaNetworkPlayer.cs`: Remote player interpolation, health sync, and name tags.
  - `KyokaBackendClient.cs`: REST client for PostgreSQL backend.
  - `Database/schema.sql`: Full PostgreSQL production database schema.
- **Comprehensive UI & HUD**:
  - `KyokaHUD.cs`: Top status bar, vitals bars (Health, Aether), mission tracker, Battle Royale overlay, vehicle stats.
  - `KyokaUIManager.cs`: Modal pause menu, inventory window, map travel selector, game mode switcher, settings with Low/Med/High quality presets.
- **Automation & Tooling**:
  - `Development/ValidateProject.ps1`: 12-point project validation suite.
  - `Development/BuildWindows.ps1`: Batchmode Windows Standalone builder.
  - `Development/BuildAndroid.ps1`: Batchmode Android ARM64 APK builder.
  - `Development/InstallAndroid.ps1`: ADB device detector and APK installer.
  - `Assets/KYOKA/Editor/KyokaBuildPipeline.cs`: Editor top menu options for build, validation, and scene creation.
- **Scene & Project Configuration**:
  - Generated `Assets/KYOKA/Scenes/KYOKA_Bootstrap.unity` with runtime bootstrap and main camera rig.
  - Configured `ProjectSettings/ProjectSettings.asset`, `QualitySettings.asset`, `TagManager.asset`, and `EditorBuildSettings.asset`.

### Fixed
- Fixed critical syntax error in `KyokaRuntimeBootstrap.cs` (literal `\n` characters in using directives).
- Fixed critical syntax error in `KyokaPlayerBootstrap.cs` (literal `\n` characters in using directives).
- Fixed camera audio listener duplication: ensured exactly one active `AudioListener` is retained at runtime.
- Updated `Packages/manifest.json` with pinned versions for Universal Render Pipeline, Input System, Netcode for GameObjects, and Transport.

