using UnityEngine;

namespace _Project.Menu.Scripts
{
    [CreateAssetMenu(fileName = "MenuSettings", menuName = "Menu/Menu Settings")]
    public class MenuSettings : ScriptableObject
    {
        public float mouseSensitivity = 5f;
        public float gamepadSensitivity = 45f;
        
        public void SetMouseSensitivity(float sensitivity) => mouseSensitivity = sensitivity;
        public void SetGamepadSensitivity(float sensitivity) => gamepadSensitivity = sensitivity;
    }
}
