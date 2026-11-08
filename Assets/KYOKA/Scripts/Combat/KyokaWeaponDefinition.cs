using UnityEngine;

namespace Kyoka.Combat
{
    public enum WeaponType
    {
        Melee,
        RangedPistol,
        RangedRifle,
        RangedSniper,
        HeavyEnergy
    }

    [CreateAssetMenu(fileName = "NewWeapon", menuName = "KYOKA/Weapon Definition")]
    public class KyokaWeaponDefinition : ScriptableObject
    {
        public string weaponId = "katana_aether";
        public string displayName = "Aether Katana";
        public WeaponType weaponType = WeaponType.Melee;

        [Header("Damage & Stats")]
        [Min(1)] public int baseDamage = 25;
        [Min(0.05f)] public float attackCooldown = 0.4f;
        [Min(0.5f)] public float attackRange = 3.0f;
        [Range(0f, 1f)] public float critChance = 0.15f;
        public float critMultiplier = 2.0f;

        [Header("Ranged Specifics")]
        public int magazineCapacity = 30;
        public float reloadDuration = 1.8f;
        public float projectileSpeed = 80.0f;
        public float recoilPitch = 1.2f;

        [Header("Energy & Stamina")]
        public float energyCost = 5.0f;

        [Header("Visuals")]
        public Color weaponThemeColor = Color.cyan;
    }
}
