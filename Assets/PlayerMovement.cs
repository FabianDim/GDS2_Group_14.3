using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
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

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundDistance = 0.2f;
    [SerializeField] private float playerHeight = 2f;

    public bool isGrounded { get; private set; }
    public bool isCrouching { get; set; }
    public bool isSliding { get; set; }

    private float horizontalMovement;
    private float verticalMovement;

    private Vector3 moveDirection;
    private Vector3 slopeMoveDirection;

    private Rigidbody rb;
    private RaycastHit slopeHit;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];

        rb.freezeRotation = true;

        moveSpeed = walkSpeed;
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundDistance,
            groundMask
        );

        MyInput();
        ControlDrag();
        ControlSpeed();

        if (jumpAction.WasPressedThisFrame() && isGrounded)
        {
            Jump();
        }

        if (OnSlope())
        {
            slopeMoveDirection = Vector3.ProjectOnPlane(
                moveDirection,
                slopeHit.normal
            );
        }
        else
        {
            slopeMoveDirection = moveDirection;
        }
    }

    private void FixedUpdate()
    {
        if (!isSliding)
        {
            MovePlayer();
        }

        ApplyGravity();
    }

    private void MyInput()
    {
        Vector2 movementInput = moveAction.ReadValue<Vector2>();

        horizontalMovement = movementInput.x;
        verticalMovement = movementInput.y;

        moveDirection =
            orientation.forward * verticalMovement +
            orientation.right * horizontalMovement;
    }

    private void MovePlayer()
    {
        if (isGrounded && OnSlope())
        {
            rb.AddForce(
                slopeMoveDirection.normalized *
                moveSpeed *
                movementMultiplier,
                ForceMode.Acceleration
            );
        }
        else if (isGrounded)
        {
            rb.AddForce(
                moveDirection.normalized *
                moveSpeed *
                movementMultiplier,
                ForceMode.Acceleration
            );
        }
        else
        {
            Vector3 horizontalVelocity = new Vector3(
                rb.linearVelocity.x,
                0f,
                rb.linearVelocity.z
            );

            if (horizontalVelocity.magnitude < moveSpeed)
            {
                rb.AddForce(
                    moveDirection.normalized *
                    moveSpeed *
                    movementMultiplier *
                    airMultiplier,
                    ForceMode.Acceleration
                );
            }
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        rb.AddForce(
            transform.up * jumpForce,
            ForceMode.Impulse
        );
    }

    private void ApplyGravity()
    {
        if (!isGrounded && rb.useGravity)
        {
            rb.AddForce(
                Physics.gravity * (gravityMultiplier - 1f),
                ForceMode.Acceleration
            );
        }
    }

    private void ControlSpeed()
    {
        float targetSpeed;

        if (isCrouching && !isSliding)
        {
            targetSpeed = crouchSpeed;
        }
        else if (sprintAction.IsPressed() && isGrounded)
        {
            targetSpeed = sprintSpeed;
        }
        else if (isGrounded)
        {
            targetSpeed = walkSpeed;
        }
        else
        {
            targetSpeed = moveSpeed;
        }

        moveSpeed = Mathf.Lerp(
            moveSpeed,
            targetSpeed,
            acceleration * Time.deltaTime
        );
    }

    private void ControlDrag()
    {
        if (isSliding)
        {
            rb.linearDamping = slideDrag;
        }
        else if (isGrounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = airDrag;
        }
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(
                transform.position,
                Vector3.down,
                out slopeHit,
                playerHeight / 2f + 0.5f,
                groundMask))
        {
            return Vector3.Angle(
                slopeHit.normal,
                Vector3.up
            ) > 0.1f;
        }

        return false;
    }
}