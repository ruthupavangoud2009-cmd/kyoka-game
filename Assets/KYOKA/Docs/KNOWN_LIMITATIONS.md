# Known Limitations

- Runtime environment is generated greybox geometry.
- Camera and movement use legacy Unity Input calls for broad starter compatibility.
- Mobile UI is not yet a complete touch-control implementation.
- Vehicle controller is a simple prototype, not production vehicle physics.
- Ability effects are local visual prototypes; authoritative multiplayer validation is absent.
- Inventory and mission modules are foundational and need complete gameplay UI/persistence.
- Creative placement is local only.
- Battle royale is a local match-state prototype, not a real online match.
- Networking status explicitly remains offline until a real provider is integrated.
- No production backend or dedicated server is included.
- iOS signing/building requires the appropriate Apple development environment.

- The uploaded archive contains no saved `.unity` scene or prefab assets; use the included Editor menu to generate the bootstrap scene.
- The player bootstrap now attaches inventory, creative placement and vehicle interaction components, but these remain local prototypes.
- Mission, match-state and network-status manager components are initialized, but their complete gameplay/service flows are not implemented.
- The runtime uses legacy `UnityEngine.Input` calls despite including the Input System package. Verify Active Input Handling in Unity Player Settings; migrate to Input Actions before production/mobile use.
- Save restoration currently covers player position only; inventory, currency, missions, world state and server state are not persisted.
