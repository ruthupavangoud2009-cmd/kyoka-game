using UnityEngine;
using System;
using System.Collections.Generic;
using Kyoka.Player;
using Kyoka.Core;

namespace Kyoka.Abilities
{
    public class KyokaAbilityUser : MonoBehaviour
    {
        [Header("Energy Stats")]
        public float maxEnergy = 100f;
        public float energy = 100f;
        public float energyRegenRate = 8f;

        [Header("Equipped Abilities")]
        public KyokaAbilityDefinition primaryAbility;    // Slot 1 (Q / Touch 1)
        public KyokaAbilityDefinition secondaryAbility;  // Slot 2 (F / Touch 2)
        public KyokaAbilityDefinition ultimateAbility;   // Slot 3 (R)
        public KyokaAbilityDefinition transformation;    // Slot 4 (T)

        [Header("Transformation State")]
        public bool isTransformed = false;
        private float transformTimer = 0f;

        // Cooldown timestamps
        private readonly Dictionary<string, float> cooldownEnds = new Dictionary<string, float>();

        // Components
        private CharacterController characterController;
        private KyokaHealth health;

        // Events
        public event Action<KyokaAbilityDefinition> OnAbilityFired;
        public event Action<bool> OnTransformationStateChanged;

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            health = GetComponent<KyokaHealth>();

            // Assign default abilities from library if null
            var lib = KyokaAbilityLibrary.Instance;
            if (lib != null)
            {
                if (primaryAbility == null) primaryAbility = lib.GetAbility("wind_tempest_dash");
                if (secondaryAbility == null) secondaryAbility = lib.GetAbility("flame_surge");
                if (ultimateAbility == null) ultimateAbility = lib.GetAbility("thunder_strike");
                if (transformation == null) transformation = lib.GetAbility("kyoka_awakening");
            }
        }

        private void Update()
        {
            // Energy regeneration
            float regen = isTransformed ? energyRegenRate * 1.5f : energyRegenRate;
            energy = Mathf.Min(maxEnergy, energy + regen * Time.deltaTime);

            // Transformation timer
            if (isTransformed)
            {
                transformTimer -= Time.deltaTime;
                if (transformTimer <= 0)
                {
                    EndTransformation();
                }
            }

            var input = KyokaInputBridge.Instance;
            if (input == null) return;

            if (input.Ability1Pressed && primaryAbility != null)
            {
                TryCastAbility(primaryAbility);
            }
            if (input.Ability2Pressed && secondaryAbility != null)
            {
                TryCastAbility(secondaryAbility);
            }
            if (input.Ability3Pressed && ultimateAbility != null)
            {
                TryCastAbility(ultimateAbility);
            }
            if (input.TransformPressed && transformation != null)
            {
                TryCastAbility(transformation);
            }
        }

        public bool IsOnCooldown(KyokaAbilityDefinition ability)
        {
            if (ability == null) return false;
            return cooldownEnds.TryGetValue(ability.abilityId, out float readyTime) && Time.time < readyTime;
        }

        public float GetRemainingCooldown(KyokaAbilityDefinition ability)
        {
            if (ability == null) return 0f;
            if (cooldownEnds.TryGetValue(ability.abilityId, out float readyTime))
            {
                return Mathf.Max(0f, readyTime - Time.time);
            }
            return 0f;
        }

        public bool TryCastAbility(KyokaAbilityDefinition ability)
        {
            if (ability == null) return false;
            if (energy < ability.energyCost) return false;
            if (IsOnCooldown(ability)) return false;

            // Consume energy and start cooldown
            energy -= ability.energyCost;
            float cd = isTransformed ? ability.cooldown * 0.6f : ability.cooldown;
            cooldownEnds[ability.abilityId] = Time.time + cd;

            ExecuteAbilityEffect(ability);
            OnAbilityFired?.Invoke(ability);
            return true;
        }

        private void ExecuteAbilityEffect(KyokaAbilityDefinition ability)
        {
            switch (ability.abilityType)
            {
                case AbilityType.Dash:
                    ExecuteDash(ability);
                    break;
                case AbilityType.Projectile:
                    ExecuteProjectile(ability);
                    break;
                case AbilityType.TargetedArea:
                    ExecuteAreaStrike(ability);
                    break;
                case AbilityType.Shield:
                    ExecuteShield(ability);
                    break;
                case AbilityType.HealArea:
                    ExecuteHeal(ability);
                    break;
                case AbilityType.Transformation:
                    ExecuteTransformation(ability);
                    break;
            }
        }

