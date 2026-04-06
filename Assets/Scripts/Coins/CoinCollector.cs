using UnityEngine;
using SkibidiBrainrotFruit.Core;

namespace SkibidiBrainrotFruit.Coins
{
    public class CoinCollector : MonoBehaviour
    {
        private int _coinsCollected;

        public int CoinsCollected => _coinsCollected;

        private void OnEnable()
        {
            GameEvents.OnGameStart += ResetCoins;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStart -= ResetCoins;
        }

        private void ResetCoins()
        {
            _coinsCollected = 0;
            GameEvents.TriggerCoinCollected(0);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Coin>(out _))
            {
                _coinsCollected++;
                GameEvents.TriggerCoinCollected(_coinsCollected);
            }
        }
    }
}
