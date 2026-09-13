using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

namespace _Experimenation.Fraser.Scripts
{
    /// <summary>
    /// Networked player movement. State Authority owns the replicated movement
    /// modifiers, while every peer runs the same input-driven movement pipeline.
    /// </summary>
    [RequireComponent(typeof(SimpleKCC))]
    public class PlayerMovement : NetworkBehaviour, IGameplayInputConsumer
    {
        private const float DefaultJumpForce = 7f;

        [Header("References")]
        [SerializeField] private Transform orientation;

        [Header("Movement")]
        [SerializeField] public float moveSpeed = 10f;
        public float defaultMoveSpeed = 10f; // Kept for existing prefab compatibility.
        [SerializeField] public float maxMoveSpeed = 40f;
        [SerializeField] private float walkSpeed = 10f;
        [SerializeField] private float defaultSprintSpeed = 15f;
        [SerializeField] private float crouchSpeed = 5f;
        [SerializeField] private float acceleration = 12f;

        [Header("Acceleration and Drag")]
        [SerializeField] private float movementMultiplier = 10f;
        [SerializeField] private float defaultAirMultiplier = 0.55f;
        [SerializeField] private float groundDrag = 6f;
        [SerializeField] private float airDrag;
        [SerializeField] private float slideDrag = 1f;

        [Header("Gravity")]
        [SerializeField] private float gravityMultiplier = 1.8f;

        // These values are replicated because abilities can modify them at runtime.
        [Networked] public float SpeedBoostMultiplier { get; set; }
        [Networked] private float NetworkedSprintSpeed { get; set; }
        [Networked] private float NetworkedAerialMultiplier { get; set; }
        [Networked] private float NetworkedAgilityMultiplier { get; set; }
        [Networked] private float NetworkedJumpForce { get; set; }

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
        public float AgilityMultiplier => NetworkedAgilityMultiplier > 0f
            ? NetworkedAgilityMultiplier
            : 1f;

        private float AirMultiplier => NetworkedAerialMultiplier > 0f
            ? NetworkedAerialMultiplier
            : defaultAirMultiplier;

        private float JumpForce => NetworkedJumpForce > 0f
            ? NetworkedJumpForce
            : DefaultJumpForce;

        private float SprintSpeed => NetworkedSprintSpeed > 0f
            ? NetworkedSprintSpeed
            : defaultSprintSpeed;

        public override void Spawned()
        {
            CacheMovementComponents();

            if (_kcc == null)
            {
                Debug.LogError("PlayerMovement requires a SimpleKCC component.", this);
                return;
            }

            moveSpeed = walkSpeed;
            _kcc.SetGravity(NormalGravity);

            if (!HasStateAuthority)
                return;

            // Networked properties start at zero. Initialize their baseline values
            // before abilities or sprint/jump input can use them.
            NetworkedSprintSpeed = defaultSprintSpeed;
            NetworkedAerialMultiplier = defaultAirMultiplier;
            NetworkedAgilityMultiplier = 1f;
            NetworkedJumpForce = DefaultJumpForce;
        }

        public void ProcessInput(GameplayInput input, NetworkButtons previousButtons)
        {
            if (_kcc == null)
                return;

            IsGrounded = _kcc.IsGrounded;
            UpdateMovementStates(input);
            ControlSpeed(input);

            var moveDirection = GetMoveDirection(input.MoveInput);
            var movementVelocity = GetMovementVelocity(input, moveDirection);
            var jumpImpulse = HandleJump(input, previousButtons, ref movementVelocity);

            _kcc.Move(movementVelocity, jumpImpulse);
        }

        private void CacheMovementComponents()
        {
            _kcc = GetComponent<SimpleKCC>();
            _slide = GetComponent<Slide>();
            _wallRun = GetComponent<WallRun>();
            _climb = GetComponent<Climb>();
        }

        private void UpdateMovementStates(GameplayInput input)
        {
            _climb?.UpdateClimbState(input);
            _wallRun?.UpdateWallRunState();
        }

        private Vector3 GetMovementVelocity(GameplayInput input, Vector3 moveDirection)
        {
            if (IsClimbing && _climb != null)
                return _climb.GetClimbVelocity(input);

            if (IsWallRunning && _wallRun != null)
                WallRunMovement(moveDirection);
            else if (IsSliding)
                SlideMovement();
            else if (IsGrounded)
                GroundMovement(moveDirection);
            else
                AirMovement(moveDirection);

            return _horizontalVelocity;
        }

