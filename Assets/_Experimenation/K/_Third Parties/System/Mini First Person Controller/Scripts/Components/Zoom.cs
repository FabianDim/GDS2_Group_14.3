using UnityEngine;

namespace _Experimenation.K._Third_Parties.System.Mini_First_Person_Controller.Scripts.Components
{
    [ExecuteInEditMode]
    public class Zoom : MonoBehaviour
    {
        Camera camera;
        public float defaultFOV = 60;
        public float maxZoomFOV = 15;
        [Range(0, 1)]
        public float currentZoom;
        public float sensitivity = 1;


        void Awake()
        {
            // Get the camera on this gameObject and the defaultZoom.
            camera = GetComponent<Camera>();
            if (camera)
            {
                defaultFOV = camera.fieldOfView;
            }
        }

        void Update()
        {
            // Update the currentZoom and the camera's fieldOfView.
            currentZoom += Input.mouseScrollDelta.y * sensitivity * .05f;
            currentZoom = Mathf.Clamp01(currentZoom);
            camera.fieldOfView = Mathf.Lerp(defaultFOV, maxZoomFOV, currentZoom);
        }
    }
}