        private void ExecuteDash(KyokaAbilityDefinition ability)
        {
            Vector3 dashDirection = transform.forward;
            if (characterController != null)
            {
                characterController.Move(dashDirection * ability.dashSpeed * 0.25f);
            }

            // Wind trail visual
            var vfx = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            vfx.name = "TempestDash_VFX";
            vfx.transform.position = transform.position + Vector3.up * 0.8f;
            vfx.transform.localScale = new Vector3(2f, 0.1f, 2f);
            var r = vfx.GetComponent<Renderer>();
            if (r) r.material.color = ability.effectColor;
            var col = vfx.GetComponent<Collider>();
            if (col) Destroy(col);
            Destroy(vfx, 0.4f);

            // Damage enemies in dash path
            Collider[] hits = Physics.OverlapSphere(transform.position, ability.range);
            foreach (var h in hits)
            {
                if (h.transform == transform) continue;
                var enemyHealth = h.GetComponent<KyokaHealth>();
                if (enemyHealth) enemyHealth.ApplyGameDamage(ability.gamePower);
            }
        }

        private void ExecuteProjectile(KyokaAbilityDefinition ability)
        {
            Vector3 spawnPos = transform.position + transform.forward * 1.5f + Vector3.up * 1.2f;
            var projGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projGO.name = "Ability_FlameSurge";
            projGO.transform.position = spawnPos;
            projGO.transform.localScale = Vector3.one * 0.8f;

            var rend = projGO.GetComponent<Renderer>();
            if (rend) rend.material.color = ability.effectColor;

            var proj = projGO.AddComponent<Kyoka.Combat.KyokaProjectile>();
            proj.Initialize(transform, transform.forward, ability.gamePower, 30f, 3.5f);
        }

        private void ExecuteAreaStrike(KyokaAbilityDefinition ability)
        {
            Vector3 targetCenter = transform.position + transform.forward * 8f;

            // Pillar visual
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "ThunderStrike_Pillar";
            pillar.transform.position = targetCenter + Vector3.up * 6f;
            pillar.transform.localScale = new Vector3(ability.range * 0.8f, 12f, ability.range * 0.8f);
            var r = pillar.GetComponent<Renderer>();
            if (r) r.material.color = ability.effectColor;
            var col = pillar.GetComponent<Collider>();
            if (col) Destroy(col);
            Destroy(pillar, 0.5f);

            // Ground burst damage
            Collider[] hits = Physics.OverlapSphere(targetCenter, ability.range);
            foreach (var h in hits)
            {
                if (h.transform == transform) continue;
                var targetHealth = h.GetComponent<KyokaHealth>();
                if (targetHealth) targetHealth.ApplyGameDamage(ability.gamePower);
            }
        }

        private void ExecuteShield(KyokaAbilityDefinition ability)
        {
            var shield = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shield.name = "VoidShield_Barrier";
            shield.transform.SetParent(transform);
            shield.transform.localPosition = Vector3.up * 0.9f;
            shield.transform.localScale = Vector3.one * 2.8f;

            var r = shield.GetComponent<Renderer>();
            if (r)
            {
                Color c = ability.effectColor;
                c.a = 0.4f;
                r.material.color = c;
            }
            var col = shield.GetComponent<Collider>();
            if (col) Destroy(col);

            Destroy(shield, ability.buffDuration);
        }

        private void ExecuteHeal(KyokaAbilityDefinition ability)
        {
            if (health != null)
            {
                health.Restore(ability.gamePower);
            }

            // Healing spring visual
            var aura = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            aura.name = "TideBlossom_Aura";
            aura.transform.position = transform.position;
            aura.transform.localScale = new Vector3(ability.range * 2f, 0.05f, ability.range * 2f);
            var r = aura.GetComponent<Renderer>();
            if (r) r.material.color = ability.effectColor;
            var col = aura.GetComponent<Collider>();
            if (col) Destroy(col);
            Destroy(aura, ability.buffDuration);
        }

        private void ExecuteTransformation(KyokaAbilityDefinition ability)
        {
            isTransformed = true;
            transformTimer = ability.buffDuration;
            OnTransformationStateChanged?.Invoke(true);

            // Aura ring visual
            var aura = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            aura.name = "Awakening_Aura";
            aura.transform.SetParent(transform);
            aura.transform.localPosition = Vector3.up * 1f;
            aura.transform.localScale = Vector3.one * 2.2f;
            var r = aura.GetComponent<Renderer>();
            if (r) r.material.color = ability.effectColor;
            var col = aura.GetComponent<Collider>();
            if (col) Destroy(col);
            Destroy(aura, ability.buffDuration);
        }

        private void EndTransformation()
        {
            isTransformed = false;
            OnTransformationStateChanged?.Invoke(false);
        }
    }
}
