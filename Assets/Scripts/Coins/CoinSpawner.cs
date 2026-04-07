using UnityEngine;
using SkibidiBrainrotFruit.Core;

namespace SkibidiBrainrotFruit.Coins
{
    public enum CoinPattern
    {
        StraightLine,
        Arc,
        Zigzag
    }

    public class CoinSpawner : MonoBehaviour
    {
        [SerializeField] private GameConfig _config;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private GameObject _coinPrefab;

        private float _nextSpawnZ;
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
            ClearCoins();
            _nextSpawnZ = 20f;
            _isActive = true;
        }

        private void OnGameOver()
        {
            _isActive = false;
        }

        private void Update()
        {
            if (!_isActive || _playerTransform == null || _coinPrefab == null) return;

            float playerZ = _playerTransform.position.z;

            while (_nextSpawnZ < playerZ + 80f)
            {
                CoinPattern pattern = (CoinPattern)Random.Range(0, 3);
                int count = Random.Range(_config.MinCoinsPerPattern, _config.MaxCoinsPerPattern + 1);
                int lane = Random.Range(-1, 2);

                SpawnPattern(pattern, _nextSpawnZ, lane, count);
                _nextSpawnZ += count * _config.CoinSpacing + 10f;
            }
        }

        private void SpawnPattern(CoinPattern pattern, float startZ, int lane, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float z = startZ + i * _config.CoinSpacing;
                float x;
                float y = 1.5f;

                switch (pattern)
                {
                    case CoinPattern.StraightLine:
                        x = lane * _config.LaneDistance;
                        break;
                    case CoinPattern.Arc:
                        x = lane * _config.LaneDistance;
                        float t = count > 1 ? (float)i / (count - 1) : 0f;
                        y = 1.5f + Mathf.Sin(t * Mathf.PI) * 2f;
                        break;
                    case CoinPattern.Zigzag:
                        int zigLane = (i % 2 == 0) ? lane : Mathf.Clamp(lane + 1, -1, 1);
                        x = zigLane * _config.LaneDistance;
                        break;
                    default:
                        x = lane * _config.LaneDistance;
                        break;
                }

                Vector3 pos = new Vector3(x, y, z);
                Instantiate(_coinPrefab, pos, Quaternion.identity, transform);
            }
        }

        private void ClearCoins()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}
