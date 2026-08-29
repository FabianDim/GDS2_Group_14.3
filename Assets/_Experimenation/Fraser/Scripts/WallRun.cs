using Fusion;
using UnityEngine;

namespace _Experimenation.Fraser.Scripts
{
    public class WallRun : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform orientation;

        [Header("Detection")]
        [SerializeField] private LayerMask wallMask;
        [SerializeField] private float wallDistance = 1f;
        [SerializeField] private float wallCheckHeight = 1f;
        [SerializeField] private float wallCheckRadius = 0.15f;

        [Header("Wall Running")]
        [SerializeField] private float wallRunGravity = -2f;
        [SerializeField] private float wallRunJumpHorizontalForce = 7f;
        [SerializeField] private float wallRunJumpVerticalForce = 7f;
        [SerializeField] private float wallJumpReattachDelay = 0.15f;

        [Header("Camera")]
        [SerializeField] private Camera cam;
        [SerializeField] private float fov = 90f;
        [SerializeField] private float wallRunFov = 110f;
        [SerializeField] private float wallRunFovTime = 8f;
        [SerializeField] private float camTilt = 10f;
        [SerializeField] private float camTiltTime = 8f;

        [Header("Debug")]
        [SerializeField] private bool wallLeft;
        [SerializeField] private bool wallRight;
        [SerializeField] private bool grounded;
        [SerializeField] private bool blockedByMovementState;
        [SerializeField] private bool sameWallBlocked;

        private bool IsWallRunning { get; set; }
        private float Tilt { get; set; }

        public float WallJumpVerticalForce
        {
            get
            {
                return wallRunJumpVerticalForce;
            }
        }

        private RaycastHit _leftWallHit;
        private RaycastHit _rightWallHit;
        private RaycastHit _currentWallHit;

        private Collider _blockedWallCollider;

        private PlayerMovement _playerMovement;

        private TickTimer _wallJumpReattachTimer;

        public override void Spawned()
        {
            _playerMovement =
                GetComponent<PlayerMovement>();
        }

        public void UpdateWallRunState()
        {
            if (_playerMovement == null)
            {
                return;
            }

            if (_playerMovement.IsGrounded)
            {
                _blockedWallCollider = null;
            }

            CheckWall();

            grounded =
                _playerMovement.IsGrounded;

            blockedByMovementState =
                _playerMovement.IsSliding ||
                _playerMovement.IsCrouching ||
                _playerMovement.IsClimbing;

            if (_playerMovement.IsClimbing ||
                !_wallJumpReattachTimer
                    .ExpiredOrNotRunning(Runner) ||
                grounded ||
                blockedByMovementState)
            {
                StopWallRun();
                return;
            }

            var canUseLeftWall =
                wallLeft &&
                _leftWallHit.collider !=
                _blockedWallCollider;

            var canUseRightWall =
                wallRight &&
                _rightWallHit.collider !=
                _blockedWallCollider;

            sameWallBlocked =
                (wallLeft &&
                 _leftWallHit.collider ==
                 _blockedWallCollider) ||
                (wallRight &&
                 _rightWallHit.collider ==
                 _blockedWallCollider);

            if (canUseLeftWall)
            {
                _currentWallHit =
                    _leftWallHit;

                StartWallRun();
            }
            else if (canUseRightWall)
            {
                _currentWallHit =
                    _rightWallHit;

                StartWallRun();
            }
            else
            {
                StopWallRun();
            }
        }

        private void CheckWall()
        {
            var physicsScene =
                Runner.GetPhysicsScene();

            var checkPosition =
                transform.position +
                Vector3.up *
                wallCheckHeight;

            wallLeft =
                physicsScene.SphereCast(
                    checkPosition,
                    wallCheckRadius,
                    -orientation.right,
                    out _leftWallHit,
                    wallDistance,
                    wallMask,
                    QueryTriggerInteraction.Ignore
                );

            wallRight =
                physicsScene.SphereCast(
                    checkPosition,
                    wallCheckRadius,
                    orientation.right,
                    out _rightWallHit,
                    wallDistance,
                    wallMask,
                    QueryTriggerInteraction.Ignore
                );

            Debug.DrawRay(
                checkPosition,
                -orientation.right *
                wallDistance,
                wallLeft ?
                    Color.yellow :
                    Color.red
            );

            Debug.DrawRay(
                checkPosition,
                orientation.right *
                wallDistance,
                wallRight ?
                    Color.yellow :
                    Color.green
            );
        }

        private void StartWallRun()
        {
            if (!IsWallRunning)
            {
                IsWallRunning = true;

                _playerMovement
                    .IsWallRunning = true;
            }

            _playerMovement.SetGravity(
                wallRunGravity
            );
        }

        private void StopWallRun()
        {
            if (!IsWallRunning)
            {
                return;
            }

            IsWallRunning = false;

            _playerMovement
                .IsWallRunning = false;

            if (!_playerMovement.IsClimbing)
            {
                _playerMovement.SetGravity(
                    _playerMovement.NormalGravity
                );
            }
        }

        public void StopWallRunFromJump()
        {
            if (_currentWallHit.collider != null)
            {
                _blockedWallCollider =
                    _currentWallHit.collider;
            }

            _wallJumpReattachTimer =
                TickTimer.CreateFromSeconds(
                    Runner,
                    wallJumpReattachDelay
                );

            StopWallRun();
        }

        public void StopWallRunFromClimb()
        {
            if (!IsWallRunning)
            {
                return;
            }

            IsWallRunning = false;

            _playerMovement
                .IsWallRunning = false;
        }

        public Vector3 GetWallRunDirection(
            Vector3 inputDirection
        )
        {
            var wallDirection =
                Vector3.ProjectOnPlane(
                    inputDirection,
                    _currentWallHit.normal
                );

            wallDirection.y = 0f;

            if (wallDirection.sqrMagnitude >
                1f)
            {
                wallDirection.Normalize();
            }

            return wallDirection;
        }

        public Vector3 GetWallRunVelocity(
            Vector3 currentVelocity
        )
        {
            var velocityAlongWall =
                Vector3.ProjectOnPlane(
                    currentVelocity,
                    _currentWallHit.normal
                );

            velocityAlongWall.y = 0f;

            return velocityAlongWall;
        }

        public Vector3
            GetWallJumpHorizontalImpulse()
        {
            var awayFromWall =
                _currentWallHit.normal;

            awayFromWall.y = 0f;

            return
                awayFromWall.normalized *
                wallRunJumpHorizontalForce;
        }

        private void LateUpdate()
        {
            UpdateCameraEffects();
        }

        private void UpdateCameraEffects()
        {
            if (cam == null)
            {
                return;
            }

            float targetFov;

            if (IsWallRunning)
            {
                targetFov =
                    wallRunFov;
            }
            else
            {
                targetFov =
                    fov;
            }

            cam.fieldOfView =
                Mathf.Lerp(
                    cam.fieldOfView,
                    targetFov,
                    wallRunFovTime *
                    Time.deltaTime
                );

            var targetTilt = 0f;

            if (IsWallRunning)
            {
                if (wallLeft)
                {
                    targetTilt =
                        -camTilt;
                }
                else if (wallRight)
                {
                    targetTilt =
                        camTilt;
                }
            }

            Tilt =
                Mathf.Lerp(
                    Tilt,
                    targetTilt,
                    camTiltTime *
                    Time.deltaTime
                );
        }
    }
}