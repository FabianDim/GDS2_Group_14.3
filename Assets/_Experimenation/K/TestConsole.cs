using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using UnityEngine;

namespace _Experimenation.K
{
    public class TestConsole : NetworkBehaviour
    {
        public override void Spawned()
        {
            if (!HasStateAuthority) Destroy(gameObject);
        }

        public override void FixedUpdateNetwork()
        {
            Debug.Log("TestConsole: FixedUpdateNetwork");
            if (!GetInput<GameplayInput>(out var input)) return;
            Debug.Log($"TestConsole: {input.StartRunPhase}");
            if(input.StartRunPhase) StartRunPhase();
        }
        
        private void StartRunPhase()
        {
            EventBus.Raise(new RunPhaseStartsEvent());
        }
    }
}
