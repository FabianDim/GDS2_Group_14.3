using UnityEngine;
using UnityEngine.InputSystem;

public class Climb : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;

    [Header("Climbing")]
    [SerializeField] private float climbSpeed = 5f;
    [SerializeField] private float climbSideSpeed = 2.5f;
    [SerializeField] private float climbDistance = 0.7f;
    [SerializeField] private float wallPullForce = 5f;
    [SerializeField] private float climbHeightMultiplier = 5f;

    [Header("Top Out")]
    [SerializeField] private float lowerCheckOffset = 0.7f;
    [SerializeField] private float topOutForwardForce = 3f;

    public bool isClimbing { get; private set; }

    private bool climbLocked;

    private float maximumClimbHeight;
    private float climbDistanceUsed;
    private float previousClimbHeight;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private PlayerMovement playerMovement;
    private WallRun wallRun;

    private PlayerInput playerInput;
    private InputAction moveAction;

    private RaycastHit wallHit;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        playerMovement = GetComponent<PlayerMovement>();
        wallRun = GetComponent<WallRun>();

        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];

        maximumClimbHeight =
            capsuleCollider.height * climbHeightMultiplier;
    }

    private void Update()
    {
        if (playerMovement.isGrounded)
        {
            climbLocked = false;
            climbDistanceUsed = 0f;
        }

        Vector2 movementInput = moveAction.ReadValue<Vector2>();

        Vector3 lowerCheckPosition =
            transform.position +
            Vector3.down * lowerCheckOffset;

        bool wallInFront = Physics.Raycast(
            transform.position,
            orientation.forward,
            out wallHit,
            climbDistance
        );

        bool lowerWallInFront = Physics.Raycast(
            lowerCheckPosition,
            orientation.forward,
            climbDistance
        );

        bool movingForward = movementInput.y > 0.1f;

        if (isClimbing)
        {
            TrackClimbDistance();

            if (climbDistanceUsed >= maximumClimbHeight)
            {
                ExhaustClimb();
                return;
            }

            if (movingForward &&
                (wallInFront || lowerWallInFront))
            {
                return;
            }

            StopClimb();
            return;
        }

        if (!climbLocked &&
            climbDistanceUsed < maximumClimbHeight &&
            !playerMovement.isCrouching &&
            !playerMovement.isSliding &&
            movingForward &&
            wallInFront)
        {
            StartClimb();
        }
    }

    private void FixedUpdate()
    {
        if (isClimbing)
        {
            ClimbMovement();
        }
    }

    private void StartClimb()
    {
        if (!isClimbing)
        {
            wallRun.StopWallRunFromClimb();

            previousClimbHeight = transform.position.y;

            rb.linearVelocity = Vector3.zero;
        }

        isClimbing = true;
        playerMovement.isClimbing = true;

        rb.useGravity = false;
    }

    private void StopClimb()
    {
        if (!isClimbing)
        {
            return;
        }

        isClimbing = false;
        playerMovement.isClimbing = false;

        rb.useGravity = true;
    }

    private void ExhaustClimb()
    {
        climbLocked = true;

        isClimbing = false;
        playerMovement.isClimbing = false;

        rb.useGravity = true;

        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );
    }

    private void TrackClimbDistance()
    {
        float heightDifference =
            transform.position.y - previousClimbHeight;

        if (heightDifference > 0f)
        {
            climbDistanceUsed += heightDifference;
        }

        previousClimbHeight = transform.position.y;
    }

    private void ClimbMovement()
    {
        Vector2 movementInput = moveAction.ReadValue<Vector2>();

        Vector3 lowerCheckPosition =
            transform.position +
            Vector3.down * lowerCheckOffset;

        bool wallInFront = Physics.Raycast(
            transform.position,
            orientation.forward,
            out RaycastHit centreWallHit,
            climbDistance
        );

        bool lowerWallInFront = Physics.Raycast(
            lowerCheckPosition,
            orientation.forward,
            out RaycastHit lowerWallHit,
            climbDistance
        );

        RaycastHit currentWallHit;

        if (wallInFront)
        {
            currentWallHit = centreWallHit;
        }
        else
        {
            currentWallHit = lowerWallHit;
        }

        Vector3 wallSideDirection =
            Vector3.Cross(
                Vector3.up,
                currentWallHit.normal
            ).normalized;

        if (Vector3.Dot(
                wallSideDirection,
                orientation.right
            ) < 0f)
        {
            wallSideDirection = -wallSideDirection;
        }

        Vector3 sideMovement =
            wallSideDirection *
            movementInput.x *
            climbSideSpeed;

        rb.linearVelocity = new Vector3(
            sideMovement.x,
            climbSpeed,
            sideMovement.z
        );

        if (wallInFront)
        {
            rb.AddForce(
                -centreWallHit.normal * wallPullForce,
                ForceMode.Acceleration
            );
        }
        else if (lowerWallInFront)
        {
            rb.AddForce(
                orientation.forward * topOutForwardForce,
                ForceMode.Acceleration
            );
        }
    }
}