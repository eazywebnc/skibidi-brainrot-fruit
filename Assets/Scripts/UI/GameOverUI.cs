using UnityEngine;
using TMPro;
using SkibidiBrainrotFruit.Core;
using SkibidiBrainrotFruit.GameManagement;
using SkibidiBrainrotFruit.Scoring;

namespace SkibidiBrainrotFruit.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _highScoreText;
        [SerializeField] private TextMeshProUGUI _coinsText;
        [SerializeField] private ScoreManager _scoreManager;

        private int _lastCoinCount;

        private void OnEnable()
        {
            GameEvents.OnGameOver += ShowGameOver;
            GameEvents.OnGameStart += HideGameOver;
            GameEvents.OnCoinCollected += OnCoinUpdate;
        }

        private void OnDisable()
        {
            GameEvents.OnGameOver -= ShowGameOver;
            GameEvents.OnGameStart -= HideGameOver;
            GameEvents.OnCoinCollected -= OnCoinUpdate;
        }

        private void Start()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        private void OnCoinUpdate(int coins)
        {
            _lastCoinCount = coins;
        }

        private void ShowGameOver()
        {
            if (_panel != null) _panel.SetActive(true);

            if (_scoreManager != null)
            {
                if (_scoreText != null)
                    _scoreText.text = $"{Mathf.FloorToInt(_scoreManager.CurrentScore)}m";
                if (_highScoreText != null)
                    _highScoreText.text = $"Best: {_scoreManager.HighScore}m";
            }

            if (_coinsText != null)
                _coinsText.text = $"{_lastCoinCount} coins";
        }

        private void HideGameOver()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        public void OnRetryClicked()
        {
            GameManager.Instance?.RetryGame();
        }

        public void OnMenuClicked()
        {
            GameManager.Instance?.LoadMainMenu();
        }
    }
}
