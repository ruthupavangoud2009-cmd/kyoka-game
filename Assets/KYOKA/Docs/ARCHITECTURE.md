# KYOKA Architecture

## Runtime flow
`KyokaRuntimeBootstrap` ensures runtime bootstrapping. It creates the world, player, vehicle prototypes, ability library and HUD if absent.

## Modules
- Core: startup and configuration.
- Player: movement, health and camera.
- Abilities: ScriptableObject definitions and ability execution.
- Vehicles: shared vehicle type and prototype controller.
- World: runtime greybox environment.
- Inventory: item stacks.
- Missions: objective progress.
- Survival: resource nodes and recipe definitions.
- Creative: basic block placement.
- BattleRoyale: local match-state prototype.
- Networking: explicit integration boundary, not a live connection.
- Save: local JSON persistence.
- UI: prototype HUD.

## Production expansion
Use interfaces and service abstractions as the project grows. Move valuable state to an authoritative backend. Replace prototype input with Input System actions and implement touch controls. Replace generated geometry with optimized Addressable environment assets. Add scene/region streaming and server-hosted sessions.
