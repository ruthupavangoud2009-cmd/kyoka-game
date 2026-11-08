using System;
using UnityEngine;

namespace Kyoka.Missions
{
    public enum MissionType
    {
        Story,
        Side,
        Daily,
        WorldEvent
    }

    public enum ObjectiveType
    {
        ReachLocation,
        DefeatEnemies,
        CollectResources,
        PilotVehicle
    }

    [Serializable]
    public class KyokaMission
    {
        public string missionId;
        public string title;
        [TextArea] public string description;
        public MissionType missionType = MissionType.Story;
        public ObjectiveType objectiveType = ObjectiveType.ReachLocation;

        [Header("Requirements & Progress")]
        public int requiredProgress = 1;
        public int currentProgress;
        public bool isCompleted;

        [Header("Rewards")]
        public int rewardCredits = 100;
        public int rewardShards = 5;
        public int rewardXP = 150;

        public bool IsComplete => isCompleted || currentProgress >= requiredProgress;

        public void AddProgress(int amount = 1)
        {
            if (isCompleted) return;
            currentProgress = Mathf.Clamp(currentProgress + amount, 0, requiredProgress);
            if (currentProgress >= requiredProgress)
            {
                isCompleted = true;
            }
        }
    }
}
