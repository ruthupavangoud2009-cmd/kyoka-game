using UnityEngine;
using Kyoka.Abilities;
using Kyoka.Combat;
using Kyoka.Inventory;
using Kyoka.Creative;
using Kyoka.Character;
using Kyoka.Save;

namespace Kyoka.Player
{
    public class KyokaPlayerBootstrap : MonoBehaviour
    {
        private void Start()
        {
            if (FindObjectOfType<KyokaPlayerController>() != null) return;

            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "KYOKA_Player";
            player.tag = "Player";
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

            // Core Player Controllers
            player.AddComponent<KyokaPlayerController>();
            player.AddComponent<KyokaCameraRig>();
            player.AddComponent<KyokaAbilityUser>();
            player.AddComponent<KyokaCombatSystem>();
            player.AddComponent<KyokaInventory>();
            player.AddComponent<KyokaCreativeBuilder>();
            player.AddComponent<KyokaVehicleInteractor>();
            player.AddComponent<KyokaCharacterCustomization>();

            // Load saved position and stats if available
            var save = KyokaSaveSystem.Load();
            if (save != null)
            {
                if (save.hasSavedPosition)
                {
                    player.transform.position = new Vector3(save.posX, save.posY, save.posZ);
                    player.transform.rotation = Quaternion.Euler(0, save.rotY, 0);
                }
                if (save.currentHealth > 0)
                {
                    health.maxHealth = save.maxHealth > 0 ? save.maxHealth : 100;
                }
                var inv = player.GetComponent<KyokaInventory>();
                if (inv != null) inv.LoadFrom(save);
                var custom = player.GetComponent<KyokaCharacterCustomization>();
                if (custom != null) custom.LoadFrom(save);
            }

            // Spawn platform marker
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "PlayerSpawnMarker";
            marker.transform.position = new Vector3(0, 0.1f, 0);
            marker.transform.localScale = new Vector3(2.0f, 0.2f, 2.0f);
            var rend = marker.GetComponent<Renderer>();
            if (rend) rend.material.color = new Color(0.2f, 0.8f, 0.9f);
            Destroy(marker.GetComponent<Collider>());
        }
    }
}
