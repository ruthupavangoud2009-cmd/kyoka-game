#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;
using Kyoka.Inventory;
using Kyoka.Player;

namespace Kyoka.Tests.EditMode
{
    public class KyokaInventoryTests
    {
        private GameObject playerGO;
        private KyokaInventory inventory;

        [SetUp]
        public void Setup()
        {
            playerGO = new GameObject("Test_Player");
            playerGO.AddComponent<KyokaHealth>();
            inventory = playerGO.AddComponent<KyokaInventory>();
        }

        [TearDown]
        public void Teardown()
        {
            if (playerGO != null) Object.DestroyImmediate(playerGO);
        }

        [Test]
        public void Inventory_AddItems_IncreasesQuantity()
        {
            inventory.Add("wood", 10);
            Assert.AreEqual(25, inventory.GetQuantity("wood")); // 15 starter + 10 = 25
        }

        [Test]
        public void Inventory_RemoveItems_ReducesQuantity()
        {
            bool removed = inventory.Remove("wood", 5);
            Assert.IsTrue(removed);
            Assert.AreEqual(10, inventory.GetQuantity("wood")); // 15 starter - 5 = 10
        }

        [Test]
        public void Inventory_RemoveItems_FailsIfInsufficient()
        {
            bool removed = inventory.Remove("wood", 999);
            Assert.IsFalse(removed);
            Assert.AreEqual(15, inventory.GetQuantity("wood"));
        }

        [Test]
        public void Inventory_UseHealthPack_RestoresHealth()
        {
            var health = playerGO.GetComponent<KyokaHealth>();
            health.ApplyGameDamage(50);
            Assert.AreEqual(50, health.CurrentHealth);

            bool used = inventory.UseItem("health_pack");
            Assert.IsTrue(used);
            Assert.AreEqual(90, health.CurrentHealth); // 50 + 40 = 90
            Assert.AreEqual(2, inventory.GetQuantity("health_pack")); // 3 starter - 1 = 2
        }
    }
}
#endif
