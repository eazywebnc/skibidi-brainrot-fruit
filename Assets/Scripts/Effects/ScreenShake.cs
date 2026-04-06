using UnityEngine;
using SkibidiBrainrotFruit.Core;

namespace SkibidiBrainrotFruit.Effects
{
    public class ScreenShake : MonoBehaviour
    {
        [SerializeField] private GameConfig _config;

        private float _shakeTimer;
        private Vector3 _originalPosition;
        private bool _isShaking;

        private void OnEnable()
        {
            GameEvents.OnNearMiss += TriggerShake;
            GameEvents.OnGameOver += TriggerDeathShake;
        }

        private void OnDisable()
        {
            GameEvents.OnNearMiss -= TriggerShake;
            GameEvents.OnGameOver -= TriggerDeathShake;
        }

        private void TriggerShake()
        {
            Shake(_config.ScreenShakeIntensity, _config.ScreenShakeDuration);
        }

        private void TriggerDeathShake()
        {
            Shake(_config.ScreenShakeIntensity * 3f, _config.ScreenShakeDuration * 2f);
        }

        public void Shake(float intensity, float duration)
        {
            if (!_isShaking)
            {
                _originalPosition = transform.localPosition;
            }
            _shakeTimer = duration;
            _isShaking = true;
        }

        private void Update()
        {
            if (!_isShaking) return;

            _shakeTimer -= Time.unscaledDeltaTime;

            if (_shakeTimer <= 0f)
            {
                _isShaking = false;
                transform.localPosition = _originalPosition;
                return;
            }

            float intensity = _config.ScreenShakeIntensity * (_shakeTimer / _config.ScreenShakeDuration);
            transform.localPosition = _originalPosition + Random.insideUnitSphere * intensity;
        }
    }
}
