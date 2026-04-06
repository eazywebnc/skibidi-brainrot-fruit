using UnityEngine;

namespace SkibidiBrainrotFruit.Obstacles
{
    public class Obstacle : MonoBehaviour
    {
        [SerializeField] private ObstacleType _type;
        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _moveRange = 3f;

        public ObstacleType Type => _type;

        private float _startX;
        private int _moveDirection = 1;

        private void Start()
        {
            _startX = transform.position.x;
        }

        private void Update()
        {
            if (_type != ObstacleType.MovingBlock) return;

            Vector3 pos = transform.position;
            pos.x += _moveSpeed * _moveDirection * Time.deltaTime;

            if (Mathf.Abs(pos.x - _startX) >= _moveRange)
            {
                _moveDirection *= -1;
            }

            transform.position = pos;
        }
    }
}
