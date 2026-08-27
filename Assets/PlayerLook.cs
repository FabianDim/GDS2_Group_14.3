using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WallRun wallRun;
    [SerializeField] private Transform cam;
    [SerializeField] private Transform orientation;

    [Header("Sensitivity")]
    [SerializeField] private float mouseSensitivityX = 0.1f;
    [SerializeField] private float mouseSensitivityY = 0.1f;
    [SerializeField] private float controllerSensitivityX = 180f;
    [SerializeField] private float controllerSensitivityY = 180f;

    private float xRotation;
    private float yRotation;

    private PlayerInput playerInput;
    private InputAction lookAction;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError("PlayerLook requires a PlayerInput component on the same GameObject.");
            enabled = false;
            return;
        }

        lookAction = playerInput.actions.FindAction("Tools");

        if (lookAction == null)
        {
            Debug.LogError("Could not find input action: Tools");
            enabled = false;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        bool usingGamepad = lookAction.activeControl?.device is Gamepad;

        float sensitivityX;
        float sensitivityY;

        if (usingGamepad)
        {
            sensitivityX = controllerSensitivityX * Time.deltaTime;
            sensitivityY = controllerSensitivityY * Time.deltaTime;
        }
        else
        {
            sensitivityX = mouseSensitivityX;
            sensitivityY = mouseSensitivityY;
        }

        yRotation += lookInput.x * sensitivityX;
        xRotation -= lookInput.y * sensitivityY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cam.rotation = Quaternion.Euler(
            xRotation,
            yRotation,
            wallRun.tilt
        );

        orientation.rotation = Quaternion.Euler(
            0f,
            yRotation,
            0f
        );
    }
}