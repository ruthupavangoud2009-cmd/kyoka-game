using UnityEngine;
using System;
using System.Collections.Generic;
using Kyoka.Player;
using Kyoka.Abilities;
using Kyoka.Save;

namespace Kyoka.Inventory
{
    [Serializable]
    public class InventoryStack
    {
        public string itemId;
        public int quantity;

        public InventoryStack() { }
        public InventoryStack(string id, int qty)
        {
            itemId = id;
            quantity = qty;
        }
    }

    public class KyokaInventory : MonoBehaviour
    {
        [Header("Inventory Capacity")]
        public int maxSlots = 24;
        [SerializeField] private List<InventoryStack> items = new List<InventoryStack>();
        public IReadOnlyList<InventoryStack> Items => items;

        private KyokaHealth health;
        private KyokaAbilityUser abilityUser;

        public event Action OnInventoryChanged;
        public event Action<string, int> OnItemAdded;

        private void Awake()
        {
            health = GetComponent<KyokaHealth>();
            abilityUser = GetComponent<KyokaAbilityUser>();
            InitializeStarterItems();
        }

        private void InitializeStarterItems()
        {
            if (items.Count == 0)
            {
                Add("health_pack", 3);
                Add("energy_vial", 2);
                Add("wood", 15);
                Add("stone", 10);
            }
        }

        public bool Add(string itemId, int quantity)
        {
            if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0) return false;

            var existing = items.Find(x => x.itemId == itemId);
            if (existing != null)
            {
                existing.quantity += quantity;
            }
            else
            {
                if (items.Count >= maxSlots)
                {
                    Debug.LogWarning("[KYOKA Inventory] Inventory is full!");
                    return false;
                }
                items.Add(new InventoryStack(itemId, quantity));
            }

            OnItemAdded?.Invoke(itemId, quantity);
            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool Remove(string itemId, int quantity)
        {
            if (quantity <= 0) return false;
            var stack = items.Find(x => x.itemId == itemId);
            if (stack == null || stack.quantity < quantity) return false;

            stack.quantity -= quantity;
            if (stack.quantity <= 0) items.Remove(stack);

            OnInventoryChanged?.Invoke();
            return true;
        }

        public int GetQuantity(string itemId)
        {
            var stack = items.Find(x => x.itemId == itemId);
            return stack != null ? stack.quantity : 0;
        }

        public bool UseItem(string itemId)
        {
            var stack = items.Find(x => x.itemId == itemId);
            if (stack == null || stack.quantity <= 0) return false;

            // Apply item effect
            if (itemId == "health_pack" && health != null)
            {
                health.Restore(40);
            }
            else if (itemId == "energy_vial" && abilityUser != null)
            {
                abilityUser.energy = Mathf.Min(abilityUser.maxEnergy, abilityUser.energy + 50);
            }

            Remove(itemId, 1);
            return true;
        }

        public void SaveTo(KyokaSaveData save)
        {
            if (save == null) return;
            save.inventoryItems.Clear();
            foreach (var item in items)
            {
                save.inventoryItems.Add(new SavedItemStack { itemId = item.itemId, quantity = item.quantity });
            }
        }

        public void LoadFrom(KyokaSaveData save)
        {
            if (save == null || save.inventoryItems == null) return;
            items.Clear();
            foreach (var item in save.inventoryItems)
            {
                items.Add(new InventoryStack(item.itemId, item.quantity));
            }
            OnInventoryChanged?.Invoke();
        }
    }
}
