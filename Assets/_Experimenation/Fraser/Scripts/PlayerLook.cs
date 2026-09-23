using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

namespace _Experimenation.Fraser.Scripts
{
    public class PlayerLook : NetworkBehaviour, IGameplayInputConsumer
    {
        [Header("References")]
        [SerializeField] private Transform cam;
        [SerializeField] private Transform orientation;

        private WallRun _wallRun;
        private SimpleKCC _kcc;
        private float _pitch;
        private float _yaw;

        public override void Spawned()
        {
            _kcc = GetComponent<SimpleKCC>();
            _wallRun = GetComponent<WallRun>();

            if (!HasInputAuthority) return;
            LockCursor(true);
            EventBus.Subscribe<BuyZoneEnteredEvent>(OnBuyZoneEntered);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (!HasInputAuthority) return;
            EventBus.Unsubscribe<BuyZoneEnteredEvent>(OnBuyZoneEntered);
        }

        public void ProcessInput(GameplayInput input, NetworkButtons previousButtons)
        {
            _kcc?.AddLookRotation(input.LookRotationDelta);
        }


        private void LateUpdate()
        {
            if (!HasInputAuthority) return;
            RefreshCamera();
        }

        private void RefreshCamera()
        {
            var lookRotation = _kcc.GetLookRotation(true, false);
            var tilt = _wallRun != null ? _wallRun.CameraTilt : 0f;

            cam.localRotation = Quaternion.Euler(
                lookRotation.x,
                lookRotation.y,
                tilt
            );
        }

        private static void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
        
        private void OnBuyZoneEntered(BuyZoneEnteredEvent ev)
        {
            cam.gameObject.SetActive(!ev.Entered);
            LockCursor(!ev.Entered);
        }
    }
}