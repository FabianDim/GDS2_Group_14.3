using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

namespace _Experimenation.K.Cube_Tokens.Scripts
{
    public class Token : NetworkBehaviour
    {
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private int tokenValue = 10;

        private void Update() => 
            transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime));

        private void OnTriggerEnter(Collider other)
        {
            if (!HasStateAuthority) return;
            var collector = GetComponent<SimpleKCC>()?.GetComponent<Player>();
            if(collector) RPC_GetCollected(collector);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_GetCollected(Player collector)
        {
            if (collector.HasInputAuthority)
                EventBus.Raise(new TokenCollectedEvent(tokenValue, collector.Role));
            Runner.Despawn(Object);
        }
    }
}
