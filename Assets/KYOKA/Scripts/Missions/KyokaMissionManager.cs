using UnityEngine;
using System.Collections.Generic;

namespace Kyoka.Missions
{
    public class KyokaMissionManager : MonoBehaviour
    {
        public List<KyokaMission> missions = new List<KyokaMission>();

        private void Start()
        {
            if (missions.Count == 0)
            {
                missions.Add(new KyokaMission {
                    missionId = "explore_spawn", title = "Explore the Training Island",
                    description = "Visit the prototype landmark.", requiredProgress = 1, rewardCurrency = 25
                });
            }
        }

        public void AddProgress(string missionId, int amount = 1)
        {
            var mission = missions.Find(m => m.missionId == missionId);
            if (mission != null) mission.AddProgress(amount);
        }
    }
}
