#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;
using Kyoka.Economy;

namespace Kyoka.Tests.EditMode
{
    public class KyokaEconomyTests
    {
        private GameObject ecoGO;
        private KyokaEconomyManager eco;

        [SetUp]
        public void Setup()
        {
            ecoGO = new GameObject("Test_EconomyManager");
            eco = ecoGO.AddComponent<KyokaEconomyManager>();
        }

        [TearDown]
        public void Teardown()
        {
            if (ecoGO != null) Object.DestroyImmediate(ecoGO);
        }

        [Test]
        public void Economy_InitialBalance_IsDefault500Credits()
        {
            Assert.AreEqual(500, eco.KyokaCredits);
        }

        [Test]
        public void Economy_SpendCredits_ReducesBalanceCorrectly()
        {
            bool success = eco.SpendCredits(200);
            Assert.IsTrue(success);
            Assert.AreEqual(300, eco.KyokaCredits);
        }

        [Test]
        public void Economy_SpendCredits_FailsWhenInsufficientFunds()
        {
            bool success = eco.SpendCredits(9999);
            Assert.IsFalse(success);
            Assert.AreEqual(500, eco.KyokaCredits);
        }

        [Test]
        public void Economy_AntiExploit_RejectsAbnormalGain()
        {
            bool success = eco.AddCredits(5000000, "TamperedExploitSource");
            Assert.IsFalse(success);
            Assert.AreEqual(500, eco.KyokaCredits);
        }

        [Test]
        public void Economy_Shards_AddAndSpendCorrectly()
        {
            eco.AddShards(5);
            Assert.AreEqual(15, eco.AetherShards);

            bool spent = eco.SpendShards(10);
            Assert.IsTrue(spent);
            Assert.AreEqual(5, eco.AetherShards);
        }
    }
}
#endif

