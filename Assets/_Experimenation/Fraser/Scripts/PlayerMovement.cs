using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

namespace _Experimenation.Fraser.Scripts
{
    public class PlayerMovement : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform orientation;

        [Header("Movement")]
        [SerializeField] public float moveSpeed = 10f;
        public float defaultMoveSpeed = 10f;
        [SerializeField] public float maxMoveSpeed = 40f;

        [SerializeField] private float movementMultiplier = 10f;
        [SerializeField] private float airMultiplier = 0.55f;

        [Header("Sprinting")]
        [SerializeField] private float walkSpeed = 10f;
        [SerializeField] private float sprintSpeed = 15f;
        [SerializeField] private float crouchSpeed = 5f;
        [SerializeField] private float acceleration = 12f;

        [Header("Jumping")]
        public float jumpForce = 7f;

        public float defaultJumpForce = 7f;
        public float maxJumpForce = 14f;

        [Header("Gravity")]
        [SerializeField] private float gravityMultiplier = 1.8f;

        [Header("Drag")]
        [SerializeField] private float groundDrag = 6f;
        [SerializeField] private float airDrag;
        [SerializeField] private float slideDrag = 1f;

        public bool IsGrounded { get; private set; }
        public bool IsCrouching { get; set; }
        public bool IsSliding { get; set; }
        public bool IsWallRunning { get; set; }
        public bool IsClimbing { get; set; }

        public float HorizontalSpeed
        {
            get
            {
                return _horizontalVelocity.magnitude;
            }
        }

        public float NormalGravity
        {
            get
            {
                return Physics.gravity.y *
                       gravityMultiplier;
            }
        }

        private Vector3 _horizontalVelocity;

        private SimpleKCC _kcc;
        private Slide _slide;
        private WallRun _wallRun;
        private Climb _climb;

        public override void Spawned()
        {
            _kcc =
                GetComponent<SimpleKCC>();

            _slide =
                GetComponent<Slide>();

            _wallRun =
                GetComponent<WallRun>();

            _climb =
                GetComponent<Climb>();

            moveSpeed = walkSpeed;

            _kcc.SetGravity(
                NormalGravity
            );
        }

        public override void FixedUpdateNetwork()
        {
            Debug.Log(
                $"Object: {Object.Id} | " +
                $"InputAuthority: {Object.InputAuthority} | " +
                $"StateAuthority: {Object.StateAuthority} | " +
                $"HasInput: {Object.HasInputAuthority} | " +
                $"IsSimulated: {Object.IsInSimulation}"
            );

            if (!_kcc || !GetInput<GameplayInput>(out var input)) return;

            IsGrounded =
                _kcc.IsGrounded;

            if (_climb != null)
            {
                _climb.UpdateClimbState(input);
            }

            if (_wallRun != null)
            {
                _wallRun.UpdateWallRunState();
            }

            ControlSpeed(input);

            var movementInput =
                input.MoveInput;

            var moveDirection =
                orientation.forward *
                movementInput.y +
                orientation.right *
                movementInput.x;

            moveDirection.y = 0f;

            if (moveDirection.sqrMagnitude > 1f)
            {
                moveDirection.Normalize();
            }

            Vector3 movementVelocity;

            if (IsClimbing &&
                _climb != null)
            {
                movementVelocity =
                    _climb.GetClimbVelocity(input);
            }
            else
            {
                if (IsWallRunning &&
                    _wallRun != null)
                {
                    WallRunMovement(
                        moveDirection
                    );
                }
                else if (IsSliding)
                {
                    SlideMovement();
                }
                else if (IsGrounded)
                {
                    GroundMovement(
                        moveDirection
                    );
                }
                else
                {
                    AirMovement(
                        moveDirection
                    );
                }

                movementVelocity =
                    _horizontalVelocity;
            }

            var jumpImpulse = 0f;

            if (input.Jump)
            {
                if (IsWallRunning &&
                    _wallRun != null)
                {
                    _horizontalVelocity +=
                        _wallRun
                            .GetWallJumpHorizontalImpulse();

                    jumpImpulse =
                        _wallRun
                            .WallJumpVerticalForce;

                    _wallRun
                        .StopWallRunFromJump();

                    movementVelocity =
                        _horizontalVelocity;
                }
                else if (IsGrounded)
                {
                    if (IsSliding &&
                        _slide != null)
                    {
                        _slide
                            .StopSlideFromJump();
                    }

                    jumpImpulse =
                        jumpForce;
                }
            }

            _kcc.Move(
                movementVelocity,
                jumpImpulse
            );
        }

