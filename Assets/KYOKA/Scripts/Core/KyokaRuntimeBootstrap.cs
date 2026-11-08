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
using Kyoka.Audio;

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
            EnsureComponent<KyokaInputBridge>();
            EnsureComponent<KyokaAudioManager>();
            EnsureComponent<KyokaEconomyManager>();
            EnsureComponent<KyokaShopManager>();
            EnsureComponent<KyokaSocialManager>();
            EnsureComponent<KyokaNetworkManager>();
            EnsureComponent<KyokaBackendClient>();

            // Master Game Flow Coordinator (Title -> Character Customizer -> 3D Lobby -> Battle Royale / Open World)
            EnsureComponent<KyokaMasterGameController>();
        }

        private T EnsureComponent<T>() where T : Component
        {
            T existing = FindObjectOfType<T>();
            if (existing != null) return existing;
            return gameObject.AddComponent<T>();
        }
    }
}
