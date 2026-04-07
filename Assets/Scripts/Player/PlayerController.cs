using UnityEngine;
using SkibidiBrainrotFruit.Core;
using SkibidiBrainrotFruit.GameManagement;

namespace SkibidiBrainrotFruit.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private GameConfig _config;
        [SerializeField] private Transform _fruitModel;

        private CharacterController _controller;
        private int _currentLane; // -1 left, 0 center, 1 right
        private float _targetXPosition;
        private float _verticalVelocity;
        private float _currentSpeed;
        private bool _isSliding;
        private float _slideTimer;
        private float _originalControllerHeight;
        private float _originalControllerCenterY;
        private bool _isAlive;

        public float CurrentSpeed => _currentSpeed;
        public float DistanceTraveled { get; private set; }
        public bool IsGrounded => _controller.isGrounded;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _originalControllerHeight = _controller.height;
            _originalControllerCenterY = _controller.center.y;
        }

        private void OnEnable()
        {
            GameEvents.OnGameStart += OnGameStart;
            GameEvents.OnGameOver += OnGameOver;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStart -= OnGameStart;
            GameEvents.OnGameOver -= OnGameOver;
        }

        private void OnGameStart()
        {
            _isAlive = true;
            _currentSpeed = _config.InitialForwardSpeed;
            _currentLane = 0;
            _targetXPosition = 0f;
            _verticalVelocity = 0f;
            DistanceTraveled = 0f;
            _isSliding = false;
            ResetColliderSize();
        }

        private void OnGameOver()
        {
            _isAlive = false;
        }

        private void Update()
        {
            if (!_isAlive) return;

            _currentSpeed = Mathf.Min(
                _currentSpeed + _config.SpeedIncreaseRate * Time.deltaTime,
                _config.MaxForwardSpeed
            );

            DistanceTraveled += _currentSpeed * Time.deltaTime;

            HandleLaneMovement();
            HandleVerticalMovement();
            HandleSlide();
            HandleFruitRotation();

            Vector3 moveDir = new Vector3(0f, 0f, _currentSpeed);
            float xMove = (_targetXPosition - transform.position.x) * _config.LaneSwitchSpeed;
            moveDir.x = xMove;
            moveDir.y = _verticalVelocity;

            _controller.Move(moveDir * Time.deltaTime);
        }

        public void SwitchLane(int direction)
        {
            if (!_isAlive) return;

            int targetLane = _currentLane + direction;
            if (targetLane < -1 || targetLane > 1) return;

            _currentLane = targetLane;
            _targetXPosition = _currentLane * _config.LaneDistance;
        }

        public void Jump()
        {
            if (!_isAlive || !_controller.isGrounded || _isSliding) return;
            _verticalVelocity = _config.JumpForce;
        }

        public void Slide()
        {
            if (!_isAlive || _isSliding) return;

            _isSliding = true;
            _slideTimer = _config.SlideDuration;

            _controller.height = _originalControllerHeight * 0.5f;
            _controller.center = new Vector3(
                _controller.center.x,
                _originalControllerCenterY * 0.5f,
                _controller.center.z
            );

            if (!_controller.isGrounded)
            {
                _verticalVelocity = _config.Gravity * 0.5f;
            }
        }

        private void HandleLaneMovement()
        {
            _targetXPosition = _currentLane * _config.LaneDistance;
        }

        private void HandleVerticalMovement()
        {
            if (_controller.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -1f;
            }
            else
            {
                _verticalVelocity += _config.Gravity * Time.deltaTime;
            }
        }

        private void HandleSlide()
        {
            if (!_isSliding) return;

            _slideTimer -= Time.deltaTime;
            if (_slideTimer <= 0f)
            {
                _isSliding = false;
                ResetColliderSize();
            }
        }

        private void HandleFruitRotation()
        {
            if (_fruitModel == null) return;
            _fruitModel.Rotate(
                Vector3.right,
                _config.FruitRotationSpeed * (_currentSpeed / _config.InitialForwardSpeed) * Time.deltaTime,
                Space.World
            );
        }

        private void ResetColliderSize()
        {
            _controller.height = _originalControllerHeight;
            _controller.center = new Vector3(
                _controller.center.x,
                _originalControllerCenterY,
                _controller.center.z
            );
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!_isAlive) return;

            if (hit.gameObject.CompareTag("Obstacle"))
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.GameOver();
                else
                    GameEvents.TriggerGameOver();
            }
        }
    }
}
