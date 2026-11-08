using UnityEngine;
using Kyoka.Inventory;
using Kyoka.Player;
using Kyoka.Core;

namespace Kyoka.BattleRoyale
{
    public class KyokaLootSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        public int crateCount = 10;
        public float spawnRadius = 80f;

        private void Start()
        {
            SpawnLootCrates();
        }

        public void SpawnLootCrates()
        {
            for (int i = 0; i < crateCount; i++)
            {
                Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * spawnRadius;
                Vector3 pos = new Vector3(randomCircle.x, 0.5f, randomCircle.y);

                var crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                crate.name = $"LootCrate_{i}";
                crate.transform.position = pos;
                crate.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

                var rend = crate.GetComponent<Renderer>();
                if (rend) rend.material.color = new Color(0.95f, 0.7f, 0.1f); // Golden loot box

                var loot = crate.AddComponent<KyokaLootCrate>();
                loot.itemRewardId = (i % 2 == 0) ? "health_pack" : "energy_vial";
                loot.itemAmount = UnityEngine.Random.Range(2, 5);
            }
        }
    }

    public class KyokaLootCrate : MonoBehaviour
    {
        public string itemRewardId = "health_pack";
        public int itemAmount = 2;
        public float pickupDistance = 3.0f;

        private void Update()
        {
            var input = KyokaInputBridge.Instance;
            if (input != null && input.InteractPressed)
            {
                var player = FindObjectOfType<KyokaPlayerController>();
                if (player != null && Vector3.Distance(transform.position, player.transform.position) <= pickupDistance)
                {
                    OpenCrate(player);
                }
            }
        }

        public void OpenCrate(KyokaPlayerController player)
        {
            var inv = player.GetComponent<KyokaInventory>();
            if (inv != null)
            {
                inv.Add(itemRewardId, itemAmount);
                Debug.Log($"[KYOKA Loot] Collected {itemAmount}x {itemRewardId} from supply crate!");
            }

            // Burst visual
            var fx = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fx.transform.position = transform.position;
            fx.transform.localScale = Vector3.one * 1.5f;
            var r = fx.GetComponent<Renderer>();
            if (r) r.material.color = Color.yellow;
            Destroy(fx.GetComponent<Collider>());
            Destroy(fx, 0.3f);

            Destroy(gameObject);
        }
    }
}
