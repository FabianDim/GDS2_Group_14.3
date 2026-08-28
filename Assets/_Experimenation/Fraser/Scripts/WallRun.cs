using _Experimenation.Fraser.Scripts;
using Fusion;
using UnityEngine;

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

    public bool isWallRunning { get; private set; }
    public float tilt { get; private set; }

    public float WallJumpVerticalForce
    {
        get
        {
            return wallRunJumpVerticalForce;
        }
    }

    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private RaycastHit currentWallHit;

    private Collider blockedWallCollider;

    private PlayerMovement playerMovement;

    private TickTimer wallJumpReattachTimer;

    public override void Spawned()
    {
        playerMovement =
            GetComponent<PlayerMovement>();
    }

    public void UpdateWallRunState()
    {
        if (playerMovement == null)
        {
            return;
        }

        if (playerMovement.IsGrounded)
        {
            blockedWallCollider = null;
        }

        CheckWall();

        grounded =
            playerMovement.IsGrounded;

        blockedByMovementState =
            playerMovement.IsSliding ||
            playerMovement.IsCrouching ||
            playerMovement.IsClimbing;

        if (playerMovement.IsClimbing)
        {
            StopWallRun();
            return;
        }

        if (!wallJumpReattachTimer
            .ExpiredOrNotRunning(Runner))
        {
            StopWallRun();
            return;
        }

        if (grounded ||
            blockedByMovementState)
        {
            StopWallRun();
            return;
        }

        bool canUseLeftWall =
            wallLeft &&
            leftWallHit.collider !=
            blockedWallCollider;

        bool canUseRightWall =
            wallRight &&
            rightWallHit.collider !=
            blockedWallCollider;

        sameWallBlocked =
            (wallLeft &&
             leftWallHit.collider ==
             blockedWallCollider) ||
            (wallRight &&
             rightWallHit.collider ==
             blockedWallCollider);

        if (canUseLeftWall)
        {
            currentWallHit =
                leftWallHit;

            StartWallRun();
        }
        else if (canUseRightWall)
        {
            currentWallHit =
                rightWallHit;

            StartWallRun();
        }
        else
        {
            StopWallRun();
        }
    }

    private void CheckWall()
    {
        PhysicsScene physicsScene =
            Runner.GetPhysicsScene();

        Vector3 checkPosition =
            transform.position +
            Vector3.up *
            wallCheckHeight;

        wallLeft =
            physicsScene.SphereCast(
                checkPosition,
                wallCheckRadius,
                -orientation.right,
                out leftWallHit,
                wallDistance,
                wallMask,
                QueryTriggerInteraction.Ignore
            );

        wallRight =
            physicsScene.SphereCast(
                checkPosition,
                wallCheckRadius,
                orientation.right,
                out rightWallHit,
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
        if (!isWallRunning)
        {
            isWallRunning = true;

            playerMovement
                .IsWallRunning = true;
        }

        playerMovement.SetGravity(
            wallRunGravity
        );
    }

    private void StopWallRun()
    {
        if (!isWallRunning)
        {
            return;
        }

        isWallRunning = false;

        playerMovement
            .IsWallRunning = false;

        if (!playerMovement.IsClimbing)
        {
            playerMovement.SetGravity(
                playerMovement.NormalGravity
            );
        }
    }

    public void StopWallRunFromJump()
    {
        if (currentWallHit.collider != null)
        {
            blockedWallCollider =
                currentWallHit.collider;
        }

        wallJumpReattachTimer =
            TickTimer.CreateFromSeconds(
                Runner,
                wallJumpReattachDelay
            );

        StopWallRun();
    }

    public void StopWallRunFromClimb()
    {
        if (!isWallRunning)
        {
            return;
        }

        isWallRunning = false;

        playerMovement
            .IsWallRunning = false;
    }

    public Vector3 GetWallRunDirection(
        Vector3 inputDirection
    )
    {
        Vector3 wallDirection =
            Vector3.ProjectOnPlane(
                inputDirection,
                currentWallHit.normal
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
        Vector3 velocityAlongWall =
            Vector3.ProjectOnPlane(
                currentVelocity,
                currentWallHit.normal
            );

        velocityAlongWall.y = 0f;

        return velocityAlongWall;
    }

    public Vector3
        GetWallJumpHorizontalImpulse()
    {
        Vector3 awayFromWall =
            currentWallHit.normal;

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

        if (isWallRunning)
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

        float targetTilt = 0f;

        if (isWallRunning)
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

        tilt =
            Mathf.Lerp(
                tilt,
                targetTilt,
                camTiltTime *
                Time.deltaTime
            );
    }
}