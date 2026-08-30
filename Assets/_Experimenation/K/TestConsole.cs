using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Multiplayer.Scripts;
using Fusion;

namespace _Experimenation.K
{
    public class TestConsole : NetworkBehaviour
    {
        [Networked] private NetworkButtons PreviousButton { get; set; }
        
        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || !GetInput(out GameplayInput input)) return;
            if(input.Buttons.WasPressed(PreviousButton, InputButton.StartRunPhase)) 
                StartRunPhase();
            
            PreviousButton = input.Buttons;
        }
        
        private void StartRunPhase()
        {
            EventBus.Raise(new RunPhaseStartsEvent());
        }
    }
}
