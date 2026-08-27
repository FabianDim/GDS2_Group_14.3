using UnityEngine;
using UnityEngine.InputSystem;

public class Slide : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform cameraPosition;

    [Header("Crouching")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchCameraHeight = 0.4f;
    [SerializeField] private float cameraCrouchSpeed = 15f;

    [Header("Sliding")]
    [SerializeField] private float slideForce = 6f;
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float minimumSlideSpeed = 3f;

    public bool isCrouching { get; private set; }
    public bool isSliding { get; private set; }

    private float standingHeight;
    private Vector3 standingCenter;
    private float standingCameraHeight;
    private float slideTimer;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private PlayerMovement playerMovement;
    private PlayerInput playerInput;
    private InputAction crouchAction;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        playerMovement = GetComponent<PlayerMovement>();
        playerInput = GetComponent<PlayerInput>();

        crouchAction = playerInput.actions["Crouch"];

        standingHeight = capsuleCollider.height;
        standingCenter = capsuleCollider.center;
        standingCameraHeight = cameraPosition.localPosition.y;
    }

    private void Update()
    {
        if (crouchAction.WasPressedThisFrame())
        {
            StartCrouch();

            if (CanSlide())
            {
                StartSlide();
            }
        }

        if (!crouchAction.IsPressed())
        {
            StopCrouch();
            StopSlide();
        }

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;

            if (slideTimer <= 0f)
            {
                StopSlide();
            }
        }

        UpdateCameraHeight();
    }

    private void FixedUpdate()
    {
        if (isSliding)
        {
            SlideMovement();
        }
    }

    private void StartCrouch()
    {
        isCrouching = true;
        playerMovement.isCrouching = true;

        capsuleCollider.height = crouchHeight;

        capsuleCollider.center = new Vector3(
            standingCenter.x,
            standingCenter.y - (standingHeight - crouchHeight) / 2f,
            standingCenter.z
        );
    }

    private void StopCrouch()
    {
        if (!CanStand())
        {
            return;
        }

        isCrouching = false;
        playerMovement.isCrouching = false;

        capsuleCollider.height = standingHeight;
        capsuleCollider.center = standingCenter;
    }

    private void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;

        playerMovement.isSliding = true;

        Vector3 horizontalVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        Vector3 slideDirection;

        if (horizontalVelocity.magnitude > 0.1f)
        {
            slideDirection = horizontalVelocity.normalized;
        }
        else
        {
            slideDirection = orientation.forward;
        }

        rb.AddForce(
            slideDirection * slideForce,
            ForceMode.Impulse
        );
    }

    private void StopSlide()
    {
        isSliding = false;
        playerMovement.isSliding = false;
    }

    private void SlideMovement()
    {
        rb.AddForce(
            Vector3.down * 5f,
            ForceMode.Acceleration
        );
    }

    private bool CanSlide()
    {
        Vector3 horizontalVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        return playerMovement.isGrounded &&
               horizontalVelocity.magnitude >= minimumSlideSpeed;
    }

    private bool CanStand()
    {
        float heightDifference = standingHeight - crouchHeight;

        return !Physics.Raycast(
            transform.position,
            Vector3.up,
            heightDifference + 0.1f
        );
    }

    private void UpdateCameraHeight()
    {
        float targetHeight;

        if (isCrouching)
        {
            targetHeight = crouchCameraHeight;
        }
        else
        {
            targetHeight = standingCameraHeight;
        }

        Vector3 cameraLocalPosition = cameraPosition.localPosition;

        cameraLocalPosition.y = Mathf.Lerp(
            cameraLocalPosition.y,
            targetHeight,
            cameraCrouchSpeed * Time.deltaTime
        );

        cameraPosition.localPosition = cameraLocalPosition;
    }
}