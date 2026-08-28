using Fusion;
using UnityEngine;

public class Climb : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;

    [Header("Detection")]
    [SerializeField] private LayerMask climbMask;
    [SerializeField] private float climbDistance = 0.8f;
    [SerializeField] private float wallCheckHeight = 1f;
    [SerializeField] private float lowerCheckHeight = 0.35f;
    [SerializeField] private float wallCheckRadius = 0.15f;

    [Header("Climbing")]
    [SerializeField] private float climbSpeed = 5f;
    [SerializeField] private float climbSideSpeed = 2.5f;
    [SerializeField] private float wallPullSpeed = 1f;
    [SerializeField] private float climbHeightMultiplier = 5f;

    [Header("Top Out")]
    [SerializeField] private float topOutForwardSpeed = 3f;

    [Header("Debug")]
    [SerializeField] private bool wallInFront;
    [SerializeField] private bool lowerWallInFront;
    [SerializeField] private bool climbLocked;
    [SerializeField] private float climbDistanceUsed;

    public bool isClimbing { get; private set; }

    private float maximumClimbHeight;
    private float previousClimbHeight;

    private RaycastHit wallHit;
    private RaycastHit lowerWallHit;

    private PlayerMovement playerMovement;
    private LocalMovementInput localInput;
    private WallRun wallRun;

    public override void Spawned()
    {
        playerMovement = GetComponent<PlayerMovement>();
        localInput = GetComponent<LocalMovementInput>();
        wallRun = GetComponent<WallRun>();

        maximumClimbHeight =
            2f * climbHeightMultiplier;

        previousClimbHeight =
            transform.position.y;
    }

    public void UpdateClimbState()
    {
        if (playerMovement == null ||
            localInput == null)
        {
            return;
        }

        if (playerMovement.isGrounded)
        {
            climbLocked = false;
            climbDistanceUsed = 0f;
        }

        CheckWall();

        if (isClimbing)
        {
            TrackClimbDistance();

            if (climbDistanceUsed >= maximumClimbHeight)
            {
                ExhaustClimb();
                return;
            }

            bool movingForward =
                localInput.MoveInput.y > 0.1f;

            if (movingForward &&
                (wallInFront || lowerWallInFront))
            {
                return;
            }

            StopClimb();
            return;
        }

        bool canStartClimb =
            !climbLocked &&
            !playerMovement.isCrouching &&
            !playerMovement.isSliding &&
            localInput.MoveInput.y > 0.1f &&
            wallInFront;

        if (canStartClimb)
        {
            StartClimb();
        }
    }

    private void CheckWall()
    {
        PhysicsScene physicsScene =
            Runner.GetPhysicsScene();

        Vector3 centrePosition =
            transform.position +
            Vector3.up * wallCheckHeight;

        Vector3 lowerPosition =
            transform.position +
            Vector3.up * lowerCheckHeight;

        wallInFront = physicsScene.SphereCast(
            centrePosition,
            wallCheckRadius,
            orientation.forward,
            out wallHit,
            climbDistance,
            climbMask,
            QueryTriggerInteraction.Ignore
        );

        lowerWallInFront = physicsScene.SphereCast(
            lowerPosition,
            wallCheckRadius,
            orientation.forward,
            out lowerWallHit,
            climbDistance,
            climbMask,
            QueryTriggerInteraction.Ignore
        );

        Debug.DrawRay(
            centrePosition,
            orientation.forward * climbDistance,
            wallInFront ? Color.yellow : Color.blue
        );

        Debug.DrawRay(
            lowerPosition,
            orientation.forward * climbDistance,
            lowerWallInFront ? Color.yellow : Color.cyan
        );
    }

    private void StartClimb()
    {
        if (isClimbing)
        {
            return;
        }

        isClimbing = true;
        playerMovement.isClimbing = true;

        previousClimbHeight =
            transform.position.y;

        playerMovement.ClearMovementVelocity();
        playerMovement.SetGravity(0f);

        if (wallRun != null)
        {
            wallRun.StopWallRunFromClimb();
        }
    }

    private void StopClimb()
    {
        if (!isClimbing)
        {
            return;
        }

        isClimbing = false;
        playerMovement.isClimbing = false;

        playerMovement.SetGravity(
            playerMovement.NormalGravity
        );
    }

    private void ExhaustClimb()
    {
        climbLocked = true;

        isClimbing = false;
        playerMovement.isClimbing = false;

        playerMovement.ClearMovementVelocity();

        playerMovement.SetGravity(
            playerMovement.NormalGravity
        );
    }

    private void TrackClimbDistance()
    {
        float heightDifference =
            transform.position.y -
            previousClimbHeight;

        if (heightDifference > 0f)
        {
            climbDistanceUsed +=
                heightDifference;
        }

        previousClimbHeight =
            transform.position.y;
    }

    public Vector3 GetClimbVelocity()
    {
        RaycastHit currentWallHit;

        if (wallInFront)
        {
            currentWallHit = wallHit;
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
            wallSideDirection =
                -wallSideDirection;
        }

        Vector3 sideVelocity =
            wallSideDirection *
            localInput.MoveInput.x *
            climbSideSpeed;

        Vector3 climbVelocity =
            Vector3.up * climbSpeed;

        Vector3 wallVelocity =
            -currentWallHit.normal *
            wallPullSpeed;

        if (!wallInFront &&
            lowerWallInFront)
        {
            wallVelocity =
                orientation.forward *
                topOutForwardSpeed;
        }

        return
            climbVelocity +
            sideVelocity +
            wallVelocity;
    }
}