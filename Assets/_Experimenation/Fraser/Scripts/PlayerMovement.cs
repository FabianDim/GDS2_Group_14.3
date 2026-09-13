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

        // Set by State Authority (e.g. the QTE Runner boost) and replicated so
        // server simulation and client prediction stay in sync.
        [SerializeField] private float defaultAirMultiplier = 0.55f;

        [Header("Sprinting")]
        [Networked] private float NetworkedSprintSpeed { get; set; }

        [Networked] private float NetworkedAerialMultiplier { get; set; }
        [Networked] private float NetworkedAgilityMultiplier { get; set; }

        public float airMultiplier => NetworkedAerialMultiplier;

        public float agilityMultiplier => NetworkedAgilityMultiplier;

        private bool dashBoostActive;

        private TickTimer SpeedBoostTimer { get; set; }

        public float sprintSpeed => NetworkedSprintSpeed;
        [SerializeField] private float walkSpeed = 10f;
        [SerializeField] private float defaultSprintSpeed = 15f;
        [SerializeField] private float crouchSpeed = 5f;
        [SerializeField] private float acceleration = 12f;

        [Header("Responsiveness & Drag")]
        [SerializeField] private float movementMultiplier = 10f;
        [SerializeField] private float groundDrag = 6f;
        [SerializeField] private float airDrag;
        [SerializeField] private float slideDrag = 1f;

        [Header("Gravity")]
        [SerializeField] private float gravityMultiplier = 1.8f;

        // Set by State Authority (e.g. the QTE Runner boost) and replicated so
        // server simulation and client prediction stay in sync.
        [Networked] public float SpeedBoostMultiplier { get; set; }

        // Networked so powerup boosts replicate; initialized to defaults in
        // Spawned() because [Networked] properties start at 0.
        [Networked] private float NetworkedJumpForce { get; set; }
        [Networked] private NetworkButtons PreviousButtons { get; set; }

        private const float DefaultJumpForce = 7f;

        private Vector3 _horizontalVelocity;
        private SimpleKCC _kcc;
        private Slide _slide;
        private WallRun _wallRun;
        private Climb _climb;

        public bool IsGrounded { get; private set; }
        public bool IsCrouching { get; set; }
        public bool IsSliding { get; set; }
        public bool IsWallRunning { get; set; }
        public bool IsClimbing { get; set; }

        public float HorizontalSpeed => _horizontalVelocity.magnitude;
        public float NormalGravity => Physics.gravity.y * gravityMultiplier;
        private float JumpForce => NetworkedJumpForce;
        private float SprintSpeed => NetworkedSprintSpeed;

        public override void Spawned()
        {
            if (!HasStateAuthority) return;

            _kcc = GetComponent<SimpleKCC>();
            _slide = GetComponent<Slide>();
            _wallRun = GetComponent<WallRun>();
            _climb = GetComponent<Climb>();

            // Initialize networked values to their design defaults, otherwise
            // jump applies zero impulse and sprint lerps moveSpeed down to zero.
            NetworkedJumpForce = DefaultJumpForce;
            NetworkedSprintSpeed = defaultSprintSpeed;

            moveSpeed = walkSpeed;

            _kcc.SetGravity(
                NormalGravity
            );

            NetworkedAerialMultiplier = defaultAirMultiplier;
            NetworkedAgilityMultiplier = 1f;
            NetworkedSprintSpeed = defaultSprintSpeed;
            NetworkedJumpForce = DefaultJumpForce;
        }

        public override void FixedUpdateNetwork()
        {
            if (_kcc == null || !GetInput(out GameplayInput input))
                return;

            IsGrounded = _kcc.IsGrounded;

            _climb?.UpdateClimbState(input);
            _wallRun?.UpdateWallRunState();

            ControlSpeed(input);

            Vector3 moveDirection = GetMoveDirection(input.MoveInput);

            Vector3 movementVelocity;
            if (IsClimbing && _climb != null)
            {
                movementVelocity = _climb.GetClimbVelocity(input);
            }
            else
            {
                if (IsWallRunning && _wallRun != null) WallRunMovement(moveDirection);
                else if (IsSliding) SlideMovement();
                else if (IsGrounded) GroundMovement(moveDirection);
                else AirMovement(moveDirection);

                movementVelocity = _horizontalVelocity;
            }

            float jumpImpulse = HandleJump(input, ref movementVelocity);

            _kcc.Move(movementVelocity, jumpImpulse);

            PreviousButtons = input.Buttons;
        }

        private Vector3 GetMoveDirection(Vector2 movementInput)
        {
            var direction = orientation.forward * movementInput.y + orientation.right * movementInput.x;
            direction.y = 0f;
            if (direction.sqrMagnitude > 1f)
                direction.Normalize();

            return direction;
        }

        private float HandleJump(GameplayInput input, ref Vector3 movementVelocity)
        {
            if (!input.Buttons.WasPressed(PreviousButtons, InputButton.Jump))
                return 0f;

            if (IsWallRunning && _wallRun != null)
            {
                _horizontalVelocity += _wallRun.GetWallJumpHorizontalImpulse();
                movementVelocity = _horizontalVelocity;
                _wallRun.StopWallRunFromJump();
                return _wallRun.WallJumpVerticalForce;
            }

            if (IsGrounded)
            {
                if (IsSliding && _slide != null)
                    _slide.StopSlideFromJump();

                return JumpForce;
            }

            return 0f;
        }

        private void GroundMovement(Vector3 moveDirection)
        {
            if (moveDirection.sqrMagnitude > 0.01f)
            {
                var targetVelocity = moveDirection * moveSpeed;
                _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, targetVelocity, movementMultiplier * Runner.DeltaTime);
            }
            else
            {
                _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, Vector3.zero, groundDrag * Runner.DeltaTime);
            }
        }

        private void AirMovement(Vector3 moveDirection)
        {
            if (moveDirection.sqrMagnitude > 0.01f)
            {
                var targetVelocity = moveDirection * moveSpeed;
                _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, targetVelocity, movementMultiplier * airMultiplier * Runner.DeltaTime);
            }

            if (airDrag > 0f)
            {
                _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, Vector3.zero, airDrag * Runner.DeltaTime);
            }
        }

        private void WallRunMovement(Vector3 moveDirection)
        {
            var wallMoveDirection = _wallRun.GetWallRunDirection(moveDirection);

            if (wallMoveDirection.sqrMagnitude > 0.01f)
            {
                var targetVelocity = wallMoveDirection * moveSpeed;
                _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, targetVelocity, movementMultiplier * airMultiplier * Runner.DeltaTime);
            }

            _horizontalVelocity = _wallRun.GetWallRunVelocity(_horizontalVelocity);
        }

        private void SlideMovement()
        {
            _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, Vector3.zero, slideDrag * Runner.DeltaTime);
        }

        private void ControlSpeed(GameplayInput input)
        {
            float speedMultiplier = Mathf.Max(1f, SpeedBoostMultiplier);
            float targetSpeed = IsGrounded switch
            {
                true when IsCrouching && !IsSliding => crouchSpeed * speedMultiplier,
                true when input.Buttons.IsSet(InputButton.SprintHeld) => SprintSpeed * speedMultiplier,
                _ => walkSpeed * speedMultiplier
            };

            moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, acceleration * Runner.DeltaTime);
        }

        public void AddSlideImpulse(Vector3 direction, float force)
        {
            _horizontalVelocity += direction.normalized * force;
        }

        public void ClearMovementVelocity()
        {
            _horizontalVelocity = Vector3.zero;
        }

        public void SetGravity(float gravity)
        {
            _kcc?.SetGravity(gravity);
        }

        internal void ApplyJumpBoost(float boostMultiplier, float maxJumpForce)
        {
            if (!HasStateAuthority)
                return;

            NetworkedJumpForce = Mathf.Min(maxJumpForce, DefaultJumpForce + DefaultJumpForce * boostMultiplier);
        }

        internal void ApplyDashBoost(float boostMultiplier)
        {
            if (!HasStateAuthority)
                return;

            NetworkedSprintSpeed = Mathf.Min(
                maxMoveSpeed,
                defaultSprintSpeed + defaultSprintSpeed * boostMultiplier
            );

            dashBoostActive = true;
        }
        internal void ApplyAerialControlBoost(float boostMultiplier)
        {
            if (!HasStateAuthority)
                return;

            NetworkedAerialMultiplier = Mathf.Min(
                defaultAirMultiplier * 2, //Not sure what air should stay around.
                defaultAirMultiplier * boostMultiplier
            );
        }
        internal void ApplyAgilityBoost(float boostMultiplier)
        {
            if (!HasStateAuthority)
                return;

            NetworkedAgilityMultiplier = boostMultiplier;

        }
    }
}
