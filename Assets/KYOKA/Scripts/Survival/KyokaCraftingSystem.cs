using UnityEngine;
using System;
using System.Collections.Generic;
using Kyoka.Inventory;

namespace Kyoka.Survival
{
    public class KyokaCraftingSystem : MonoBehaviour
    {
        public static KyokaCraftingSystem Instance { get; private set; }

        public List<KyokaCraftingRecipe> recipes = new List<KyokaCraftingRecipe>();

        public event Action<KyokaCraftingRecipe> OnItemCrafted;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            InitializeDefaultRecipes();
        }

        private void InitializeDefaultRecipes()
        {
            if (recipes.Count == 0)
            {
                recipes.Add(CreateRecipe("craft_health_pack", "health_pack", 1, new[] {
                    new KyokaRecipeIngredient { itemId = "wood", amount = 2 },
                    new KyokaRecipeIngredient { itemId = "stone", amount = 1 }
                }));

                recipes.Add(CreateRecipe("craft_energy_vial", "energy_vial", 1, new[] {
                    new KyokaRecipeIngredient { itemId = "stone", amount = 3 }
                }));
            }
        }

        private KyokaCraftingRecipe CreateRecipe(string id, string outId, int outAmt, KyokaRecipeIngredient[] ing)
        {
            var r = ScriptableObject.CreateInstance<KyokaCraftingRecipe>();
            r.recipeId = id;
            r.outputItemId = outId;
            r.outputAmount = outAmt;
            r.ingredients = ing;
            return r;
        }

        public bool CanCraft(KyokaCraftingRecipe recipe, KyokaInventory inventory)
        {
            if (recipe == null || !inventory || recipe.ingredients == null) return false;
            foreach (var ing in recipe.ingredients)
            {
                if (inventory.GetQuantity(ing.itemId) < ing.amount) return false;
            }
            return true;
        }

        public bool Craft(KyokaCraftingRecipe recipe, KyokaInventory inventory)
        {
            if (!CanCraft(recipe, inventory)) return false;

            // Consume ingredients
            foreach (var ing in recipe.ingredients)
            {
                inventory.Remove(ing.itemId, ing.amount);
            }

            // Grant output
            inventory.Add(recipe.outputItemId, recipe.outputAmount);
            OnItemCrafted?.Invoke(recipe);
            Debug.Log($"[KYOKA Crafting] Successfully crafted {recipe.outputAmount}x {recipe.outputItemId}");
            return true;
        }
    }
}

