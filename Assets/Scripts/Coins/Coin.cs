using UnityEngine;
using SkibidiBrainrotFruit.Core;

namespace SkibidiBrainrotFruit.Coins
{
    [RequireComponent(typeof(Collider))]
    public class Coin : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeed = 180f;
        [SerializeField] private float _bobSpeed = 2f;
        [SerializeField] private float _bobHeight = 0.3f;
        [SerializeField] private GameObject _collectEffect;

        private Vector3 _startPosition;
        private bool _collected;

        private void Start()
        {
            _startPosition = transform.position;
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);

            Vector3 pos = _startPosition;
            pos.y += Mathf.Sin(Time.time * _bobSpeed) * _bobHeight;
            transform.position = pos;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_collected) return;
            if (!other.CompareTag("Player")) return;

            _collected = true;

            if (_collectEffect != null)
            {
                Instantiate(_collectEffect, transform.position, Quaternion.identity);
            }

            // Disable collider immediately to prevent double-collection
            GetComponent<Collider>().enabled = false;
            Destroy(gameObject);
        }
    }
}
