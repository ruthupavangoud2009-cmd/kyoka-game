using UnityEngine;
using System;
using Kyoka.Player;
using Kyoka.Economy;

namespace Kyoka.BattleRoyale
{
    public enum KyokaMatchState { Waiting, DropPhase, InProgress, Finished }

    public class KyokaMatchController : MonoBehaviour
    {
        public static KyokaMatchController Instance { get; private set; }

        [Header("Match Settings")]
        public KyokaMatchState State { get; private set; } = KyokaMatchState.Waiting;
        public int matchDurationSeconds = 600;
        public int initialPlayerCount = 50;
        public int alivePlayers = 50;
        public float RemainingSeconds { get; private set; }

        [Header("Safe Zone Mechanics")]
        public Vector3 zoneCenter = Vector3.zero;
        public float initialRadius = 150f;
        public float currentRadius = 150f;
        public float minRadius = 15f;
        public float shrinkRate = 0.8f;
        public int stormDamagePerSecond = 5;

        [Header("Match Outcome")]
        public int playerKills = 0;
        public int finalRank = 1;
        public bool isPlayerAlive = true;

        public event Action<KyokaMatchState> StateChanged;
        public event Action<int> OnAlivePlayersCountChanged;
        public event Action<float, Vector3> OnSafeZoneUpdated;

        private float stormDamageTimer = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void StartLocalPrototypeMatch()
        {
            RemainingSeconds = matchDurationSeconds;
            alivePlayers = initialPlayerCount;
            currentRadius = initialRadius;
            playerKills = 0;
            isPlayerAlive = true;
            SetState(KyokaMatchState.InProgress);
            Debug.Log("[KYOKA BattleRoyale] Match commenced! Jump from drop aircraft.");
        }

        private void Update()
        {
            if (State != KyokaMatchState.InProgress) return;

            RemainingSeconds -= Time.deltaTime;

            // Shrink safe zone
            if (currentRadius > minRadius)
            {
                currentRadius = Mathf.Max(minRadius, currentRadius - shrinkRate * Time.deltaTime);
                OnSafeZoneUpdated?.Invoke(currentRadius, zoneCenter);
            }

            // Storm damage check on local player
            stormDamageTimer += Time.deltaTime;
            if (stormDamageTimer >= 1.0f)
            {
                stormDamageTimer = 0f;
                CheckPlayerStormDamage();
                SimulateCompetitorEliminations();
            }

            if (RemainingSeconds <= 0 || alivePlayers <= 1)
            {
                EndMatch(alivePlayers == 1 ? 1 : UnityEngine.Random.Range(2, 5));
            }
        }

        private void CheckPlayerStormDamage()
        {
            var player = FindObjectOfType<KyokaPlayerController>();
            if (player == null) return;

            float distFromCenter = Vector3.Distance(new Vector3(player.transform.position.x, 0, player.transform.position.z),
                                                   new Vector3(zoneCenter.x, 0, zoneCenter.z));

            if (distFromCenter > currentRadius)
            {
                var health = player.GetComponent<KyokaHealth>();
                if (health)
                {
                    health.ApplyGameDamage(stormDamagePerSecond);
                    Debug.Log($"[KYOKA Storm] Player taking storm damage! Outside safe zone by {distFromCenter - currentRadius:F1}m");
                }
            }
        }

        private void SimulateCompetitorEliminations()
        {
            // In standalone/prototype mode, simulate bots getting eliminated over time
            if (alivePlayers > 2 && UnityEngine.Random.value < 0.2f)
            {
                alivePlayers--;
                OnAlivePlayersCountChanged?.Invoke(alivePlayers);
            }
        }

        public void RegisterPlayerKill()
        {
            playerKills++;
            alivePlayers = Mathf.Max(1, alivePlayers - 1);
            OnAlivePlayersCountChanged?.Invoke(alivePlayers);
            Debug.Log($"[KYOKA BattleRoyale] Enemy eliminated! Kills: {playerKills}, Remaining: {alivePlayers}");
        }

        public void EndMatch(int rank)
        {
            finalRank = rank;
            SetState(KyokaMatchState.Finished);

            // Grant rewards based on rank
            int rewardCredits = (finalRank == 1 ? 500 : (1000 / finalRank)) + (playerKills * 50);
            int rewardShards = finalRank == 1 ? 15 : (finalRank <= 5 ? 5 : 1);

            var eco = KyokaEconomyManager.Instance;
            if (eco != null)
            {
                eco.AddCredits(rewardCredits, "BattleRoyale_MatchReward");
                eco.AddShards(rewardShards);
            }

            Debug.Log($"[KYOKA BattleRoyale] Match Finished! Rank #{finalRank} | Kills: {playerKills} | Reward: {rewardCredits} Credits");
        }

        private void SetState(KyokaMatchState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }
    }
}
