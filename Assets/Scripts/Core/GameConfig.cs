using UnityEngine;

namespace SkibidiBrainrotFruit.Core
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "SkibidiBrainrotFruit/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Player Movement")]
        public float InitialForwardSpeed = 10f;
        public float MaxForwardSpeed = 30f;
        public float SpeedIncreaseRate = 0.1f;
        public float LaneDistance = 3f;
        public float LaneSwitchSpeed = 10f;
        public float JumpForce = 10f;
        public float Gravity = -30f;
        public float SlideDuration = 0.6f;
        public float FruitRotationSpeed = 180f;

        [Header("Track")]
        public int ChunksAhead = 5;
        public int ChunksBehind = 2;
        public float ChunkLength = 30f;

        [Header("Obstacles")]
        public float MinObstacleSpacing = 15f;
        public float MaxObstacleSpacing = 30f;
        public float DifficultyIncreaseRate = 0.02f;
        public float MaxDifficultyMultiplier = 3f;

        [Header("Coins")]
        public float CoinSpacing = 2f;
        public int MinCoinsPerPattern = 3;
        public int MaxCoinsPerPattern = 8;

        [Header("Game Feel")]
        public float ScreenShakeIntensity = 0.15f;
        public float ScreenShakeDuration = 0.2f;
        public float NearMissDistance = 1.5f;

        [Header("Input")]
        public float SwipeThreshold = 50f;
    }
}
