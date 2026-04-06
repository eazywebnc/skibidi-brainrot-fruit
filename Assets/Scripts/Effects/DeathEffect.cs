using UnityEngine;
using SkibidiBrainrotFruit.Core;

namespace SkibidiBrainrotFruit.Effects
{
    public class DeathEffect : MonoBehaviour
    {
        [SerializeField] private Transform _fruitModel;
        [SerializeField] private GameObject _explosionParticlePrefab;
        [SerializeField] private float _explodeForce = 10f;

        private Vector3 _originalScale;
        private bool _isDead;

        private void OnEnable()
        {
            GameEvents.OnGameOver += PlayDeathEffect;
            GameEvents.OnGameStart += ResetEffect;
        }

        private void OnDisable()
        {
            GameEvents.OnGameOver -= PlayDeathEffect;
            GameEvents.OnGameStart -= ResetEffect;
        }

        private void Start()
        {
            if (_fruitModel != null)
                _originalScale = _fruitModel.localScale;
        }

        private void PlayDeathEffect()
        {
            _isDead = true;

            if (_explosionParticlePrefab != null)
            {
                Instantiate(_explosionParticlePrefab, transform.position, Quaternion.identity);
            }

            if (_fruitModel != null)
            {
                _fruitModel.localScale = _originalScale * 1.5f;
            }
        }

        private void ResetEffect()
        {
            _isDead = false;
            if (_fruitModel != null)
                _fruitModel.localScale = _originalScale;
        }

        private void Update()
        {
            if (!_isDead || _fruitModel == null) return;

            _fruitModel.localScale = Vector3.Lerp(
                _fruitModel.localScale,
                Vector3.zero,
                Time.deltaTime * 3f
            );
        }
    }
}
