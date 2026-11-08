using UnityEngine;
using System;
using Kyoka.Player;
using Kyoka.Economy;
using Kyoka.Missions;
using Kyoka.Audio;

namespace Kyoka.Combat
{
    public enum BossPhase
    {
        Phase1_Physical,
        Phase2_ElementalStorm,
        Phase3_AwakenedVoid
    }

    /// <summary>
    /// Multi-phase Anime Realm boss encounter: "The Celestial Sovereign".
    /// Features dynamic phase transitions, elemental barrages, and heroic rewards.
    /// </summary>
    public class KyokaBossController : MonoBehaviour
    {
        [Header("Boss Identity")]
        public string bossName = "The Celestial Sovereign";
        public BossPhase currentPhase = BossPhase.Phase1_Physical;

        [Header("Combat Stats")]
        public int phase1Damage = 25;
        public int phase2Damage = 35;
        public int phase3Damage = 50;
        public float attackCooldown = 2.0f;
        public float moveSpeed = 5.0f;

        [Header("Rewards")]
        public int rewardCredits = 1500;
        public int rewardShards = 50;

        private KyokaHealth health;
        private CharacterController controller;
        private Transform playerTarget;
        private float lastAttackTime = -10f;
        private bool isDefeated = false;

        public event Action<BossPhase> OnBossPhaseTransition;

        private void Awake()
        {
            health = GetComponent<KyokaHealth>();
            if (!health) health = gameObject.AddComponent<KyokaHealth>();
            health.maxHealth = 600;
            health.ResetHealth();

            controller = GetComponent<CharacterController>();
            if (!controller) controller = gameObject.AddComponent<CharacterController>();
        }

        private void Start()
        {
            health.HealthChanged += CheckPhaseTransitions;
            health.Defeated += OnBossDefeated;
        }

        private void Update()
        {
            if (isDefeated) return;

            if (playerTarget == null)
            {
                var player = FindObjectOfType<KyokaPlayerController>();
                if (player != null) playerTarget = player.transform;
            }

            if (playerTarget != null)
            {
                float distance = Vector3.Distance(transform.position, playerTarget.position);

                // Face target
                Vector3 dir = (playerTarget.position - transform.position);
                dir.y = 0;
                if (dir.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir.normalized), 5f * Time.deltaTime);
                }

