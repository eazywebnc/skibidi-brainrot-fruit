using UnityEngine;
using UnityEngine.InputSystem;
using SkibidiBrainrotFruit.Core;
using SkibidiBrainrotFruit.Player;

namespace SkibidiBrainrotFruit.Input
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] private PlayerController _player;
        [SerializeField] private GameConfig _config;

        private Vector2 _touchStartPosition;
        private bool _isTouching;

        private void Update()
        {
            HandleKeyboardInput();
            HandleTouchInput();
        }

        private void HandleKeyboardInput()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
                _player.SwitchLane(-1);
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
                _player.SwitchLane(1);
            if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
                _player.Jump();
            if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
                _player.Slide();
        }

        private void HandleTouchInput()
        {
            if (Touchscreen.current == null) return;

            var touch = Touchscreen.current.primaryTouch;

            if (touch.press.wasPressedThisFrame)
            {
                _touchStartPosition = touch.position.ReadValue();
                _isTouching = true;
            }
            else if (touch.press.wasReleasedThisFrame && _isTouching)
            {
                _isTouching = false;
                Vector2 delta = touch.position.ReadValue() - _touchStartPosition;

                if (delta.magnitude < _config.SwipeThreshold) return;

                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                {
                    _player.SwitchLane(delta.x > 0 ? 1 : -1);
                }
                else
                {
                    if (delta.y > 0)
                        _player.Jump();
                    else
                        _player.Slide();
                }
            }
        }
    }
}
