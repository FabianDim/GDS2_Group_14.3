using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using Fusion;

namespace _Experimenation.K
{
    public class TestConsole : NetworkBehaviour
    {
        public override void Spawned()
        {
            if (!HasStateAuthority) Destroy(gameObject);
        }
        
        public void StartRunPhase()
        {
            EventBus.Raise(new RunPhaseStartsEvent());
        }
    }
}
