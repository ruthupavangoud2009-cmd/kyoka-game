using UnityEngine;

namespace Kyoka.Abilities
{
    [CreateAssetMenu(menuName = "KYOKA/Ability Definition")]
    public class KyokaAbilityDefinition : ScriptableObject
    {
        public string abilityId = "wind_burst";
        public string displayName = "Wind Burst";
        [TextArea] public string description = "A fictional short-range wind energy burst.";
        [Min(0)] public float cooldown = 4f;
        [Min(0)] public float energyCost = 15f;
        [Min(0)] public float range = 5f;
        [Min(0)] public int gamePower = 10;
        public Color effectColor = Color.cyan;
    }
}
