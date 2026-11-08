using UnityEngine;
using Kyoka.Player;
using Kyoka.World;
using Kyoka.UI;
using Kyoka.Vehicles;
using Kyoka.Abilities;
using Kyoka.Combat;
using Kyoka.Missions;
using Kyoka.BattleRoyale;
using Kyoka.Networking;
using Kyoka.Economy;
using Kyoka.Survival;
using Kyoka.Social;
using Kyoka.Backend;

namespace Kyoka.Core
{
    /// <summary>
    /// Master runtime bootstrap for KYOKA.
    /// Ensures every core subsystem, manager, world builder, UI canvas,
    /// and player avatar is initialized and registered upon launch.
    /// </summary>
    public class KyokaRuntimeBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureBootstrap()
        {
            if (FindObjectOfType<KyokaRuntimeBootstrap>() != null) return;
            var root = new GameObject("KYOKA_Runtime");
            root.AddComponent<KyokaRuntimeBootstrap>();
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            // Core Foundation & Input
            EnsureComponent<KyokaInputBridge>("InputBridge");
            EnsureComponent<KyokaGameModeManager>("GameModeManager");
            EnsureComponent<KyokaEconomyManager>("EconomyManager");
            EnsureComponent<KyokaShopManager>("ShopManager");
            EnsureComponent<KyokaCraftingSystem>("CraftingSystem");

            // World & Spawners
            EnsureComponent<KyokaWorldBootstrap>("WorldBootstrap");
            EnsureComponent<KyokaPlayerBootstrap>("PlayerBootstrap");
            EnsureComponent<KyokaVehicleSpawner>("VehicleSpawner");
            EnsureComponent<KyokaAbilityLibrary>("AbilityLibrary");

            // Gameplay Rules & Systems
            EnsureComponent<KyokaMissionManager>("MissionManager");
            EnsureComponent<KyokaMatchController>("BattleRoyaleMatchController");
            EnsureComponent<KyokaWantedSystem>("WantedSystem");

            // Social & Networking
            EnsureComponent<KyokaSocialManager>("SocialManager");
            EnsureComponent<KyokaNetworkManager>("NetworkManager");
            EnsureComponent<KyokaBackendClient>("BackendClient");

            // UI & Presentation
            EnsureComponent<KyokaUIManager>("UIManager");
            EnsureComponent<KyokaTouchControls>("TouchControls");
            EnsureComponent<KyokaHUD>("KYOKA_HUD");
        }

        private T EnsureComponent<T>(string goName) where T : Component
        {
            T existing = FindObjectOfType<T>();
            if (existing != null) return existing;

            var go = new GameObject(goName);
            go.transform.SetParent(transform);
            return go.AddComponent<T>();
        }
    }
}
