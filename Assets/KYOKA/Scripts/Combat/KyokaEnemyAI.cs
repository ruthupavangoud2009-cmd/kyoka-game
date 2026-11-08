using UnityEngine;
using System;
using Kyoka.Player;
using Kyoka.Economy;
using Kyoka.Audio;

namespace Kyoka.Combat
{
    public enum EnemyType
    {
        MeleeGrunt,
        RangedSentry,
        EliteBrute
    }

    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Stunned,
        Defeated
    }

    /// <summary>
    /// Autonomous enemy combatant AI for open world encounters, wildlands beasts,
    /// and dungeon sentinels.
    /// </summary>
    public class KyokaEnemyAI : MonoBehaviour
    {
        [Header("Classification & Stats")]
        public EnemyType enemyType = EnemyType.MeleeGrunt;
        public string enemyName = "Void Drone";
        public int attackDamage = 15;
        public float attackCooldown = 1.6f;
        public float attackRange = 2.2f;
        public float moveSpeed = 4.5f;

        [Header("Perception")]
        public float detectionRadius = 14f;
        public float fieldOfViewAngle = 120f;
        public Transform currentTarget;

        [Header("Rewards on Defeat")]
        public int rewardCredits = 45;
        public int rewardShards = 2;
        public int rewardXP = 50;

        [Header("State")]
        [SerializeField] private EnemyState currentState = EnemyState.Idle;
        public EnemyState CurrentState => currentState;

        private KyokaHealth health;
        private CharacterController controller;
        private float lastAttackTime = -10f;
        private Vector3 spawnPoint;

        public event Action<EnemyState> OnStateChanged;

        private void Awake()
        {
            health = GetComponent<KyokaHealth>();
            if (!health) health = gameObject.AddComponent<KyokaHealth>();

            controller = GetComponent<CharacterController>();
            if (!controller) controller = gameObject.AddComponent<CharacterController>();

            spawnPoint = transform.position;
            ConfigureTypeStats();
        }

        private void Start()
        {
            health.Defeated += OnDefeated;
            health.HealthChanged += OnHurt;
        }

        private void ConfigureTypeStats()
        {
            switch (enemyType)
            {
                case EnemyType.MeleeGrunt:
                    health.maxHealth = 60;
                    attackDamage = 12;
                    moveSpeed = 4.8f;
                    attackRange = 2.2f;
                    break;
                case EnemyType.RangedSentry:
                    health.maxHealth = 45;
                    attackDamage = 18;
                    moveSpeed = 3.2f;
                    attackRange = 12.0f;
                    break;
                case EnemyType.EliteBrute:
                    health.maxHealth = 180;
                    attackDamage = 30;
                    moveSpeed = 3.6f;
                    attackRange = 3.0f;
                    break;
            }
            health.ResetHealth();
        }

        private void Update()
        {
            if (currentState == EnemyState.Defeated) return;

            // Search for player target if none
            if (currentTarget == null)
            {
                FindPlayerTarget();
            }

            if (currentTarget != null)
            {
                float distance = Vector3.Distance(transform.position, currentTarget.position);

                if (distance <= attackRange)
                {
                    currentState = EnemyState.Attack;
                    LookAtTarget();
                    ExecuteAttack();
                }
                else if (distance <= detectionRadius)
                {
                    currentState = EnemyState.Chase;
                    ChaseTarget();
                }
                else
                {
                    // Lost target, return to idle
                    currentState = EnemyState.Idle;
                    currentTarget = null;
                }
            }
        }

        private void FindPlayerTarget()
        {
            var player = FindObjectOfType<KyokaPlayerController>();
            if (player == null) return;

            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist <= detectionRadius)
            {
                Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, dirToPlayer);

                if (angle < fieldOfViewAngle * 0.5f || dist < 4f)
                {
                    currentTarget = player.transform;
                }
            }
        }

        private void ChaseTarget()
        {
            Vector3 direction = (currentTarget.position - transform.position);
            direction.y = 0;

            if (direction.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction.normalized), 8f * Time.deltaTime);
                controller.Move(direction.normalized * moveSpeed * Time.deltaTime + Vector3.down * 9.8f * Time.deltaTime);
            }
        }

        private void LookAtTarget()
        {
            Vector3 dir = (currentTarget.position - transform.position);
            dir.y = 0;
            if (dir.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(dir.normalized);
            }
        }

        private void ExecuteAttack()
        {
            if (Time.time - lastAttackTime < attackCooldown) return;
            lastAttackTime = Time.time;

            if (enemyType == EnemyType.RangedSentry)
            {
                // Ranged attack projectile
                var projGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                projGO.name = "Enemy_EnergyShot";
                projGO.transform.position = transform.position + transform.forward * 1.2f + Vector3.up * 1f;
                projGO.transform.localScale = Vector3.one * 0.35f;
                var r = projGO.GetComponent<Renderer>();
                if (r) r.material.color = Color.magenta;

                var proj = projGO.AddComponent<KyokaProjectile>();
                proj.Initialize(transform, transform.forward, attackDamage, 28f);

                if (KyokaAudioManager.Instance != null)
                    KyokaAudioManager.Instance.PlaySFX(SoundEffectType.RangedShot, transform.position);
            }
            else
            {
                // Melee strike
                var playerHealth = currentTarget.GetComponent<KyokaHealth>();
                if (playerHealth != null)
                {
                    playerHealth.ApplyGameDamage(attackDamage);
                    Debug.Log($"[KYOKA Enemy] {enemyName} struck player for {attackDamage} damage!");

                    if (KyokaAudioManager.Instance != null)
                        KyokaAudioManager.Instance.PlaySFX(SoundEffectType.HitImpact, transform.position);
                }
            }
        }

        private void OnHurt(int currentHp, int maxHp)
        {
            // Turn to face attacker if hit from behind
            if (currentTarget == null)
            {
                var player = FindObjectOfType<KyokaPlayerController>();
                if (player != null) currentTarget = player.transform;
            }
        }

        private void OnDefeated()
        {
            currentState = EnemyState.Defeated;
            Debug.Log($"[KYOKA Enemy] {enemyName} defeated!");

            // Reward player
            var eco = KyokaEconomyManager.Instance;
            if (eco != null)
            {
                eco.AddCredits(rewardCredits, "EnemyDefeat_" + enemyName);
                eco.AddShards(rewardShards);
            }

            if (KyokaAudioManager.Instance != null)
            {
                KyokaAudioManager.Instance.PlaySFX(SoundEffectType.RewardChime, transform.position);
            }

            // Collapse & despawn visual
            transform.localScale = new Vector3(transform.localScale.x, 0.2f, transform.localScale.z);
            Destroy(gameObject, 2.5f);
        }
    }
}

