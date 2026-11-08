using UnityEngine;
using System;

namespace Kyoka.Player
{
    public class KyokaHealth : MonoBehaviour
    {
        [Min(1)] public int maxHealth = 100;
        public int CurrentHealth { get; private set; }
        public event Action<int, int> HealthChanged;
        public event Action Defeated;

        private void Awake() => ResetHealth();

        public void ResetHealth()
        {
            CurrentHealth = maxHealth;
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public void ApplyGameDamage(int amount)
        {
            if (amount <= 0 || CurrentHealth <= 0) return;
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
            if (CurrentHealth == 0) Defeated?.Invoke();
        }

        public void Restore(int amount)
        {
            if (amount <= 0) return;
            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
    }
}
