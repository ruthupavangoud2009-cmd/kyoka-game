using UnityEngine;
using System;
using Kyoka.Save;

namespace Kyoka.Economy
{
    public interface ITransactionValidator
    {
        bool ValidateCreditChange(int currentBalance, int delta, string source);
    }

    /// <summary>
    /// Local implementation of transaction validation for development.
    /// In competitive production, replaced with authoritative server validation.
    /// </summary>
    public class LocalSecurityValidator : ITransactionValidator
    {
        public bool ValidateCreditChange(int currentBalance, int delta, string source)
        {
            // Protect against negative exploits or impossible single-frame bursts
            if (delta > 100000 && source != "AdminGrant")
            {
                Debug.LogWarning($"[Anti-Exploit] Suspicious transaction flagged: +{delta} from {source}");
                return false;
            }
            if (currentBalance + delta < 0) return false;
            return true;
        }
    }

    public class KyokaEconomyManager : MonoBehaviour
    {
        public static KyokaEconomyManager Instance { get; private set; }

        [SerializeField] private int kyokaCredits = 500;
        [SerializeField] private int aetherShards = 10;

        public int KyokaCredits => kyokaCredits;
        public int AetherShards => aetherShards;

        private ITransactionValidator validator = new LocalSecurityValidator();

        public event Action<int, int> OnBalanceChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
        }

        public void SetValidator(ITransactionValidator newValidator)
        {
            if (newValidator != null) validator = newValidator;
        }

        public bool AddCredits(int amount, string source = "Gameplay")
        {
            if (amount <= 0) return false;
            if (!validator.ValidateCreditChange(kyokaCredits, amount, source)) return false;

            kyokaCredits += amount;
            OnBalanceChanged?.Invoke(kyokaCredits, aetherShards);
            return true;
        }

        public bool SpendCredits(int amount)
        {
            if (amount <= 0 || kyokaCredits < amount) return false;
            if (!validator.ValidateCreditChange(kyokaCredits, -amount, "ShopPurchase")) return false;

            kyokaCredits -= amount;
            OnBalanceChanged?.Invoke(kyokaCredits, aetherShards);
            return true;
        }

        public bool AddShards(int amount)
        {
            if (amount <= 0) return false;
            aetherShards += amount;
            OnBalanceChanged?.Invoke(kyokaCredits, aetherShards);
            return true;
        }

        public bool SpendShards(int amount)
        {
            if (amount <= 0 || aetherShards < amount) return false;
            aetherShards -= amount;
            OnBalanceChanged?.Invoke(kyokaCredits, aetherShards);
            return true;
        }

        public void SaveTo(KyokaSaveData save)
        {
            if (save != null)
            {
                save.kyokaCredits = kyokaCredits;
                save.aetherShards = aetherShards;
            }
        }

        public void LoadFrom(KyokaSaveData save)
        {
            if (save != null)
            {
                kyokaCredits = save.kyokaCredits;
                aetherShards = save.aetherShards;
                OnBalanceChanged?.Invoke(kyokaCredits, aetherShards);
            }
        }
    }
}

