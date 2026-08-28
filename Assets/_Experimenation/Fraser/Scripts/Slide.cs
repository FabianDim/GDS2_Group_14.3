using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

public class Slide : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform cameraPosition;

    [Header("Crouching")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchCameraHeight = 0.4f;
    [SerializeField] private float cameraCrouchSpeed = 15f;

    [Header("Sliding")]
    [SerializeField] private float slideForce = 6f;
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float minimumSlideSpeed = 10f;

    [Header("Standing")]
    [SerializeField] private LayerMask obstacleMask;

    public bool isCrouching { get; private set; }
    public bool isSliding { get; private set; }

    private float standingCameraHeight;
    private float slideTimer;

    private SimpleKCC kcc;
    private PlayerMovement playerMovement;
    private LocalMovementInput localInput;

    public override void Spawned()
    {
        kcc = GetComponent<SimpleKCC>();
        playerMovement = GetComponent<PlayerMovement>();
        localInput = GetComponent<LocalMovementInput>();

        standingCameraHeight =
            cameraPosition.localPosition.y;

        kcc.SetHeight(standingHeight);
    }

    public override void FixedUpdateNetwork()
    {
        if (kcc == null ||
            playerMovement == null ||
            localInput == null)
        {
            return;
        }

        if (localInput.ConsumeCrouch())
        {
            StartCrouch();

            if (CanSlide())
            {
                StartSlide();
            }
        }

        if (!localInput.CrouchHeld)
        {
            StopSlide();
            TryStopCrouch();
        }

        if (isSliding)
        {
            slideTimer -= Runner.DeltaTime;

            if (slideTimer <= 0f)
            {
                StopSlide();
            }
        }
    }

    private void LateUpdate()
    {
        UpdateCameraHeight();
    }

    private void StartCrouch()
    {
        if (isCrouching)
        {
            return;
        }

        isCrouching = true;
        playerMovement.isCrouching = true;

        kcc.SetHeight(crouchHeight);
    }

    private void TryStopCrouch()
    {
        if (!isCrouching)
        {
            return;
        }

        if (!CanStand())
        {
            return;
        }

        isCrouching = false;
        playerMovement.isCrouching = false;

        kcc.SetHeight(standingHeight);
    }

    private void StartSlide()
    {
        if (isSliding)
        {
            return;
        }

        isSliding = true;
        playerMovement.isSliding = true;

        slideTimer = slideDuration;

        Vector3 slideDirection =
            orientation.forward;

        playerMovement.AddSlideImpulse(
            slideDirection,
            slideForce
        );
    }

    private void StopSlide()
    {
        if (!isSliding)
        {
            return;
        }

        isSliding = false;
        playerMovement.isSliding = false;
    }

    public void StopSlideFromJump()
    {
        StopSlide();
    }

    private bool CanSlide()
    {
        return
            playerMovement.isGrounded &&
            playerMovement.HorizontalSpeed >=
            minimumSlideSpeed;
    }

    private bool CanStand()
    {
        float extraHeight =
            standingHeight - crouchHeight;

        Vector3 origin =
            transform.position +
            Vector3.up * (crouchHeight * 0.5f);

        return !Physics.Raycast(
            origin,
            Vector3.up,
            extraHeight,
            obstacleMask,
            QueryTriggerInteraction.Ignore
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

        Vector3 cameraLocalPosition =
            cameraPosition.localPosition;

        cameraLocalPosition.y = Mathf.Lerp(
            cameraLocalPosition.y,
            targetHeight,
            cameraCrouchSpeed * Time.deltaTime
        );

        cameraPosition.localPosition =
            cameraLocalPosition;
    }
}