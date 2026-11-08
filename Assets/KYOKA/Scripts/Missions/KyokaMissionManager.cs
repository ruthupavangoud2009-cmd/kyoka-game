using UnityEngine;
using System;
using System.Collections.Generic;
using Kyoka.Economy;
using Kyoka.Save;

namespace Kyoka.Missions
{
    public class KyokaMissionManager : MonoBehaviour
    {
        public static KyokaMissionManager Instance { get; private set; }

        public List<KyokaMission> missions = new List<KyokaMission>();
        [SerializeField] private string activeMissionId = "m_story_01";

        public KyokaMission ActiveMission => missions.Find(m => m.missionId == activeMissionId);

        public event Action<KyokaMission> OnMissionUpdated;
        public event Action<KyokaMission> OnMissionCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            InitializeDefaultMissions();
        }

        private void InitializeDefaultMissions()
        {
            if (missions.Count == 0)
            {
                missions.Add(new KyokaMission
                {
                    missionId = "m_story_01",
                    title = "Awakening in Kyoka",
                    description = "Awaken your latent abilities at the central training grounds.",
                    missionType = MissionType.Story,
                    objectiveType = ObjectiveType.ReachLocation,
                    requiredProgress = 1,
                    rewardCredits = 150,
                    rewardShards = 5,
                    rewardXP = 100
                });

                missions.Add(new KyokaMission
                {
                    missionId = "m_story_02",
                    title = "The Spires of Aether",
                    description = "Investigate the high-resonance anime spires in the eastern district.",
                    missionType = MissionType.Story,
                    objectiveType = ObjectiveType.ReachLocation,
                    requiredProgress = 1,
                    rewardCredits = 300,
                    rewardShards = 10,
                    rewardXP = 250
                });

                missions.Add(new KyokaMission
                {
                    missionId = "m_side_01",
                    title = "Speed Trial",
                    description = "Pilot any motor vehicle across the district bridge.",
                    missionType = MissionType.Side,
                    objectiveType = ObjectiveType.PilotVehicle,
                    requiredProgress = 1,
                    rewardCredits = 100,
                    rewardShards = 2,
                    rewardXP = 75
                });

                missions.Add(new KyokaMission
                {
                    missionId = "m_daily_01",
                    title = "Resource Gathering Drill",
                    description = "Gather 5 timber or stone nodes for local defenses.",
                    missionType = MissionType.Daily,
                    objectiveType = ObjectiveType.CollectResources,
                    requiredProgress = 5,
                    rewardCredits = 200,
                    rewardShards = 4,
                    rewardXP = 120
                });
            }
        }

        public void AddProgress(string missionId, int amount = 1)
        {
            var mission = missions.Find(m => m.missionId == missionId);
            if (mission != null && !mission.isCompleted)
            {
                mission.AddProgress(amount);
                OnMissionUpdated?.Invoke(mission);

                if (mission.IsComplete)
                {
                    ClaimRewards(mission);
                    OnMissionCompleted?.Invoke(mission);
                }
            }
        }

        private void ClaimRewards(KyokaMission mission)
        {
            var eco = KyokaEconomyManager.Instance;
            if (eco != null)
            {
                eco.AddCredits(mission.rewardCredits, "MissionReward_" + mission.missionId);
                eco.AddShards(mission.rewardShards);
            }
            Debug.Log($"[KYOKA Missions] Completed: {mission.title}! Awarded {mission.rewardCredits} Credits & {mission.rewardShards} Shards.");
        }

        public void SetActiveMission(string missionId)
        {
            activeMissionId = missionId;
            OnMissionUpdated?.Invoke(ActiveMission);
        }

        public void SaveTo(KyokaSaveData save)
        {
            if (save == null) return;
            save.missions.Clear();
            foreach (var m in missions)
            {
                save.missions.Add(new SavedMissionProgress
                {
                    missionId = m.missionId,
                    currentProgress = m.currentProgress,
                    isCompleted = m.isCompleted
                });
            }
        }

        public void LoadFrom(KyokaSaveData save)
        {
            if (save == null || save.missions == null) return;
            foreach (var savedM in save.missions)
            {
                var existing = missions.Find(x => x.missionId == savedM.missionId);
                if (existing != null)
                {
                    existing.currentProgress = savedM.currentProgress;
                    existing.isCompleted = savedM.isCompleted;
                }
            }
        }
    }
}
