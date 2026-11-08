using UnityEngine;
using System;

namespace Kyoka.World
{
    public class KyokaWantedSystem : MonoBehaviour
    {
        public static KyokaWantedSystem Instance { get; private set; }

        [Range(0, 5)]
        [SerializeField] private int currentWantedStars = 0;
        public int CurrentWantedStars => currentWantedStars;

        public float decayCooldown = 15f;
        private float lastCrimeTimestamp = -100f;

        public event Action<int> OnWantedLevelChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Update()
        {
            if (currentWantedStars > 0 && Time.time - lastCrimeTimestamp > decayCooldown)
            {
                currentWantedStars--;
                lastCrimeTimestamp = Time.time;
                OnWantedLevelChanged?.Invoke(currentWantedStars);
                Debug.Log($"[KYOKA Wanted] Evaded authorities! Stars reduced to: {currentWantedStars}");
            }
        }

        public void ReportCrime(int severity = 1)
        {
            currentWantedStars = Mathf.Clamp(currentWantedStars + severity, 0, 5);
            lastCrimeTimestamp = Time.time;
            OnWantedLevelChanged?.Invoke(currentWantedStars);
            Debug.Log($"[KYOKA Wanted] Crime reported! Stars increased to: {currentWantedStars}");
        }

        public void ClearWanted()
        {
            currentWantedStars = 0;
            OnWantedLevelChanged?.Invoke(0);
        }
    }
}
