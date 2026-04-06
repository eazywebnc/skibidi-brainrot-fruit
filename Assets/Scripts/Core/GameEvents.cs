using System;

namespace SkibidiBrainrotFruit.Core
{
    public static class GameEvents
    {
        public static event Action OnGameStart;
        public static event Action OnGameOver;
        public static event Action OnGamePause;
        public static event Action OnGameResume;
        public static event Action<int> OnCoinCollected;
        public static event Action<float> OnScoreUpdated;
        public static event Action OnNearMiss;

        public static void TriggerGameStart() => OnGameStart?.Invoke();
        public static void TriggerGameOver() => OnGameOver?.Invoke();
        public static void TriggerGamePause() => OnGamePause?.Invoke();
        public static void TriggerGameResume() => OnGameResume?.Invoke();
        public static void TriggerCoinCollected(int total) => OnCoinCollected?.Invoke(total);
        public static void TriggerScoreUpdated(float score) => OnScoreUpdated?.Invoke(score);
        public static void TriggerNearMiss() => OnNearMiss?.Invoke();
    }
}
