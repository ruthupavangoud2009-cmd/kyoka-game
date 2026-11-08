#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;
using Kyoka.Combat;
using Kyoka.Player;

namespace Kyoka.Tests.EditMode
{
    public class KyokaCombatTests
    {
        [Test]
        public void Health_ApplyGameDamage_ReducesCurrentHealth()
        {
            var go = new GameObject("TestTarget");
            var health = go.AddComponent<KyokaHealth>();
            health.maxHealth = 100;
            health.ResetHealth();

            health.ApplyGameDamage(35);
            Assert.AreEqual(65, health.CurrentHealth);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Health_Restore_CapsAtMaxHealth()
        {
            var go = new GameObject("TestTarget");
            var health = go.AddComponent<KyokaHealth>();
            health.maxHealth = 100;
            health.ResetHealth();

            health.ApplyGameDamage(20);
            health.Restore(50);
            Assert.AreEqual(100, health.CurrentHealth);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Health_DefeatedEvent_FiresAtZeroHealth()
        {
            var go = new GameObject("TestTarget");
            var health = go.AddComponent<KyokaHealth>();
            health.maxHealth = 50;
            health.ResetHealth();

            bool defeatedFired = false;
            health.Defeated += () => defeatedFired = true;

            health.ApplyGameDamage(50);
            Assert.AreEqual(0, health.CurrentHealth);
            Assert.IsTrue(defeatedFired);

            Object.DestroyImmediate(go);
        }
    }
}
#endif

