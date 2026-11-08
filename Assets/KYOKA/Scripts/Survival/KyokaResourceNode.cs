using UnityEngine;
using Kyoka.Inventory;
using Kyoka.Audio;

namespace Kyoka.Survival
{
    public enum ResourceType
    {
        Tree = 0,
        Stone = 1,
        IronOre = 2,
        AetherCrystal = 3
    }

    /// <summary>
    /// Interactive survival resource node (Trees, Rocks, Iron Veins, Crystalline Ores).
    /// Supports progressive harvesting, hit wobbles, impact SFX, and inventory rewards.
    /// </summary>
    public class KyokaResourceNode : MonoBehaviour
    {
        [Header("Resource Configuration")]
        public ResourceType resourceType = ResourceType.Tree;
        public string resourceItemId = "wood_log";
        public string secondaryItemId = "fiber";
        public int yieldAmount = 3;
        public int maxHits = 4;
        public int currentHits = 0;
        public bool depleted = false;

        private Vector3 initialScale;
        private float wobbleTimer = 0f;

        private void Awake()
        {
            initialScale = transform.localScale;
            InitializeResourceDefaults();
        }

        private void InitializeResourceDefaults()
        {
            switch (resourceType)
            {
                case ResourceType.Tree:
                    resourceItemId = "wood_log";
                    secondaryItemId = "sapling";
                    yieldAmount = 4;
                    maxHits = 3;
                    break;
                case ResourceType.Stone:
                    resourceItemId = "cobblestone";
                    secondaryItemId = "flint";
                    yieldAmount = 3;
                    maxHits = 4;
                    break;
                case ResourceType.IronOre:
                    resourceItemId = "iron_ore";
                    secondaryItemId = "stone_chunk";
                    yieldAmount = 2;
                    maxHits = 5;
                    break;
                case ResourceType.AetherCrystal:
                    resourceItemId = "aether_shard";
                    secondaryItemId = "crystal_dust";
                    yieldAmount = 2;
                    maxHits = 4;
                    break;
            }
        }

        private void Update()
        {
            if (wobbleTimer > 0f)
            {
                wobbleTimer -= Time.deltaTime;
                float wobble = Mathf.Sin(wobbleTimer * 30f) * 0.08f;
                transform.localScale = initialScale + new Vector3(wobble, -wobble * 0.5f, wobble);
                if (wobbleTimer <= 0f)
                {
                    transform.localScale = initialScale;
                }
            }
        }

        /// <summary>
        /// Hit the resource node with a tool or weapon.
        /// </summary>
        public bool Hit(int damage, KyokaInventory inventory)
        {
            if (depleted) return false;

            currentHits++;
            wobbleTimer = 0.25f;

            // Audio & VFX
            KyokaAudioManager.Instance?.PlaySoundEffect(SoundEffectType.Hitmarker);

            if (currentHits >= maxHits)
            {
                DepleteNode(inventory);
                return true;
            }

            return false;
        }

        public bool Gather(KyokaInventory inventory)
        {
            return Hit(1, inventory);
        }

        private void DepleteNode(KyokaInventory inventory)
        {
            depleted = true;
            if (inventory != null)
            {
                inventory.Add(resourceItemId, yieldAmount);
                if (!string.IsNullOrEmpty(secondaryItemId))
                {
                    inventory.Add(secondaryItemId, 1);
                }
            }

            KyokaAudioManager.Instance?.PlaySoundEffect(SoundEffectType.LootPickup);
            Debug.Log($"[KYOKA Resource] Harvested {yieldAmount}x {resourceItemId} from {gameObject.name}");

            // Shrink and disable
            gameObject.SetActive(false);
        }
    }
}
