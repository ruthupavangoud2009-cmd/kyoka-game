using UnityEngine;
using System;
using System.Collections.Generic;

namespace Kyoka.Inventory
{
    [Serializable]
    public class InventoryStack
    {
        public string itemId;
        public int quantity;
    }

    public class KyokaInventory : MonoBehaviour
    {
        [SerializeField] private List<InventoryStack> items = new List<InventoryStack>();
        public IReadOnlyList<InventoryStack> Items => items;

        public bool Add(string itemId, int quantity)
        {
            if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0) return false;
            var stack = items.Find(x => x.itemId == itemId);
            if (stack == null) { stack = new InventoryStack { itemId = itemId }; items.Add(stack); }
            stack.quantity += quantity;
            return true;
        }

        public bool Remove(string itemId, int quantity)
        {
            if (quantity <= 0) return false;
            var stack = items.Find(x => x.itemId == itemId);
            if (stack == null || stack.quantity < quantity) return false;
            stack.quantity -= quantity;
            if (stack.quantity == 0) items.Remove(stack);
            return true;
        }
    }
}
