using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using UnityEngine;

namespace _Experimenation.K.Cube_Tokens.Scripts
{
    public class Token : NetworkBehaviour
    {
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private int tokenValue = 10;
        private TickTimer _timer;
        private bool _collected;

        private void Update() => 
            transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime));

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority && _collected && _timer.Expired(Runner))
                Runner.Despawn(Object);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!HasStateAuthority || _collected) return;

            var collector = other.GetComponentInParent<Player>();
            if (collector == null)
                return;

            _collected = true;
            _timer = TickTimer.CreateFromTicks(Runner, 10);
            RPC_GetCollected(collector.Object.InputAuthority, collector.Role);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_GetCollected(PlayerRef collector, PlayerRole role)
        {
            if (collector == Runner.LocalPlayer)
                EventBus.Raise(new TokenCollectedEvent(tokenValue, role));
        }
    }
}
