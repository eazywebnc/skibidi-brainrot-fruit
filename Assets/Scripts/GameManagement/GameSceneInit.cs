using UnityEngine;
using SkibidiBrainrotFruit.GameManagement;

namespace SkibidiBrainrotFruit.GameManagement
{
    public class GameSceneInit : MonoBehaviour
    {
        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
        }
    }
}
