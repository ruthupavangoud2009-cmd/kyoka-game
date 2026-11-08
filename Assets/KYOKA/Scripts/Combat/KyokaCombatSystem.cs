using UnityEngine;
using System;
using Kyoka.Core;
using Kyoka.Player;
using Kyoka.Abilities;

namespace Kyoka.Combat
{
    public class KyokaCombatSystem : MonoBehaviour
    {
        [Header("Equipped Weapon")]
        [SerializeField] private KyokaWeaponDefinition equippedWeapon;
        public KyokaWeaponDefinition EquippedWeapon => equippedWeapon;

        [Header("Combo State")]
        private int currentComboIndex = 0;
        private float lastAttackTime = -10f;
        public float comboResetWindow = 1.2f;

        [Header("Defense / Guard")]
        public bool IsGuarding { get; private set; }
        [Range(0.1f, 0.9f)] public float guardDamageReduction = 0.65f;
        public float guardEnergyDrainPerSec = 10f;

        [Header("Ranged State")]
        public int currentAmmo = 30;
        public bool isReloading = false;
        private float reloadTimer = 0f;

        // Dependencies
        private KyokaAbilityUser abilityUser;
        private KyokaHealth health;

        public event Action<int> OnComboStep;
        public event Action<int, int> OnAmmoChanged;
        public event Action<bool> OnGuardChanged;

        private void Awake()
        {
            abilityUser = GetComponent<KyokaAbilityUser>();
            health = GetComponent<KyokaHealth>();

            if (equippedWeapon == null)
            {
                equippedWeapon = CreateDefaultMeleeWeapon();
            }
            if (equippedWeapon != null)
            {
                currentAmmo = equippedWeapon.magazineCapacity;
            }
        }

        private void Update()
        {
            var input = KyokaInputBridge.Instance;
            if (input == null) return;

            // Guarding
            bool shouldGuard = input.DefendHeld;
            if (shouldGuard && abilityUser != null && abilityUser.energy > 5f)
            {
                if (!IsGuarding)
                {
                    IsGuarding = true;
                    OnGuardChanged?.Invoke(true);
                }
                abilityUser.energy = Mathf.Max(0, abilityUser.energy - guardEnergyDrainPerSec * Time.deltaTime);
            }
            else
            {
                if (IsGuarding)
                {
                    IsGuarding = false;
                    OnGuardChanged?.Invoke(false);
                }
            }

            // Reloading
            if (isReloading)
            {
                reloadTimer -= Time.deltaTime;
                if (reloadTimer <= 0)
                {
                    isReloading = false;
                    if (equippedWeapon != null) currentAmmo = equippedWeapon.magazineCapacity;
                    OnAmmoChanged?.Invoke(currentAmmo, equippedWeapon != null ? equippedWeapon.magazineCapacity : 0);
                }
            }

            // Attack
            if (input.AttackPressed && !IsGuarding && !isReloading)
            {
                ExecuteAttack();
            }

            // Reset combo if too much time passed
            if (Time.time - lastAttackTime > comboResetWindow)
            {
                currentComboIndex = 0;
            }
        }

        public void EquipWeapon(KyokaWeaponDefinition weapon)
        {
            if (weapon == null) return;
            equippedWeapon = weapon;
            currentAmmo = weapon.magazineCapacity;
            isReloading = false;
            OnAmmoChanged?.Invoke(currentAmmo, weapon.magazineCapacity);
        }

        public void ExecuteAttack()
        {
            if (equippedWeapon == null) return;
            if (Time.time - lastAttackTime < equippedWeapon.attackCooldown) return;

            // Check energy cost
            if (abilityUser != null && abilityUser.energy < equippedWeapon.energyCost) return;
            if (abilityUser != null) abilityUser.energy -= equippedWeapon.energyCost;

            lastAttackTime = Time.time;

            if (equippedWeapon.weaponType == WeaponType.Melee)
            {
                ExecuteMeleeAttack();
            }
            else
            {
                ExecuteRangedAttack();
            }
        }

        private void ExecuteMeleeAttack()
        {
            currentComboIndex = (currentComboIndex % 3) + 1;
            OnComboStep?.Invoke(currentComboIndex);

            // Forward hit box
            Vector3 origin = transform.position + Vector3.up * 1.0f;
            Vector3 attackDir = transform.forward;
            Collider[] hits = Physics.OverlapSphere(origin + attackDir * (equippedWeapon.attackRange * 0.5f), equippedWeapon.attackRange * 0.6f);

            int comboMultiplier = currentComboIndex == 3 ? 2 : 1; // 3rd hit is a finisher
            int damage = equippedWeapon.baseDamage * comboMultiplier;

            // Spawn sword slash trail visual
            SpawnSlashVFX(origin + attackDir * 1.2f, equippedWeapon.weaponThemeColor);

            foreach (var h in hits)
            {
                if (h.transform == transform) continue;
                var targetHealth = h.GetComponent<KyokaHealth>();
                if (targetHealth)
                {
                    targetHealth.ApplyGameDamage(damage);
                    Debug.Log($"[KYOKA Combat] Melee hit {h.name} for {damage} dmg (Combo #{currentComboIndex})");
                }
            }
        }

        private void ExecuteRangedAttack()
        {
            if (currentAmmo <= 0)
            {
                StartReload();
                return;
            }

            currentAmmo--;
            OnAmmoChanged?.Invoke(currentAmmo, equippedWeapon.magazineCapacity);

            Vector3 spawnPos = transform.position + transform.forward * 1.0f + Vector3.up * 1.4f;
            Vector3 shootDir = transform.forward;

            var projGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projGO.name = "Kyoka_RangedBullet";
            projGO.transform.position = spawnPos;
            projGO.transform.localScale = Vector3.one * 0.2f;

            var rend = projGO.GetComponent<Renderer>();
            if (rend) rend.material.color = equippedWeapon.weaponThemeColor;

            var proj = projGO.AddComponent<KyokaProjectile>();
            proj.Initialize(transform, shootDir, equippedWeapon.baseDamage, equippedWeapon.projectileSpeed);

            if (currentAmmo <= 0)
            {
                StartReload();
            }
        }

        public void StartReload()
        {
            if (isReloading || equippedWeapon == null || equippedWeapon.weaponType == WeaponType.Melee) return;
            isReloading = true;
            reloadTimer = equippedWeapon.reloadDuration;
        }

        private void SpawnSlashVFX(Vector3 pos, Color color)
        {
            var slash = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            slash.name = "SlashVFX";
            slash.transform.position = pos;
            slash.transform.localScale = new Vector3(1.2f, 0.05f, 0.4f);
            slash.transform.rotation = transform.rotation * Quaternion.Euler(0, 45f, 90f);
            var r = slash.GetComponent<Renderer>();
            if (r) r.material.color = color;
            var c = slash.GetComponent<Collider>();
            if (c) Destroy(c);
            Destroy(slash, 0.2f);
        }

        public KyokaWeaponDefinition CreateDefaultMeleeWeapon()
        {
            var weapon = ScriptableObject.CreateInstance<KyokaWeaponDefinition>();
            weapon.weaponId = "starter_katana";
            weapon.displayName = "Starter Katana";
            weapon.weaponType = WeaponType.Melee;
            weapon.baseDamage = 25;
            weapon.attackCooldown = 0.35f;
            weapon.attackRange = 2.8f;
            weapon.weaponThemeColor = new Color(0.2f, 0.7f, 1f);
            return weapon;
        }
    }
}
