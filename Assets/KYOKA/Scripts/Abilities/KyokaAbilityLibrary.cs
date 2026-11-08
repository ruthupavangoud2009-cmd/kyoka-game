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
        }

        public KyokaAbilityDefinition CreateRuntimeSample()
        {
            var ability = ScriptableObject.CreateInstance<KyokaAbilityDefinition>();
            ability.abilityId = "starter_energy_pulse";
            ability.displayName = "Energy Pulse";
            ability.description = "A fictional training ability for the KYOKA prototype.";
            ability.cooldown = 3f;
            ability.energyCost = 10f;
            ability.range = 4f;
            ability.gamePower = 10;
            ability.effectColor = new Color(0.2f, 0.8f, 1f);
            return ability;
        }
    }
}
