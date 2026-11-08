using UnityEngine;
using System.Collections.Generic;

namespace Kyoka.Abilities
{
    public class KyokaAbilityLibrary : MonoBehaviour
    {
        public static KyokaAbilityLibrary Instance { get; private set; }
        public List<KyokaAbilityDefinition> definitions = new List<KyokaAbilityDefinition>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            InitializeDefaultAbilities();
        }

        private void InitializeDefaultAbilities()
        {
            if (definitions.Count == 0)
            {
                definitions.Add(CreateWindDash());
                definitions.Add(CreateFireSurge());
                definitions.Add(CreateThunderStrike());
                definitions.Add(CreateVoidShield());
                definitions.Add(CreateTideHeal());
                definitions.Add(CreateKyokaAwakening());
            }
        }

        public KyokaAbilityDefinition GetAbility(string id)
        {
            return definitions.Find(a => a != null && a.abilityId == id);
        }

        public KyokaAbilityDefinition CreateRuntimeSample()
        {
            return definitions.Count > 0 ? definitions[0] : CreateWindDash();
        }

        public KyokaAbilityDefinition CreateWindDash()
        {
            var a = ScriptableObject.CreateInstance<KyokaAbilityDefinition>();
            a.abilityId = "wind_tempest_dash";
            a.displayName = "Tempest Dash";
            a.description = "A rapid burst of wind propelling the player forward and slashing nearby foes.";
            a.element = AbilityElement.Wind;
            a.abilityType = AbilityType.Dash;
            a.cooldown = 3.5f;
            a.energyCost = 15f;
            a.range = 7f;
            a.gamePower = 25;
            a.dashSpeed = 26f;
            a.effectColor = new Color(0.25f, 0.9f, 0.8f);
            return a;
        }

        public KyokaAbilityDefinition CreateFireSurge()
        {
            var a = ScriptableObject.CreateInstance<KyokaAbilityDefinition>();
            a.abilityId = "flame_surge";
            a.displayName = "Flame Surge";
            a.description = "Launches an orb of condensed flame that explodes on impact.";
            a.element = AbilityElement.Fire;
            a.abilityType = AbilityType.Projectile;
            a.cooldown = 4.5f;
            a.energyCost = 25f;
            a.range = 20f;
            a.gamePower = 40;
            a.effectColor = new Color(1.0f, 0.45f, 0.1f);
            return a;
        }

        public KyokaAbilityDefinition CreateThunderStrike()
        {
            var a = ScriptableObject.CreateInstance<KyokaAbilityDefinition>();
            a.abilityId = "thunder_strike";
            a.displayName = "Thunder Strike";
            a.description = "Calls down celestial lightning on target forward ground area.";
            a.element = AbilityElement.Lightning;
            a.abilityType = AbilityType.TargetedArea;
            a.cooldown = 6.0f;
            a.energyCost = 30f;
            a.range = 15f;
            a.gamePower = 55;
            a.effectColor = new Color(0.9f, 0.95f, 0.3f);
            return a;
        }

        public KyokaAbilityDefinition CreateVoidShield()
        {
            var a = ScriptableObject.CreateInstance<KyokaAbilityDefinition>();
            a.abilityId = "void_shield";
            a.displayName = "Void Barrier";
            a.description = "Summons a cosmic sphere deflecting oncoming attacks.";
            a.element = AbilityElement.Void;
            a.abilityType = AbilityType.Shield;
            a.cooldown = 8.0f;
            a.energyCost = 25f;
            a.range = 2.5f;
            a.gamePower = 10;
            a.buffDuration = 5.0f;
            a.effectColor = new Color(0.6f, 0.15f, 0.9f);
            return a;
        }

        public KyokaAbilityDefinition CreateTideHeal()
        {
            var a = ScriptableObject.CreateInstance<KyokaAbilityDefinition>();
            a.abilityId = "tide_blossom";
            a.displayName = "Tide Blossom";
            a.description = "Generates a sanctuary of soothing ocean water restoring health.";
            a.element = AbilityElement.Water;
            a.abilityType = AbilityType.HealArea;
            a.cooldown = 10.0f;
            a.energyCost = 35f;
            a.range = 4.0f;
            a.gamePower = 30; // Healing amount
            a.buffDuration = 4.0f;
            a.effectColor = new Color(0.1f, 0.6f, 0.95f);
            return a;
        }

        public KyokaAbilityDefinition CreateKyokaAwakening()
        {
            var a = ScriptableObject.CreateInstance<KyokaAbilityDefinition>();
            a.abilityId = "kyoka_awakening";
            a.displayName = "Kyoka Awakening";
            a.description = "Unlock latent spiritual resonance, boosting speed and power.";
            a.element = AbilityElement.Void;
            a.abilityType = AbilityType.Transformation;
            a.cooldown = 30.0f;
            a.energyCost = 50f;
            a.buffDuration = 12.0f;
            a.effectColor = new Color(0.95f, 0.85f, 0.2f);
            return a;
        }
    }
}
