using UnityEngine;

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

    private LocalMovementInput localInput;

    private void Start()
    {
        localInput =
            GetComponent<LocalMovementInput>();

        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible = false;
    }

    private void Update()
    {
        if (localInput == null)
        {
            return;
        }

        Vector2 lookInput =
            localInput.LookInput;

        float sensitivityX;
        float sensitivityY;

        if (localInput.UsingGamepadLook)
        {
            sensitivityX =
                controllerSensitivityX *
                Time.deltaTime;

            sensitivityY =
                controllerSensitivityY *
                Time.deltaTime;
        }
        else
        {
            sensitivityX =
                mouseSensitivityX;

            sensitivityY =
                mouseSensitivityY;
        }

        yRotation +=
            lookInput.x * sensitivityX;

        xRotation -=
            lookInput.y * sensitivityY;

        xRotation = Mathf.Clamp(
            xRotation,
            -90f,
            90f
        );

        float cameraTilt = 0f;

        if (wallRun != null)
        {
            cameraTilt = wallRun.tilt;
        }

        cam.rotation = Quaternion.Euler(
            xRotation,
            yRotation,
            cameraTilt
        );

        orientation.rotation =
            Quaternion.Euler(
                0f,
                yRotation,
                0f
            );
    }
}