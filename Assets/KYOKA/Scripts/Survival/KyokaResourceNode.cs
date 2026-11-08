using UnityEngine;
using Kyoka.Inventory;

namespace Kyoka.Survival
{
    public class KyokaResourceNode : MonoBehaviour
    {
        public string resourceItemId = "wood";
        public int yieldAmount = 2;
        public bool depleted;

        public bool Gather(KyokaInventory inventory)
        {
            if (depleted || !inventory) return false;
            inventory.Add(resourceItemId, yieldAmount);
            depleted = true;
            gameObject.SetActive(false);
            return true;
        }
    }
}
