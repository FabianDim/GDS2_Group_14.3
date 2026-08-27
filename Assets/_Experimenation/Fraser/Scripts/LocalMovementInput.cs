using UnityEngine;
using UnityEngine.InputSystem;

public class LocalMovementInput : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool CrouchHeld { get; private set; }
    public bool UsingGamepadLook { get; private set; }

    private bool jumpPressed;
    private bool crouchPressed;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crouchAction;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Tools"];
        jumpAction = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];
        crouchAction = playerInput.actions["Crouch"];
    }

    private void Update()
    {
        MoveInput = moveAction.ReadValue<Vector2>();
        LookInput = lookAction.ReadValue<Vector2>();

        SprintHeld = sprintAction.IsPressed();
        CrouchHeld = crouchAction.IsPressed();

        UsingGamepadLook =
            lookAction.activeControl?.device is Gamepad;

        if (jumpAction.WasPressedThisFrame())
        {
            jumpPressed = true;
        }

        if (crouchAction.WasPressedThisFrame())
        {
            crouchPressed = true;
        }
    }

    public bool ConsumeJump()
    {
        if (!jumpPressed)
        {
            return false;
        }

        jumpPressed = false;
        return true;
    }

    public bool ConsumeCrouch()
    {
        if (!crouchPressed)
        {
            return false;
        }

        crouchPressed = false;
        return true;
    }
}