using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using UnityEngine;

namespace _Experimenation.K.Game_Manager.Scripts
{
    public class RPCHandler : NetworkBehaviour
    {
        public static RPCHandler Instance { get; private set; }

        public override void Spawned()
        {
            if (Instance && Instance != this)
            {
                Runner.Despawn(Object);
                return;
            }
            Instance = this;
            Runner.MakeDontDestroyOnLoad(gameObject);
        }
        
        public void GetCollected(Collider other, int tokenValue, NetworkObject obj)
        {
            if (!HasStateAuthority) return;
            var collector = other.GetComponentInParent<Player>();
            if(collector)
            {
                RPC_GetCollected(collector, tokenValue);
                Runner.Despawn(obj);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_GetCollected(Player collector, int tokenValue) =>
            EventBus.Raise(new TokenCollectedEvent(tokenValue, collector, collector.Object.InputAuthority));
    }
}
