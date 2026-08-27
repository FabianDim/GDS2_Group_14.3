using UnityEngine;
using UnityEngine.InputSystem;

public class WallRun : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform orientation;

    [Header("Detection")]
    [SerializeField] private float wallDistance = 0.5f;
    [SerializeField] private float minimumJumpHeight = 1.5f;

    [Header("Wall Running")]
    [SerializeField] private float wallRunGravity;
    [SerializeField] private float wallRunJumpForce;

    [Header("Camera")]
    [SerializeField] private Camera cam;
    [SerializeField] private float fov;
    [SerializeField] private float wallRunfov;
    [SerializeField] private float wallRunfovTime;
    [SerializeField] private float camTilt;
    [SerializeField] private float camTiltTime;

    public float tilt { get; private set; }
    public bool isWallRunning { get; private set; }

    private bool wallLeft;
    private bool wallRight;

    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;

    private Rigidbody rb;
    private Climb climb;

    private PlayerInput playerInput;
    private InputAction jumpAction;

    private bool CanWallRun()
    {
        return !Physics.Raycast(
            transform.position,
            Vector3.down,
            minimumJumpHeight
        );
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        climb = GetComponent<Climb>();
        playerInput = GetComponent<PlayerInput>();

        jumpAction = playerInput.actions["Jump"];
    }

    private void CheckWall()
    {
        wallLeft = Physics.Raycast(
            transform.position,
            -orientation.right,
            out leftWallHit,
            wallDistance
        );

        wallRight = Physics.Raycast(
            transform.position,
            orientation.right,
            out rightWallHit,
            wallDistance
        );
    }

    private void Update()
    {
        CheckWall();

        if (climb != null && climb.isClimbing)
        {
            StopWallRun();
            return;
        }

        if (CanWallRun())
        {
            if (wallLeft)
            {
                StartWallRun();
            }
            else if (wallRight)
            {
                StartWallRun();
            }
            else
            {
                StopWallRun();
            }
        }
        else
        {
            StopWallRun();
        }
    }

    private void StartWallRun()
    {
        isWallRunning = true;

        rb.useGravity = false;

        rb.AddForce(
            Vector3.down * wallRunGravity,
            ForceMode.Force
        );

        cam.fieldOfView = Mathf.Lerp(
            cam.fieldOfView,
            wallRunfov,
            wallRunfovTime * Time.deltaTime
        );

        if (wallLeft)
        {
            tilt = Mathf.Lerp(
                tilt,
                -camTilt,
                camTiltTime * Time.deltaTime
            );
        }
        else if (wallRight)
        {
            tilt = Mathf.Lerp(
                tilt,
                camTilt,
                camTiltTime * Time.deltaTime
            );
        }

        if (jumpAction.WasPressedThisFrame())
        {
            if (wallLeft)
            {
                Vector3 wallRunJumpDirection =
                    transform.up + leftWallHit.normal;

                rb.linearVelocity = new Vector3(
                    rb.linearVelocity.x,
                    0f,
                    rb.linearVelocity.z
                );

                rb.AddForce(
                    wallRunJumpDirection *
                    wallRunJumpForce *
                    100f,
                    ForceMode.Force
                );
            }
            else if (wallRight)
            {
                Vector3 wallRunJumpDirection =
                    transform.up + rightWallHit.normal;

                rb.linearVelocity = new Vector3(
                    rb.linearVelocity.x,
                    0f,
                    rb.linearVelocity.z
                );

                rb.AddForce(
                    wallRunJumpDirection *
                    wallRunJumpForce *
                    100f,
                    ForceMode.Force
                );
            }
        }
    }

    private void StopWallRun()
    {
        isWallRunning = false;

        if (climb == null || !climb.isClimbing)
        {
            rb.useGravity = true;
        }

        cam.fieldOfView = Mathf.Lerp(
            cam.fieldOfView,
            fov,
            wallRunfovTime * Time.deltaTime
        );

        tilt = Mathf.Lerp(
            tilt,
            0f,
            camTiltTime * Time.deltaTime
        );
    }

    public void StopWallRunFromClimb()
    {
        isWallRunning = false;

        cam.fieldOfView = Mathf.Lerp(
            cam.fieldOfView,
            fov,
            wallRunfovTime * Time.deltaTime
        );

        tilt = Mathf.Lerp(
            tilt,
            0f,
            camTiltTime * Time.deltaTime
        );
    }
}