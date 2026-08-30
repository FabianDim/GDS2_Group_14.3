using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

namespace _Experimenation.Fraser.Scripts
{
    public class PlayerLook : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cam;
        [SerializeField] private Transform orientation;

        private SimpleKCC _kcc;
        private float _pitch;
        private float _yaw;

        public override void Spawned()
        {
            _kcc = GetComponent<SimpleKCC>();
            if (!HasInputAuthority) return;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out GameplayInput input)) ProcessInput(input);
            RefreshCamera();
        }

        private void LateUpdate()
        {
            if (!HasInputAuthority) return;
            RefreshCamera();
        }

        private void ProcessInput(GameplayInput input) =>
            _kcc.AddLookRotation(input.LookRotationDelta);

        private void RefreshCamera()
        {
            var pitchRotation = _kcc.GetLookRotation(true, false);
            cam.localRotation = Quaternion.Euler(pitchRotation);
        }
    }
}