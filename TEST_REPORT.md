# KYOKA — Test Report, QA Validation & Bug Tracker

Generated: November 2026

---

## 1. KYOKA 2.0 Comprehensive Bug Tracker

| Bug ID | Description | Root Cause | Files Changed | Technical Fix Applied | QA Test Result |
|---|---|---|---|---|---|
| **BUG-001** | `DontDestroyOnLoad only works for root GameObjects` warning (x7) | Singletons attached as child GameObjects under `KYOKA_Runtime` in `KyokaRuntimeBootstrap.cs` called `DontDestroyOnLoad(gameObject)` in `Awake()`. | `KyokaRuntimeBootstrap.cs`, `KyokaAudioManager.cs`, `KyokaInputBridge.cs`, `KyokaEconomyManager.cs`, `KyokaSocialManager.cs`, `KyokaNetworkManager.cs`, `KyokaBackendClient.cs`, `KyokaMasterGameController.cs`, `KyokaUIManager.cs`, `KyokaWorldManager.cs`, `KyokaGameModeManager.cs` | Guarded with `if (transform.parent == null) DontDestroyOnLoad(gameObject);` and attached manager singletons directly onto `KYOKA_Runtime`. | **PASS (0 warnings in Player.log)** |
| **BUG-002** | Open World player spawned as primitive capsule without model | `KyokaPlayerBootstrap.cs` called `GameObject.CreatePrimitive(PrimitiveType.Capsule)` without attaching `KyokaCharacterModel`. | `KyokaPlayerBootstrap.cs` | Removed primitive mesh, attached `KyokaCharacterModel` + `KyokaProceduralAnimator`, loaded customization from `KyokaSaveData`. | **PASS (3D character visible in Open World)** |
| **BUG-003** | Player locomotion did not animate character joints | `KyokaPlayerController.cs` lacked reference to `KyokaProceduralAnimator`, keeping bones static while moving. | `KyokaPlayerController.cs` | Bound `KyokaProceduralAnimator` to player velocity and grounded state, driving walking/running/jumping animations in real time. | **PASS (Smooth joint animation during locomotion)** |
| **BUG-004** | Open World gameplay HUD suppressed on launch | `KyokaHUD.cs` had `public bool showLegacyHUD = false;` with early return `if (!showLegacyHUD) return;`. | `KyokaHUD.cs` | Overhauled `KyokaHUD.cs` into a modern uGUI canvas HUD with health/energy bars, 6-slot hotbar, quest tracker, building overlay, crosshair, and pause menu. | **PASS (Modern HUD active in Open World)** |
| **BUG-005** | Missing return-to-lobby loop from Open World | Player could not pause and return to 3D Main Lobby without force quitting the application. | `KyokaHUD.cs`, `KyokaMasterGameController.cs` | Added Pause Menu modal (`Escape`) with "Return to Lobby", "Save Game", and "Resume". | **PASS (Clean transition back to Lobby)** |
| **BUG-006** | Building system only placed generic cubes | `KyokaCreativeBuilder.cs` only placed primitive cubes with single colors. | `KyokaCreativeBuilder.cs` | Upgraded to modular base building with 7 piece types (Foundation, Wall, Doorway, Roof, Chest, Barricade, Crafting Table), 3m grid snapping, 90° rotation (`R`), and deconstruction (`Right Click`). | **PASS (Modular base building functional)** |
| **BUG-007** | World regions populated with primitive shapes | `KyokaWorldBootstrap.cs` used default planes and cubes without URP materials or procedural assets. | `KyokaWorldBootstrap.cs` | Upgraded to stylized skyscrapers with neon cornices, multi-cluster branching trees, faceted crystalline rocks, and fast-travel portal arches. | **PASS (Rich world environments rendered)** |

---

## 2. Automated Test Suite Overview

| Test Fixture | Assembly | Test Count | Scope |
|---|---|---|---|
| `KyokaMissionTests` | `Kyoka.Tests.EditMode` | 2 | Mission progression clamp and completion triggers |
| `KyokaEconomyTests` | `Kyoka.Tests.EditMode` | 5 | Balance adjustments, spending validation, anti-exploit cap |
| `KyokaInventoryTests` | `Kyoka.Tests.EditMode` | 4 | Item additions, removals, limits, consumable healing use |
| `KyokaCombatTests` | `Kyoka.Tests.EditMode` | 3 | Damage application, health restoration, defeat event |
| `KyokaSaveTests` | `Kyoka.Tests.EditMode` | 1 | Serialization fidelity, position and economy persistence |

---

## 3. Platform & Hardware Verification

- **Direct3D 11 Renderer**: Standalone Windows executable verified running Direct3D 11.0 [level 11.1] with 0 errors.
- **Android ARM64 Architecture**: Standalone APK verified with IL2CPP 82 MB native C++ library (`libil2cpp.so`) and Android frame pacing (`libswappywrapper.so`).
- **Input System Readiness**: Validated unified input bridge routing keyboard, mouse, and touchscreen virtual controls.
- **Audio Subsystem**: Procedural sound bank verified synthesizing 12 dynamic waveforms without missing audio clip errors.
