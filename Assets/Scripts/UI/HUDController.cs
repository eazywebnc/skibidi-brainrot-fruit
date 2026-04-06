using UnityEngine;
using TMPro;
using SkibidiBrainrotFruit.Core;
using SkibidiBrainrotFruit.GameManagement;

namespace SkibidiBrainrotFruit.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _coinText;
        [SerializeField] private GameObject _pausePanel;

        private void OnEnable()
        {
            GameEvents.OnScoreUpdated += UpdateScore;
            GameEvents.OnCoinCollected += UpdateCoins;
            GameEvents.OnGameStart += Show;
            GameEvents.OnGameOver += Hide;
        }

        private void OnDisable()
        {
            GameEvents.OnScoreUpdated -= UpdateScore;
            GameEvents.OnCoinCollected -= UpdateCoins;
            GameEvents.OnGameStart -= Show;
            GameEvents.OnGameOver -= Hide;
        }

        private void Show()
        {
            gameObject.SetActive(true);
            if (_pausePanel != null) _pausePanel.SetActive(false);
            UpdateScore(0f);
            UpdateCoins(0);
        }

        private void Hide()
        {
            // HUD stays visible at game over, GameOverUI overlays it
        }

        private void UpdateScore(float score)
        {
            if (_scoreText != null)
                _scoreText.text = $"{Mathf.FloorToInt(score)}m";
        }

        private void UpdateCoins(int coins)
        {
            if (_coinText != null)
                _coinText.text = coins.ToString();
        }

        public void OnPauseButtonClicked()
        {
            if (GameManager.Instance == null) return;

            if (GameManager.Instance.CurrentState == GameState.Playing)
            {
                GameManager.Instance.PauseGame();
                if (_pausePanel != null) _pausePanel.SetActive(true);
            }
        }

        public void OnResumeButtonClicked()
        {
            if (GameManager.Instance == null) return;

            GameManager.Instance.ResumeGame();
            if (_pausePanel != null) _pausePanel.SetActive(false);
        }
    }
}
