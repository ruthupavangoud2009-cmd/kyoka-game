using System;
using UnityEngine;

namespace Kyoka.Missions
{
    [Serializable]
    public class KyokaMission
    {
        public string missionId;
        public string title;
        [TextArea] public string description;
        public int requiredProgress = 1;
        public int rewardCurrency = 25;
        [NonSerialized] public int currentProgress;
        public bool IsComplete => currentProgress >= requiredProgress;
        public void AddProgress(int amount = 1) => currentProgress = Mathf.Clamp(currentProgress + amount, 0, requiredProgress);
    }
}
