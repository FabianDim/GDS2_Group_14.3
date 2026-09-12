using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
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
            LockCursor(true);
            EventBus.Subscribe<BuyZoneEnteredEvent>(OnBuyZoneEntered);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (!HasInputAuthority) return;
            EventBus.Unsubscribe<BuyZoneEnteredEvent>(OnBuyZoneEntered);
        }

        public override void FixedUpdateNetwork()
        {
            // Look input stays ungated: in host mode the host simulates every
            // player's look rotation, including remote players.
            if (GetInput(out GameplayInput input))
                ProcessInput(input);

            // RefreshCamera was removed here - it only matters for the local
            // player and already runs in LateUpdate (input-authority gated),
            // so per-tick camera work for remote players is skipped.
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