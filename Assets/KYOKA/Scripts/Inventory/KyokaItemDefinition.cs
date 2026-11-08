using UnityEngine;

namespace Kyoka.Inventory
{
    public enum ItemCategory
    {
        Consumable,
        Weapon,
        Resource,
        Equipment,
        BuildingMaterial,
        KeyItem
    }

    [CreateAssetMenu(fileName = "NewItem", menuName = "KYOKA/Item Definition")]
    public class KyokaItemDefinition : ScriptableObject
    {
        public string itemId = "health_pack";
        public string displayName = "Medical Nanites";
        [TextArea] public string description = "Restores 40 health over 2 seconds.";
        public ItemCategory category = ItemCategory.Consumable;

        [Header("Values & Stacking")]
        [Min(1)] public int stackLimit = 99;
        public int buyPrice = 50;
        public int sellValue = 25;

        [Header("Consumable Effects")]
        public int healthRestore = 40;
        public int energyRestore = 0;

        [Header("Visual")]
        public Color iconColor = Color.green;
    }
}
