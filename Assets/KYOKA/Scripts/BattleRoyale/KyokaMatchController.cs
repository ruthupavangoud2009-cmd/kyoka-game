using UnityEngine;
using System;

namespace Kyoka.BattleRoyale
{
    public enum KyokaMatchState { Waiting, Starting, InProgress, Finished }

    public class KyokaMatchController : MonoBehaviour
    {
        public KyokaMatchState State { get; private set; } = KyokaMatchState.Waiting;
        public int matchDurationSeconds = 900;
        public int simulatedPlayerCount = 1;
        public float RemainingSeconds { get; private set; }
        public event Action<KyokaMatchState> StateChanged;

        public void StartLocalPrototypeMatch()
        {
            RemainingSeconds = matchDurationSeconds;
            SetState(KyokaMatchState.InProgress);
        }

        private void Update()
        {
            if (State != KyokaMatchState.InProgress) return;
            RemainingSeconds -= Time.deltaTime;
            if (RemainingSeconds <= 0) SetState(KyokaMatchState.Finished);
        }

        private void SetState(KyokaMatchState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }
    }
}
