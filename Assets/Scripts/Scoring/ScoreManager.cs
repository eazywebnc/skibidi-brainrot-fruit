using UnityEngine;
using SkibidiBrainrotFruit.Core;
using SkibidiBrainrotFruit.Player;

namespace SkibidiBrainrotFruit.Scoring
{
    public class ScoreManager : MonoBehaviour
    {
        private const string HighScoreKey = "HighScore";

        [SerializeField] private PlayerController _player;

        private float _currentScore;
        private int _highScore;
        private bool _isActive;

        public float CurrentScore => _currentScore;
        public int HighScore => _highScore;

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
            _currentScore = 0f;
            _highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
            _isActive = true;
        }

        private void OnGameOver()
        {
            _isActive = false;

            int finalScore = Mathf.FloorToInt(_currentScore);
            if (finalScore > _highScore)
            {
                _highScore = finalScore;
                PlayerPrefs.SetInt(HighScoreKey, _highScore);
                PlayerPrefs.Save();
            }
        }

        private void Update()
        {
            if (!_isActive || _player == null) return;

            _currentScore = _player.DistanceTraveled;
            GameEvents.TriggerScoreUpdated(_currentScore);
        }
    }
}
