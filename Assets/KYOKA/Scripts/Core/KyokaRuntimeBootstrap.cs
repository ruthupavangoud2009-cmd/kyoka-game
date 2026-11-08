using UnityEngine;
using Kyoka.Player;
using Kyoka.World;
using Kyoka.UI;
using Kyoka.Vehicles;
using Kyoka.Abilities;\nusing Kyoka.Missions;\nusing Kyoka.BattleRoyale;\nusing Kyoka.Networking;

namespace Kyoka.Core
{
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
            if (FindObjectOfType<KyokaWorldBootstrap>() == null)
                new GameObject("WorldBootstrap").AddComponent<KyokaWorldBootstrap>();
            if (FindObjectOfType<KyokaPlayerBootstrap>() == null)
                new GameObject("PlayerBootstrap").AddComponent<KyokaPlayerBootstrap>();
            if (FindObjectOfType<KyokaVehicleSpawner>() == null)
                new GameObject("VehicleSpawner").AddComponent<KyokaVehicleSpawner>();
            if (FindObjectOfType<KyokaAbilityLibrary>() == null)
                new GameObject("AbilityLibrary").AddComponent<KyokaAbilityLibrary>();
            if (FindObjectOfType<KyokaHUD>() == null)
                new GameObject("KYOKA_HUD").AddComponent<KyokaHUD>();
            if (FindObjectOfType<KyokaMissionManager>() == null)
                new GameObject("MissionManager").AddComponent<KyokaMissionManager>();
            if (FindObjectOfType<KyokaMatchController>() == null)
                new GameObject("BattleRoyaleMatchController").AddComponent<KyokaMatchController>();
            if (FindObjectOfType<KyokaNetworkStatus>() == null)
                new GameObject("NetworkStatus_OfflinePrototype").AddComponent<KyokaNetworkStatus>();
        }
    }
}
