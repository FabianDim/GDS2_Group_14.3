using UnityEngine;

namespace _Experimenation.Fraser.Scripts
{
    public class MoveCamera : MonoBehaviour
    {
        [SerializeField] private Transform cameraPosition;

        private void LateUpdate()
        {
            transform.position =
                cameraPosition.position;
        }
    }
}