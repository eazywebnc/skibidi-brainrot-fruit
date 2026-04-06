using UnityEngine;
using UnityEngine.SceneManagement;
using SkibidiBrainrotFruit.Core;

namespace SkibidiBrainrotFruit.GameManagement
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameConfig _config;

        private GameState _currentState = GameState.Menu;

        public static GameManager Instance { get; private set; }
        public GameState CurrentState => _currentState;
        public GameConfig Config => _config;

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

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void StartGame()
        {
            _currentState = GameState.Playing;
            Time.timeScale = 1f;
            GameEvents.TriggerGameStart();
        }

        public void PauseGame()
        {
            if (_currentState != GameState.Playing) return;
            _currentState = GameState.Paused;
            Time.timeScale = 0f;
            GameEvents.TriggerGamePause();
        }

        public void ResumeGame()
        {
            if (_currentState != GameState.Paused) return;
            _currentState = GameState.Playing;
            Time.timeScale = 1f;
            GameEvents.TriggerGameResume();
        }

        public void GameOver()
        {
            _currentState = GameState.GameOver;
            GameEvents.TriggerGameOver();
        }

        public void RetryGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            _currentState = GameState.Menu;
            SceneManager.LoadScene("MainMenu");
        }

        public void LoadGameScene()
        {
            SceneManager.LoadScene("Game");
        }
    }
}
