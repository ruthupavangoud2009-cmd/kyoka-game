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

            var player = new GameObject("KYOKA_Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0, 2, 0);

            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0, 0.9f, 0);

            // 3D Humanoid Character Model & Procedural Animator
            var model = player.AddComponent<KyokaCharacterModel>();
            var anim = player.AddComponent<KyokaProceduralAnimator>();

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

            // Load saved character customization and stats
            var save = KyokaSaveSystem.Load();
            if (save != null)
            {
                if (save.customization != null && save.customization.hasCreatedCharacter)
                {
                    model.isFemale = save.customization.isFemale;
                    model.currentHairStyle = (HairStyleType)save.customization.hairStyleIndex;
                    model.currentHairColor = new Color(save.customization.hairR, save.customization.hairG, save.customization.hairB);
                    model.currentSkinColor = new Color(save.customization.skinR, save.customization.skinG, save.customization.skinB);
                    model.currentOutfit = (OutfitPreset)save.customization.outfitIndex;
                    model.BuildHumanoidMesh();
                }

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

            model.EquipWeapon("AssaultRifle");

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
