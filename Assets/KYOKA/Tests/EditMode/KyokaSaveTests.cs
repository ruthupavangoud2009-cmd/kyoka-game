#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;
using Kyoka.Save;

namespace Kyoka.Tests.EditMode
{
    public class KyokaSaveTests
    {
        [Test]
        public void SaveSystem_SaveAndLoad_PreservesPositionAndStats()
        {
            var testData = new KyokaSaveData
            {
                posX = 125.5f,
                posY = 14.0f,
                posZ = -88.2f,
                hasSavedPosition = true,
                currentHealth = 85,
                kyokaCredits = 1450,
                aetherShards = 42
            };

            KyokaSaveSystem.Save(testData);
            var loadedData = KyokaSaveSystem.Load();

            Assert.IsNotNull(loadedData);
            Assert.IsTrue(loadedData.hasSavedPosition);
            Assert.AreEqual(125.5f, loadedData.posX, 0.01f);
            Assert.AreEqual(14.0f, loadedData.posY, 0.01f);
            Assert.AreEqual(-88.2f, loadedData.posZ, 0.01f);
            Assert.AreEqual(85, loadedData.currentHealth);
            Assert.AreEqual(1450, loadedData.kyokaCredits);
            Assert.AreEqual(42, loadedData.aetherShards);
        }
    }
}
#endif
