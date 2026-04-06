using System.Collections.Generic;
using UnityEngine;
using SkibidiBrainrotFruit.Core;

namespace SkibidiBrainrotFruit.Track
{
    public class TrackGenerator : MonoBehaviour
    {
        [SerializeField] private GameConfig _config;
        [SerializeField] private GameObject[] _chunkPrefabs;
        [SerializeField] private Transform _playerTransform;

        private readonly List<TrackChunk> _activeChunks = new List<TrackChunk>();
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
            ClearChunks();
            _nextSpawnZ = -_config.ChunkLength;
            _isActive = true;

            for (int i = 0; i < _config.ChunksAhead + _config.ChunksBehind; i++)
            {
                SpawnChunk();
            }
        }

        private void OnGameOver()
        {
            _isActive = false;
        }

        private void Update()
        {
            if (!_isActive || _playerTransform == null) return;

            float playerZ = _playerTransform.position.z;

            while (_nextSpawnZ - playerZ < _config.ChunksAhead * _config.ChunkLength)
            {
                SpawnChunk();
            }

            while (_activeChunks.Count > 0 &&
                   _activeChunks[0].EndZ < playerZ - _config.ChunksBehind * _config.ChunkLength)
            {
                RemoveOldestChunk();
            }
        }

        private void SpawnChunk()
        {
            if (_chunkPrefabs == null || _chunkPrefabs.Length == 0) return;

            GameObject prefab = _chunkPrefabs[Random.Range(0, _chunkPrefabs.Length)];
            GameObject chunkObj = Instantiate(prefab, new Vector3(0f, 0f, _nextSpawnZ), Quaternion.identity, transform);
            TrackChunk chunk = chunkObj.GetComponent<TrackChunk>();

            if (chunk == null)
            {
                chunk = chunkObj.AddComponent<TrackChunk>();
            }

            _activeChunks.Add(chunk);
            _nextSpawnZ += _config.ChunkLength;
        }

        private void RemoveOldestChunk()
        {
            if (_activeChunks.Count == 0) return;
            Destroy(_activeChunks[0].gameObject);
            _activeChunks.RemoveAt(0);
        }

        private void ClearChunks()
        {
            foreach (var chunk in _activeChunks)
            {
                if (chunk != null)
                    Destroy(chunk.gameObject);
            }
            _activeChunks.Clear();
            _nextSpawnZ = 0f;
        }
    }
}
