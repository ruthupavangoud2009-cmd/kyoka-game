using UnityEngine;
using System;
using System.Collections.Generic;
using Kyoka.Inventory;

namespace Kyoka.Economy
{
    [Serializable]
    public class ShopListing
    {
        public string itemId;
        public string displayName;
        public int buyPrice;
        public int stockCount = 99;
    }

    public class KyokaShopManager : MonoBehaviour
    {
        public static KyokaShopManager Instance { get; private set; }

        public List<ShopListing> activeListings = new List<ShopListing>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            InitializeDefaultShop();
        }

        private void InitializeDefaultShop()
        {
            if (activeListings.Count == 0)
            {
                activeListings.Add(new ShopListing { itemId = "health_pack", displayName = "Medical Nanites", buyPrice = 40 });
                activeListings.Add(new ShopListing { itemId = "energy_vial", displayName = "Aether Infusion", buyPrice = 60 });
                activeListings.Add(new ShopListing { itemId = "wood", displayName = "Hardwood Planks", buyPrice = 10 });
                activeListings.Add(new ShopListing { itemId = "stone", displayName = "Granite Blocks", buyPrice = 15 });
            }
        }

        public bool BuyItem(string itemId, KyokaInventory playerInventory)
        {
            var item = activeListings.Find(x => x.itemId == itemId);
            if (item == null || !playerInventory) return false;

            var eco = KyokaEconomyManager.Instance;
            if (eco == null || eco.KyokaCredits < item.buyPrice)
            {
                Debug.LogWarning("[KYOKA Shop] Insufficient credits.");
                return false;
            }

            if (!eco.SpendCredits(item.buyPrice)) return false;

            playerInventory.Add(item.itemId, 1);
            Debug.Log($"[KYOKA Shop] Purchased {item.displayName} for {item.buyPrice} credits.");
            return true;
        }

        public bool SellItem(string itemId, int quantity, KyokaInventory playerInventory)
        {
            if (!playerInventory || playerInventory.GetQuantity(itemId) < quantity) return false;

            var item = activeListings.Find(x => x.itemId == itemId);
            int unitPrice = item != null ? Mathf.RoundToInt(item.buyPrice * 0.5f) : 10;
            int totalGain = unitPrice * quantity;

            playerInventory.Remove(itemId, quantity);
            var eco = KyokaEconomyManager.Instance;
            if (eco != null) eco.AddCredits(totalGain, "ShopSale");

            Debug.Log($"[KYOKA Shop] Sold {quantity}x {itemId} for {totalGain} credits.");
            return true;
        }
    }
}
