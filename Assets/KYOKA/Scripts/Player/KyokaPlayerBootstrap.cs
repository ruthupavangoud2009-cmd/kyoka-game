using UnityEngine;
using Kyoka.Abilities;\nusing Kyoka.Inventory;\nusing Kyoka.Creative;\nusing Kyoka.Save;

namespace Kyoka.Player
{
    public class KyokaPlayerBootstrap : MonoBehaviour
    {
        private void Start()
        {
            if (FindObjectOfType<KyokaPlayerController>() != null) return;

            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "KYOKA_Player";
            player.transform.position = new Vector3(0, 2, 0);
            var oldCollider = player.GetComponent<CapsuleCollider>();
            if (oldCollider) Destroy(oldCollider);

            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0, 0.9f, 0);

            var health = player.AddComponent<KyokaHealth>();
            health.maxHealth = 100;
            health.ResetHealth();

            player.AddComponent<KyokaPlayerController>();
            player.AddComponent<KyokaCameraRig>();
            player.AddComponent<KyokaAbilityUser>();
            player.AddComponent<KyokaInventory>();
            player.AddComponent<KyokaCreativeBuilder>();
            player.AddComponent<KyokaVehicleInteractor>();

            var save = KyokaSaveSystem.Load();
            if (save.hasSavedPosition)
                player.transform.position = new Vector3(save.posX, save.posY, save.posZ);

            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "PlayerSpawnMarker";
            marker.transform.position = new Vector3(0, 0.1f, 0);
            marker.transform.localScale = new Vector3(1.5f, 0.2f, 1.5f);
            Destroy(marker.GetComponent<Collider>());
        }
    }
}
