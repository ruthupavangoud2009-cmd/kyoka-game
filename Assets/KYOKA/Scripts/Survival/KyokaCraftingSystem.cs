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
                // Basic Consumables & Medicine
                recipes.Add(CreateRecipe("craft_health_pack", "health_pack", 1, new[] {
                    new KyokaRecipeIngredient { itemId = "fiber", amount = 2 },
                    new KyokaRecipeIngredient { itemId = "sapling", amount = 1 }
                }));

                recipes.Add(CreateRecipe("craft_energy_vial", "energy_vial", 1, new[] {
                    new KyokaRecipeIngredient { itemId = "crystal_dust", amount = 2 },
                    new KyokaRecipeIngredient { itemId = "cobblestone", amount = 1 }
                }));

                // Harvesting Tools
                recipes.Add(CreateRecipe("craft_stone_pickaxe", "stone_pickaxe", 1, new[] {
                    new KyokaRecipeIngredient { itemId = "wood_log", amount = 3 },
                    new KyokaRecipeIngredient { itemId = "cobblestone", amount = 4 }
                }));

                recipes.Add(CreateRecipe("craft_iron_katana", "iron_katana", 1, new[] {
                    new KyokaRecipeIngredient { itemId = "wood_log", amount = 2 },
                    new KyokaRecipeIngredient { itemId = "iron_ore", amount = 5 }
                }));

                // Modular Base Building Structures
                recipes.Add(CreateRecipe("craft_wood_foundation", "wood_foundation", 1, new[] {
                    new KyokaRecipeIngredient { itemId = "wood_log", amount = 4 }
                }));

                recipes.Add(CreateRecipe("craft_wood_wall", "wood_wall", 1, new[] {
                    new KyokaRecipeIngredient { itemId = "wood_log", amount = 3 }
                }));

                recipes.Add(CreateRecipe("craft_wood_doorway", "wood_doorway", 1, new[] {
                    new KyokaRecipeIngredient { itemId = "wood_log", amount = 3 },
                    new KyokaRecipeIngredient { itemId = "cobblestone", amount = 1 }
                }));

                recipes.Add(CreateRecipe("craft_wood_roof", "wood_roof", 1, new[] {
                    new KyokaRecipeIngredient { itemId = "wood_log", amount = 2 },
                    new KyokaRecipeIngredient { itemId = "fiber", amount = 2 }
                }));

                recipes.Add(CreateRecipe("craft_storage_chest", "storage_chest", 1, new[] {
                    new KyokaRecipeIngredient { itemId = "wood_log", amount = 4 },
                    new KyokaRecipeIngredient { itemId = "flint", amount = 2 }
                }));

                recipes.Add(CreateRecipe("craft_defensive_barricade", "defensive_barricade", 1, new[] {
                    new KyokaRecipeIngredient { itemId = "wood_log", amount = 2 },
                    new KyokaRecipeIngredient { itemId = "cobblestone", amount = 3 }
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

