using UnityEngine;

namespace Kyoka.Abilities
{
    public enum AbilityElement
    {
        Wind,
        Fire,
        Lightning,
        Void,
        Water
    }

    public enum AbilityType
    {
        Projectile,
        Dash,
        TargetedArea,
        Shield,
        HealArea,
        Transformation
    }

    [CreateAssetMenu(fileName = "NewAbility", menuName = "KYOKA/Ability Definition")]
    public class KyokaAbilityDefinition : ScriptableObject
    {
        public string abilityId = "wind_burst";
        public string displayName = "Wind Burst";
        [TextArea] public string description = "A fictional short-range wind energy burst.";
        public AbilityElement element = AbilityElement.Wind;
        public AbilityType abilityType = AbilityType.Projectile;

        [Header("Costs & Timing")]
        [Min(0)] public float cooldown = 4f;
        [Min(0)] public float energyCost = 15f;
        [Min(0)] public float range = 5f;
        [Min(0)] public int gamePower = 20;

        [Header("Visual & Aesthetic")]
        public Color effectColor = Color.cyan;
        public float visualDuration = 0.5f;
        public float dashSpeed = 25f;
        public float buffDuration = 10f;
    }
}
