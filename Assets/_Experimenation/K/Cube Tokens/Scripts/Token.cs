using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Game_Manager.Abilities.Scripts;
using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using UnityEngine;

namespace _Experimenation.K.Cube_Tokens.Scripts
{
    public class Token : NetworkBehaviour
    {
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private int tokenValue = 10;

        private bool _collected;
        private TickTimer _despawnTimer;

        public override void Spawned()
        {
            // Spawned is called again when a pooled instance is acquired.
            _collected = false;
        }

        private void Update()
        {
            transform.Rotate(
                Vector3.up * (rotationSpeed * Time.deltaTime)
            );
        }

        public override void FixedUpdateNetwork()
        {
            // Keep the token alive for one network tick after the collection RPC
            // is sent. This prevents despawning/pooling the RPC source too early.
            if (!HasStateAuthority || !_collected || !_despawnTimer.Expired(Runner))
                return;

            Runner.Despawn(Object);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_collected)
                return;

            var player = other.GetComponentInParent<Player>();
            if (player == null)
                return;

            if (HasStateAuthority)
            {
                Collect(player);
            }
            else if (player.HasInputAuthority)
            {
                // The client reports its local contact; the state authority validates it.
                RPC_RequestCollect(player.Object);
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        private void RPC_RequestCollect(NetworkObject playerObject)
        {
            if (!HasStateAuthority || _collected || playerObject == null)
                return;

            var player = playerObject.GetComponent<Player>();
            if (player == null)
                return;

            Collect(player);
        }

        private void Collect(Player player)
        {
            if (_collected)
                return;

            _collected = true;

            var collector = player.Object.InputAuthority;

            FindFirstObjectByType<AbilityRoundState>()
                ?.RegisterTokenCollection(collector, player.Role);

            // Raise the local score event only on the collector's peer. The host
            // must not award a client collection to its own local UI.
            if (collector == Runner.LocalPlayer)
            {
                EventBus.Raise(
                    new TokenCollectedEvent(
                        tokenValue,
                        player.Role
                    )
                );
            }

            // Notify every peer for client-side feedback (UI, sound, VFX, etc.).
            RPC_TokenCollected(collector, player.Role);
            _despawnTimer = TickTimer.CreateFromTicks(Runner, 1);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_TokenCollected(PlayerRef collector, PlayerRole collectedBy)
        {
            // The collector's peer already raised the event locally when it was
            // the state authority (for example, the host collecting a token).
            if (collector != Runner.LocalPlayer || HasStateAuthority)
                return;

            EventBus.Raise(
                new TokenCollectedEvent(tokenValue, collectedBy)
            );
        }
    }
}