        private Vector3 GetMoveDirection(Vector2 movementInput)
        {
            if (orientation == null)
                return Vector3.zero;

            var direction = orientation.forward * movementInput.y +
                            orientation.right * movementInput.x;
            direction.y = 0f;

            return direction.sqrMagnitude > 1f
                ? direction.normalized
                : direction;
        }

        private float HandleJump(
            GameplayInput input,
            NetworkButtons previousButtons,
            ref Vector3 movementVelocity)
        {
            if (!input.Buttons.WasPressed(previousButtons, InputButton.Jump))
                return 0f;

            if (IsWallRunning && _wallRun != null)
            {
                _horizontalVelocity += _wallRun.GetWallJumpHorizontalImpulse();
                movementVelocity = _horizontalVelocity;
                _wallRun.StopWallRunFromJump();
                return _wallRun.WallJumpVerticalForce;
            }

            if (!IsGrounded)
                return 0f;

            if (IsSliding && _slide != null)
                _slide.StopSlideFromJump();

            return JumpForce;
        }

        private void GroundMovement(Vector3 moveDirection)
        {
            if (HasMovementInput(moveDirection))
            {
                var targetVelocity = moveDirection * moveSpeed;
                _horizontalVelocity = LerpVelocity(
                    targetVelocity,
                    movementMultiplier);
                return;
            }

            _horizontalVelocity = LerpVelocity(Vector3.zero, groundDrag);
        }

        private void AirMovement(Vector3 moveDirection)
        {
            if (HasMovementInput(moveDirection))
            {
                var targetVelocity = moveDirection * moveSpeed;
                _horizontalVelocity = LerpVelocity(
                    targetVelocity,
                    movementMultiplier * AirMultiplier);
            }

            if (airDrag > 0f)
                _horizontalVelocity = LerpVelocity(Vector3.zero, airDrag);
        }

        private void WallRunMovement(Vector3 moveDirection)
        {
            var wallMoveDirection = _wallRun.GetWallRunDirection(moveDirection);
            if (HasMovementInput(wallMoveDirection))
            {
                var targetVelocity = wallMoveDirection * moveSpeed;
                _horizontalVelocity = LerpVelocity(
                    targetVelocity,
                    movementMultiplier * AirMultiplier);
            }

            _horizontalVelocity = _wallRun.GetWallRunVelocity(_horizontalVelocity);
        }

        private void SlideMovement()
        {
            _horizontalVelocity = LerpVelocity(Vector3.zero, slideDrag);
        }

        private Vector3 LerpVelocity(Vector3 targetVelocity, float response)
        {
            return Vector3.Lerp(
                _horizontalVelocity,
                targetVelocity,
                response * Runner.DeltaTime);
        }

        private static bool HasMovementInput(Vector3 direction)
        {
            return direction.sqrMagnitude > 0.01f;
        }

        private void ControlSpeed(GameplayInput input)
        {
            var speedMultiplier = Mathf.Max(1f, SpeedBoostMultiplier);
            var targetSpeed = GetTargetSpeed(input) * speedMultiplier;
            moveSpeed = Mathf.Lerp(
                moveSpeed,
                targetSpeed,
                acceleration * Runner.DeltaTime);
        }

        private float GetTargetSpeed(GameplayInput input)
        {
            if (!IsGrounded)
                return walkSpeed;

            if (IsCrouching && !IsSliding)
                return crouchSpeed;

            return input.Buttons.IsSet(InputButton.SprintHeld)
                ? SprintSpeed
                : walkSpeed;
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

            NetworkedJumpForce = Mathf.Min(
                maxJumpForce,
                DefaultJumpForce + DefaultJumpForce * boostMultiplier);
        }

        internal void ApplyDashBoost(float boostMultiplier)
        {
            if (!HasStateAuthority)
                return;

            NetworkedSprintSpeed = Mathf.Min(
                maxMoveSpeed,
                defaultSprintSpeed + defaultSprintSpeed * boostMultiplier);
        }

        internal void ApplyAerialControlBoost(float boostMultiplier)
        {
            if (!HasStateAuthority)
                return;

            NetworkedAerialMultiplier = Mathf.Min(
                defaultAirMultiplier * 2f,
                defaultAirMultiplier + defaultAirMultiplier * boostMultiplier);
        }

        internal void ApplyAgilityBoost(float boostMultiplier)
        {
            if (!HasStateAuthority)
                return;

            NetworkedAgilityMultiplier += NetworkedAgilityMultiplier * boostMultiplier;
        }
    }
}