        private void GroundMovement(
            Vector3 moveDirection
        )
        {
            Vector3 targetVelocity =
                moveDirection *
                moveSpeed;

            if (moveDirection.sqrMagnitude >
                0.01f)
            {
                _horizontalVelocity =
                    Vector3.Lerp(
                        _horizontalVelocity,
                        targetVelocity,
                        movementMultiplier *
                        Runner.DeltaTime
                    );
            }
            else
            {
                _horizontalVelocity =
                    Vector3.Lerp(
                        _horizontalVelocity,
                        Vector3.zero,
                        groundDrag *
                        Runner.DeltaTime
                    );
            }
        }

        private void AirMovement(
            Vector3 moveDirection
        )
        {
            if (moveDirection.sqrMagnitude >
                0.01f)
            {
                Vector3 targetVelocity =
                    moveDirection *
                    moveSpeed;

                _horizontalVelocity =
                    Vector3.Lerp(
                        _horizontalVelocity,
                        targetVelocity,
                        movementMultiplier *
                        airMultiplier *
                        Runner.DeltaTime
                    );
            }

            if (airDrag > 0f)
            {
                _horizontalVelocity =
                    Vector3.Lerp(
                        _horizontalVelocity,
                        Vector3.zero,
                        airDrag *
                        Runner.DeltaTime
                    );
            }
        }

        private void WallRunMovement(
            Vector3 moveDirection
        )
        {
            Vector3 wallMoveDirection =
                _wallRun.GetWallRunDirection(
                    moveDirection
                );

            if (wallMoveDirection.sqrMagnitude >
                0.01f)
            {
                Vector3 targetVelocity =
                    wallMoveDirection *
                    moveSpeed;

                _horizontalVelocity =
                    Vector3.Lerp(
                        _horizontalVelocity,
                        targetVelocity,
                        movementMultiplier *
                        airMultiplier *
                        Runner.DeltaTime
                    );
            }

            _horizontalVelocity =
                _wallRun.GetWallRunVelocity(
                    _horizontalVelocity
                );
        }

        private void SlideMovement()
        {
            _horizontalVelocity =
                Vector3.Lerp(
                    _horizontalVelocity,
                    Vector3.zero,
                    slideDrag *
                    Runner.DeltaTime
                );
        }

        private void ControlSpeed(GameplayInput input)
        {

            var targetSpeed = IsGrounded switch
            {
                true when IsCrouching && !IsSliding => crouchSpeed,
                true when input.SprintHeld => sprintSpeed,
                true => walkSpeed,
                _ => moveSpeed
            };

            moveSpeed =
                Mathf.Lerp(
                    moveSpeed,
                    targetSpeed,
                    acceleration *
                    Runner.DeltaTime
                );
        }

        public void AddSlideImpulse(
            Vector3 direction,
            float force
        )
        {
            _horizontalVelocity +=
                direction.normalized *
                force;
        }

        public void ClearMovementVelocity()
        {
            _horizontalVelocity =
                Vector3.zero;
        }

        public void SetGravity(
            float gravity
        )
        {
            if (_kcc != null)
            {
                _kcc.SetGravity(
                    gravity
                );
            }
        }
    }
}