using UnityEngine;
using SkibidiBrainrotFruit.Core;

namespace SkibidiBrainrotFruit.Effects
{
    public class ScreenShake : MonoBehaviour
    {
        [SerializeField] private GameConfig _config;

        private float _shakeTimer;
        private float _shakeDuration;
        private float _shakeIntensity;
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
            _shakeIntensity = intensity;
            _shakeDuration = duration;
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

            float currentIntensity = _shakeIntensity * (_shakeTimer / _shakeDuration);
            transform.localPosition = _originalPosition + Random.insideUnitSphere * currentIntensity;
        }
    }
}
