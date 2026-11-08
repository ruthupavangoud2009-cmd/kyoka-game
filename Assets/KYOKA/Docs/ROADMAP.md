# KYOKA Roadmap

This folder includes the first integrated prototype foundation.

Next engineering priorities:
1. Compile and repair all project/package issues.
2. Replace legacy Input Manager calls with Unity Input System actions.
3. Add proper prefabs, materials and scene setup.
4. Implement touch controls and controller mapping.
5. Add real inventory UI, crafting transactions and save integration.
6. Implement region streaming and multiple playable scenes.
7. Integrate real multiplayer.
8. Build backend APIs and persistent player accounts.
9. Create original production art, animation and audio.
10. Profile PC, Android and iOS builds.

## Immediate integration repair batch
- Reuse the scene Main Camera to avoid creating a second camera/audio listener.
- Add local E-key vehicle enter/exit interaction.
- Attach inventory, creative builder and vehicle interaction to the player.
- Bootstrap mission, local battle match and explicit offline network status components.
- Restore the saved player position on startup.
- Document free-budget constraints and verification status.

These additions are source-level changes only and still require Unity Editor compilation and runtime testing.
