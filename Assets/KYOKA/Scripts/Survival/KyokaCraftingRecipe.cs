using System;
using UnityEngine;

namespace Kyoka.Survival
{
    [Serializable]
    public class KyokaRecipeIngredient
    {
        public string itemId;
        public int amount = 1;
    }

    [CreateAssetMenu(menuName = "KYOKA/Crafting Recipe")]
    public class KyokaCraftingRecipe : ScriptableObject
    {
        public string recipeId;
        public string outputItemId;
        public int outputAmount = 1;
        public KyokaRecipeIngredient[] ingredients;
    }
}
