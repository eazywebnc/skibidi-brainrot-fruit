using UnityEngine;
using SkibidiBrainrotFruit.Core;

namespace SkibidiBrainrotFruit.Obstacles
{
    public class ObstacleSpawner : MonoBehaviour
    {
        [SerializeField] private GameConfig _config;
        [SerializeField] private Transform _playerTransform;

        [Header("Obstacle Prefabs")]
        [SerializeField] private GameObject _lowWallPrefab;
        [SerializeField] private GameObject _highWallPrefab;
        [SerializeField] private GameObject _laneBlockPrefab;
        [SerializeField] private GameObject _movingBlockPrefab;

        private float _nextSpawnZ;
        private float _distanceTraveled;
        private bool _isActive;

        private void OnEnable()
        {
            GameEvents.OnGameStart += OnGameStart;
            GameEvents.OnGameOver += OnGameOver;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStart -= OnGameStart;
            GameEvents.OnGameOver -= OnGameOver;
        }

        private void OnGameStart()
        {
            ClearObstacles();
            _nextSpawnZ = 40f;
            _distanceTraveled = 0f;
            _isActive = true;
        }

        private void OnGameOver()
        {
            _isActive = false;
        }

        private void Update()
        {
            if (!_isActive || _playerTransform == null) return;

            float playerZ = _playerTransform.position.z;
            _distanceTraveled = playerZ;

            while (_nextSpawnZ < playerZ + 100f)
            {
                SpawnObstacle(_nextSpawnZ);
                float difficulty = Mathf.Min(
                    1f + _distanceTraveled * _config.DifficultyIncreaseRate,
                    _config.MaxDifficultyMultiplier
                );
                float spacing = Mathf.Lerp(
                    _config.MaxObstacleSpacing,
                    _config.MinObstacleSpacing,
                    (difficulty - 1f) / (_config.MaxDifficultyMultiplier - 1f)
                );
                _nextSpawnZ += spacing;
            }
        }

        private void SpawnObstacle(float zPos)
        {
            int lane = Random.Range(-1, 2);
            float xPos = lane * _config.LaneDistance;

            ObstacleType type = (ObstacleType)Random.Range(0, 4);
            GameObject prefab = type switch
            {
                ObstacleType.LowWall => _lowWallPrefab,
                ObstacleType.HighWall => _highWallPrefab,
                ObstacleType.LaneBlock => _laneBlockPrefab,
                ObstacleType.MovingBlock => _movingBlockPrefab,
                _ => _laneBlockPrefab
            };

            if (prefab == null) return;

            Vector3 position = new Vector3(xPos, 0f, zPos);
            Instantiate(prefab, position, Quaternion.identity, transform);
        }

        private void ClearObstacles()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}
