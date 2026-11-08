using UnityEngine;

namespace Kyoka.Inventory
{
    [CreateAssetMenu(menuName = "KYOKA/Item Definition")]
    public class KyokaItemDefinition : ScriptableObject
    {
        public string itemId;
        public string displayName;
        [TextArea] public string description;
        [Min(0)] public int stackLimit = 99;
    }
}
