using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Multiplayer.Scripts;
using Fusion;

namespace _Experimenation.K
{
    public class TestConsole : NetworkBehaviour
    {
        public override void FixedUpdateNetwork()
        {
            if (!GetInput(out GameplayInput input)) return;
            if(input.StartRunPhase) StartRunPhase();
        }
        
        private void StartRunPhase()
        {
            EventBus.Raise(new RunPhaseStartsEvent());
        }
    }
}
