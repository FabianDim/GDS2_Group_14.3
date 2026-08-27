using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float movementMultiplier = 10f;
    [SerializeField] private float airMultiplier = 0.55f;

    [Header("Sprinting")]
    [SerializeField] private float walkSpeed = 10f;
    [SerializeField] private float sprintSpeed = 15f;
    [SerializeField] private float crouchSpeed = 5f;
    [SerializeField] private float acceleration = 12f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 7f;

    [Header("Gravity")]
    [SerializeField] private float gravityMultiplier = 1.8f;

    [Header("Drag")]
    [SerializeField] private float groundDrag = 6f;
    [SerializeField] private float airDrag = 0f;
    [SerializeField] private float slideDrag = 1f;

    public bool isGrounded { get; private set; }
    public bool isCrouching { get; set; }
    public bool isSliding { get; set; }
    public bool isWallRunning { get; set; }
    public bool isClimbing { get; set; }

    public float HorizontalSpeed
    {
        get
        {
            return horizontalVelocity.magnitude;
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

    private Vector3 horizontalVelocity;

    private SimpleKCC kcc;
    private LocalMovementInput localInput;
    private Slide slide;
    private WallRun wallRun;
    private Climb climb;

    public override void Spawned()
    {
        kcc =
            GetComponent<SimpleKCC>();

        localInput =
            GetComponent<LocalMovementInput>();

        slide =
            GetComponent<Slide>();

        wallRun =
            GetComponent<WallRun>();

        climb =
            GetComponent<Climb>();

        moveSpeed = walkSpeed;

        kcc.SetGravity(
            NormalGravity
        );
    }

    public override void FixedUpdateNetwork()
    {
        if (kcc == null ||
            localInput == null)
        {
            return;
        }

        isGrounded =
            kcc.IsGrounded;

        if (climb != null)
        {
            climb.UpdateClimbState();
        }

        if (wallRun != null)
        {
            wallRun.UpdateWallRunState();
        }

        ControlSpeed();

        Vector2 movementInput =
            localInput.MoveInput;

        Vector3 moveDirection =
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

        if (isClimbing &&
            climb != null)
        {
            movementVelocity =
                climb.GetClimbVelocity();
        }
        else
        {
            if (isWallRunning &&
                wallRun != null)
            {
                WallRunMovement(
                    moveDirection
                );
            }
            else if (isSliding)
            {
                SlideMovement();
            }
            else if (isGrounded)
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
                horizontalVelocity;
        }

        float jumpImpulse = 0f;

        if (localInput.ConsumeJump())
        {
            if (isWallRunning &&
                wallRun != null)
            {
                horizontalVelocity +=
                    wallRun
                    .GetWallJumpHorizontalImpulse();

                jumpImpulse =
                    wallRun
                    .WallJumpVerticalForce;

                wallRun
                    .StopWallRunFromJump();

                movementVelocity =
                    horizontalVelocity;
            }
            else if (isGrounded)
            {
                if (isSliding &&
                    slide != null)
                {
                    slide
                        .StopSlideFromJump();
                }

                jumpImpulse =
                    jumpForce;
            }
        }

        kcc.Move(
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
            horizontalVelocity =
                Vector3.Lerp(
                    horizontalVelocity,
                    targetVelocity,
                    movementMultiplier *
                    Runner.DeltaTime
                );
        }
        else
        {
            horizontalVelocity =
                Vector3.Lerp(
                    horizontalVelocity,
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

            horizontalVelocity =
                Vector3.Lerp(
                    horizontalVelocity,
                    targetVelocity,
                    movementMultiplier *
                    airMultiplier *
                    Runner.DeltaTime
                );
        }

        if (airDrag > 0f)
        {
            horizontalVelocity =
                Vector3.Lerp(
                    horizontalVelocity,
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
            wallRun.GetWallRunDirection(
                moveDirection
            );

        if (wallMoveDirection.sqrMagnitude >
            0.01f)
        {
            Vector3 targetVelocity =
                wallMoveDirection *
                moveSpeed;

            horizontalVelocity =
                Vector3.Lerp(
                    horizontalVelocity,
                    targetVelocity,
                    movementMultiplier *
                    airMultiplier *
                    Runner.DeltaTime
                );
        }

        horizontalVelocity =
            wallRun.GetWallRunVelocity(
                horizontalVelocity
            );
    }

    private void SlideMovement()
    {
        horizontalVelocity =
            Vector3.Lerp(
                horizontalVelocity,
                Vector3.zero,
                slideDrag *
                Runner.DeltaTime
            );
    }

    private void ControlSpeed()
    {
        float targetSpeed;

        if (isGrounded &&
            isCrouching &&
            !isSliding)
        {
            targetSpeed =
                crouchSpeed;
        }
        else if (
            isGrounded &&
            localInput.SprintHeld)
        {
            targetSpeed =
                sprintSpeed;
        }
        else if (isGrounded)
        {
            targetSpeed =
                walkSpeed;
        }
        else
        {
            targetSpeed =
                moveSpeed;
        }

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
        horizontalVelocity +=
            direction.normalized *
            force;
    }

    public void ClearMovementVelocity()
    {
        horizontalVelocity =
            Vector3.zero;
    }

    public void SetGravity(
        float gravity
    )
    {
        if (kcc != null)
        {
            kcc.SetGravity(
                gravity
            );
        }
    }
}