                if (distance > 3.5f)
                {
                    controller.Move(dir.normalized * moveSpeed * Time.deltaTime + Vector3.down * 9.8f * Time.deltaTime);
                }
                else
                {
                    ExecutePhaseAttack();
                }
            }
        }

        private void CheckPhaseTransitions(int curHp, int maxHp)
        {
            float hpPct = (float)curHp / maxHp;

            if (hpPct <= 0.33f && currentPhase != BossPhase.Phase3_AwakenedVoid)
            {
                currentPhase = BossPhase.Phase3_AwakenedVoid;
                moveSpeed = 7.0f;
                attackCooldown = 1.2f;
                OnBossPhaseTransition?.Invoke(currentPhase);
                Debug.Log($"[KYOKA Boss] {bossName} entered PHASE 3: AWAKENED VOID!");
            }
            else if (hpPct <= 0.66f && currentPhase == BossPhase.Phase1_Physical)
            {
                currentPhase = BossPhase.Phase2_ElementalStorm;
                moveSpeed = 6.0f;
                attackCooldown = 1.6f;
                OnBossPhaseTransition?.Invoke(currentPhase);
                Debug.Log($"[KYOKA Boss] {bossName} entered PHASE 2: ELEMENTAL STORM!");
            }
        }

        private void ExecutePhaseAttack()
        {
            if (Time.time - lastAttackTime < attackCooldown) return;
            lastAttackTime = Time.time;

            switch (currentPhase)
            {
                case BossPhase.Phase1_Physical:
                    ExecuteGroundSlam();
                    break;
                case BossPhase.Phase2_ElementalStorm:
                    ExecuteElementalStorm();
                    break;
                case BossPhase.Phase3_AwakenedVoid:
                    ExecuteVoidCataclysm();
                    break;
            }
        }

        private void ExecuteGroundSlam()
        {
            if (playerTarget == null) return;
            var targetHealth = playerTarget.GetComponent<KyokaHealth>();
            if (targetHealth) targetHealth.ApplyGameDamage(phase1Damage);

            // Shockwave visual
            var wave = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wave.transform.position = transform.position;
            wave.transform.localScale = new Vector3(8f, 0.1f, 8f);
            var r = wave.GetComponent<Renderer>();
            if (r) r.material.color = Color.yellow;
            Destroy(wave.GetComponent<Collider>());
            Destroy(wave, 0.4f);

            if (KyokaAudioManager.Instance != null)
                KyokaAudioManager.Instance.PlaySFX(SoundEffectType.Explosion, transform.position);
        }

        private void ExecuteElementalStorm()
        {
            // Fire 3 fireballs outward
            for (int i = -1; i <= 1; i++)
            {
                Vector3 shootDir = Quaternion.Euler(0, i * 25f, 0) * transform.forward;
                var projGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                projGO.transform.position = transform.position + shootDir * 2f + Vector3.up * 1.5f;
                projGO.transform.localScale = Vector3.one * 0.7f;
                var r = projGO.GetComponent<Renderer>();
                if (r) r.material.color = new Color(1f, 0.4f, 0.1f);

                var proj = projGO.AddComponent<KyokaProjectile>();
                proj.Initialize(transform, shootDir, phase2Damage, 25f, 2.5f);
            }

            if (KyokaAudioManager.Instance != null)
                KyokaAudioManager.Instance.PlaySFX(SoundEffectType.AbilityCast, transform.position);
        }

        private void ExecuteVoidCataclysm()
        {
            ExecuteGroundSlam();
            ExecuteElementalStorm();

            // Additional dark vortex visual
            var vortex = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            vortex.transform.position = transform.position + Vector3.up * 1.5f;
            vortex.transform.localScale = Vector3.one * 4f;
            var r = vortex.GetComponent<Renderer>();
            if (r) r.material.color = new Color(0.4f, 0.1f, 0.6f);
            Destroy(vortex.GetComponent<Collider>());
            Destroy(vortex, 0.6f);
        }

        private void OnBossDefeated()
        {
            isDefeated = true;
            Debug.Log($"[KYOKA Boss] {bossName} HAS BEEN DEFEATED! Victory!");

            var eco = KyokaEconomyManager.Instance;
            if (eco != null)
            {
                eco.AddCredits(rewardCredits, "BossVictory_" + bossName);
                eco.AddShards(rewardShards);
            }

            var mm = KyokaMissionManager.Instance;
            if (mm != null)
            {
                mm.AddProgress("m_story_02", 1);
            }

            if (KyokaAudioManager.Instance != null)
                KyokaAudioManager.Instance.PlaySFX(SoundEffectType.RewardChime);

            Destroy(gameObject, 3.0f);
        }

        private void OnGUI()
        {
            if (isDefeated || health == null) return;

            // Draw Boss Health Bar on top center
            float w = Screen.width;
            float barW = 400f;
            float barH = 26f;
            float x = (w - barW) * 0.5f;
            float y = 65f;

            GUI.Box(new Rect(x - 5, y - 22, barW + 10, barH + 28), "");
            GUI.Label(new Rect(x, y - 20, barW, 20), $"{bossName} ({currentPhase})");

            float hpPct = (float)health.CurrentHealth / Mathf.Max(1, health.maxHealth);
            GUI.Box(new Rect(x, y, barW, barH), "");

            Color phaseColor = currentPhase == BossPhase.Phase3_AwakenedVoid ? Color.magenta : (currentPhase == BossPhase.Phase2_ElementalStorm ? Color.red : Color.yellow);
            var prevCol = GUI.color;
            GUI.color = phaseColor;
            GUI.DrawTexture(new Rect(x + 1, y + 1, (barW - 2) * hpPct, barH - 2), Texture2D.whiteTexture);
            GUI.color = prevCol;
        }
    }
}
