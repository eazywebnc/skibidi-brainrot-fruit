using UnityEngine;
using SkibidiBrainrotFruit.GameManagement;

namespace SkibidiBrainrotFruit.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject _settingsPanel;

        private void Start()
        {
            if (_settingsPanel != null) _settingsPanel.SetActive(false);
        }

        public void OnPlayClicked()
        {
            GameManager.Instance?.LoadGameScene();
        }

        public void OnSettingsClicked()
        {
            if (_settingsPanel != null)
                _settingsPanel.SetActive(true);
        }

        public void OnSettingsCloseClicked()
        {
            if (_settingsPanel != null)
                _settingsPanel.SetActive(false);
        }

        public void OnSoundToggle(bool isOn)
        {
            AudioListener.volume = isOn ? 1f : 0f;
            PlayerPrefs.SetInt("SoundEnabled", isOn ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
