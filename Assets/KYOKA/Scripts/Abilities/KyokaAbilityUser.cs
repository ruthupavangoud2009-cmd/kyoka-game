using UnityEngine;
using System.Collections.Generic;
using Kyoka.Player;

namespace Kyoka.Abilities
{
    public class KyokaAbilityUser : MonoBehaviour
    {
        public float maxEnergy = 100;
        public float energy = 100;
        private readonly Dictionary<string, float> nextUse = new Dictionary<string, float>();
        private KyokaAbilityDefinition sample;

        private void Start()
        {
            sample = KyokaAbilityLibrary.Instance != null
                ? KyokaAbilityLibrary.Instance.CreateRuntimeSample()
                : ScriptableObject.CreateInstance<KyokaAbilityDefinition>();
        }

        private void Update()
        {
            energy = Mathf.Min(maxEnergy, energy + 5f * Time.deltaTime);
            if (Input.GetKeyDown(KeyCode.Q)) TryUseSampleAbility();
        }

        public bool TryUseSampleAbility()
        {
            if (!sample || energy < sample.energyCost) return false;
            float readyAt;
            if (nextUse.TryGetValue(sample.abilityId, out readyAt) && Time.time < readyAt) return false;

            energy -= sample.energyCost;
            nextUse[sample.abilityId] = Time.time + sample.cooldown;

            var effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            effect.name = "AbilityEffect_VisualOnly";
            effect.transform.position = transform.position + transform.forward * 2f + Vector3.up;
            effect.transform.localScale = Vector3.one * 1.1f;
            var renderer = effect.GetComponent<Renderer>();
            renderer.material.color = sample.effectColor;
            Destroy(effect.GetComponent<Collider>());
            Destroy(effect, 0.35f);

            var hits = Physics.OverlapSphere(transform.position + transform.forward * 2f, sample.range);
            foreach (var hit in hits)
            {
                if (hit.transform == transform) continue;
                var health = hit.GetComponent<KyokaHealth>();
                if (health) health.ApplyGameDamage(sample.gamePower);
            }
            return true;
        }
    }
}
