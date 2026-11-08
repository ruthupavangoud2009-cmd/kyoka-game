using UnityEngine;
using System;
using Kyoka.World;

namespace Kyoka.Core
{
    public enum KyokaGameMode
    {
        OpenWorld = 0,
        BattleRoyale = 1,
        AnimeRPG = 2,
        Survival = 3,
        Creative = 4,
        Roleplay = 5
    }

    /// <summary>
    /// Coordinates active game rules, spawn locations, mode-specific UI overlays,
    /// and mode switching for all 6 KYOKA game modes.
    /// </summary>
    public class KyokaGameModeManager : MonoBehaviour
    {
        public static KyokaGameModeManager Instance { get; private set; }

        [SerializeField] private KyokaGameMode activeMode = KyokaGameMode.OpenWorld;
        public KyokaGameMode ActiveMode => activeMode;

        public event Action<KyokaGameMode> OnGameModeChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetGameMode(KyokaGameMode mode)
        {
            activeMode = mode;
            Debug.Log($"[KYOKA] Game Mode set to: {activeMode}");
            ApplyModeRules(mode);
            OnGameModeChanged?.Invoke(activeMode);
        }

        private void ApplyModeRules(KyokaGameMode mode)
        {
            switch (mode)
            {
                case KyokaGameMode.OpenWorld:
                    // Full access to City, Wildlands, Ocean, Anime Realm
                    break;
                case KyokaGameMode.BattleRoyale:
                    // Enable Battle Royale match controller and safe zone
                    var match = FindObjectOfType<Kyoka.BattleRoyale.KyokaMatchController>();
                    if (match != null) match.StartLocalPrototypeMatch();
                    break;
                case KyokaGameMode.AnimeRPG:
                    // Boost elemental energy recharge, activate boss spawner
                    break;
                case KyokaGameMode.Survival:
                    // Activate resource harvesting and survival craft loops
                    break;
                case KyokaGameMode.Creative:
                    // Enable infinite building and placement
                    break;
                case KyokaGameMode.Roleplay:
                    // Enable city jobs and roleplay interactions
                    break;
            }
        }
    }
}
