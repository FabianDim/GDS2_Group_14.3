using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using UnityEngine;

namespace _Experimenation.Fraser.Scripts
{
    public class PlayerLook : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cam;
        [SerializeField] private Transform orientation;

        [Header("Sensitivity")]
        [SerializeField] private float mouseSensitivity = 0.1f;
        [SerializeField] private float controllerSensitivity = 180f;

        private float _pitch;
        private float _yaw;

        public override void Spawned()
        {
            if (!Object.HasInputAuthority)
                return;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasInputAuthority)
                return;

            if (!GetInput<GameplayInput>(out var input))
                return;

            var sensitivity = input.UsingGamepadLook
                ? controllerSensitivity * Runner.DeltaTime
                : mouseSensitivity;
            
            _yaw += input.LookInput.x * sensitivity;
            _pitch -= input.LookInput.y * sensitivity;

            _pitch = Mathf.Clamp(_pitch, -90f, 90f);

            orientation.rotation = Quaternion.Euler(0f, _yaw, 0f);
            cam.localRotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }
    }
}