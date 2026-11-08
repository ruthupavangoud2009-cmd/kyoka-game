# KYOKA — System Architecture

See root `ARCHITECTURE.md` for the full technical breakdown, class diagrams, and flow charts.

## Core Modules
- **Core**: Runtime bootstrap (`KyokaRuntimeBootstrap.cs`), Input bridge (`KyokaInputBridge.cs`), Game mode manager (`KyokaGameModeManager.cs`).
- **Player**: Movement & states (`KyokaPlayerController.cs`), Camera rig (`KyokaCameraRig.cs`), Health (`KyokaHealth.cs`), Customization (`KyokaCharacterCustomization.cs`).
- **Combat**: Melee combos, ranged weapons, guard defense (`KyokaCombatSystem.cs`), Projectiles (`KyokaProjectile.cs`).
- **Abilities**: 5 elemental powers + Awakening transformation (`KyokaAbilityLibrary.cs`), energy and cooldown management (`KyokaAbilityUser.cs`).
- **Vehicles**: Car, Motorcycle, Boat, Aircraft controllers (`KyokaVehicleController.cs`), Garage services (`KyokaGarageManager.cs`).
- **World**: 6 Major Regions, Day/Night lighting, dynamic weather (`KyokaWorldManager.cs`, `KyokaWorldBootstrap.cs`).
- **Economy**: Currency, shops, anti-exploit validation (`KyokaEconomyManager.cs`, `KyokaShopManager.cs`).
- **Survival**: Gathering nodes, crafting recipes (`KyokaCraftingSystem.cs`, `KyokaResourceNode.cs`).
- **Missions**: Story, side, and daily quest tracker (`KyokaMissionManager.cs`).
- **Battle Royale**: Safe zone circle, storm damage, alive counter, match results (`KyokaMatchController.cs`, `KyokaLootSpawner.cs`).
- **Creative**: Grid placement, color palette, JSON blueprint saving (`KyokaCreativeBuilder.cs`).
- **Social**: Player profiles, party management, multi-channel chat (`KyokaSocialManager.cs`).
- **Networking**: LAN socket server/client mode, Relay/Lobby integration boundaries (`KyokaNetworkManager.cs`).
- **UI**: In-game HUD, modal pause menu, settings, mobile touch joystick (`KyokaHUD.cs`, `KyokaUIManager.cs`, `KyokaTouchControls.cs`).
- **Persistence**: Safe local JSON save system with backup restore (`KyokaSaveSystem.cs`).
