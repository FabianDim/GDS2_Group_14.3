using UnityEngine;
using UnityEngine.InputSystem;

namespace _Experimenation.Fraser.Scripts
{
    public class LocalMovementInput : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool SprintHeld { get; private set; }
        public bool CrouchHeld { get; private set; }
        public bool UsingGamepadLook { get; private set; }

        private bool _jumpPressed;
        private bool _crouchPressed;

        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _jumpAction;
        private InputAction _sprintAction;
        private InputAction _crouchAction;

        private void Start()
        {
            _playerInput = GetComponent<PlayerInput>();

            _moveAction = _playerInput.actions["Move"];
            _lookAction = _playerInput.actions["Tools"];
            _jumpAction = _playerInput.actions["Jump"];
            _sprintAction = _playerInput.actions["Sprint"];
            _crouchAction = _playerInput.actions["Crouch"];
        }

        private void Update()
        {
            MoveInput = _moveAction.ReadValue<Vector2>();
            LookInput = _lookAction.ReadValue<Vector2>();

            SprintHeld = _sprintAction.IsPressed();
            CrouchHeld = _crouchAction.IsPressed();

            UsingGamepadLook =
                _lookAction.activeControl?.device is Gamepad;

            if (_jumpAction.WasPressedThisFrame())
            {
                _jumpPressed = true;
            }

            if (_crouchAction.WasPressedThisFrame())
            {
                _crouchPressed = true;
            }
        }

        public bool ConsumeJump()
        {
            if (!_jumpPressed)
            {
                return false;
            }

            _jumpPressed = false;
            return true;
        }

        public bool ConsumeCrouch()
        {
            if (!_crouchPressed)
            {
                return false;
            }

            _crouchPressed = false;
            return true;
        }
    }
}