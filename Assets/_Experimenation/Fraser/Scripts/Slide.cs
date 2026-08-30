using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

namespace _Experimenation.Fraser.Scripts
{
    public class Slide : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform orientation;
        [SerializeField] private Transform cameraPosition;

        [Header("Crouching")]
        [SerializeField] private float standingHeight = 2f;
        [SerializeField] private float crouchHeight = 1f;
        [SerializeField] private float crouchCameraHeight = 0.4f;
        [SerializeField] private float cameraCrouchSpeed = 15f;

        [Header("Sliding")]
        [SerializeField] private float slideForce = 6f;
        [SerializeField] private float slideDuration = 0.5f;
        [SerializeField] private float minimumSlideSpeed = 10f;

        [Header("Standing")]
        [SerializeField] private LayerMask obstacleMask;

        private bool IsCrouching { get; set; }
        private bool IsSliding { get; set; }

        private float _standingCameraHeight;
        private float _slideTimer;

        private SimpleKCC _kcc;
        private PlayerMovement _playerMovement;

        public override void Spawned()
        {
            _kcc = GetComponent<SimpleKCC>();
            _playerMovement = GetComponent<PlayerMovement>();

            _standingCameraHeight =
                cameraPosition.localPosition.y;

            _kcc.SetHeight(standingHeight);
        }

        public override void FixedUpdateNetwork()
        {
            if (_kcc == null ||
                _playerMovement == null ||
                !GetInput(out GameplayInput input))
            {
                return;
            }

            if (input.Crouch)
            {
                StartCrouch();

                if (CanSlide())
                {
                    StartSlide();
                }
            }

            if (!input.CrouchHeld)
            {
                StopSlide();
                TryStopCrouch();
            }

            if (!IsSliding) return;
            _slideTimer -= Runner.DeltaTime;

            if (_slideTimer <= 0f)
            {
                StopSlide();
            }
        }

        private void LateUpdate()
        {
            UpdateCameraHeight();
        }

        private void StartCrouch()
        {
            if (IsCrouching)
            {
                return;
            }

            IsCrouching = true;
            _playerMovement.IsCrouching = true;

            _kcc.SetHeight(crouchHeight);
        }

        private void TryStopCrouch()
        {
            if (!IsCrouching)
            {
                return;
            }

            if (!CanStand())
            {
                return;
            }

            IsCrouching = false;
            _playerMovement.IsCrouching = false;

            _kcc.SetHeight(standingHeight);
        }

        private void StartSlide()
        {
            if (IsSliding)
            {
                return;
            }

            IsSliding = true;
            _playerMovement.IsSliding = true;

            _slideTimer = slideDuration;

            Vector3 slideDirection =
                orientation.forward;

            _playerMovement.AddSlideImpulse(
                slideDirection,
                slideForce
            );
        }

        private void StopSlide()
        {
            if (!IsSliding)
            {
                return;
            }

            IsSliding = false;
            _playerMovement.IsSliding = false;
        }

        public void StopSlideFromJump()
        {
            StopSlide();
        }

        private bool CanSlide()
        {
            return
                _playerMovement.IsGrounded &&
                _playerMovement.HorizontalSpeed >=
                minimumSlideSpeed;
        }

        private bool CanStand()
        {
            float extraHeight =
                standingHeight - crouchHeight;

            Vector3 origin =
                transform.position +
                Vector3.up * (crouchHeight * 0.5f);

            return !Physics.Raycast(
                origin,
                Vector3.up,
                extraHeight,
                obstacleMask,
                QueryTriggerInteraction.Ignore
            );
        }

        private void UpdateCameraHeight()
        {

            var targetHeight = IsCrouching ? crouchCameraHeight : _standingCameraHeight;

            var cameraLocalPosition =
                cameraPosition.localPosition;

            cameraLocalPosition.y = Mathf.Lerp(
                cameraLocalPosition.y,
                targetHeight,
                cameraCrouchSpeed * Time.deltaTime
            );

            cameraPosition.localPosition =
                cameraLocalPosition;
        }
    }
}