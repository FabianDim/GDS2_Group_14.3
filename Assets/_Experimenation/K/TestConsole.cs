using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using static _Experimenation.K.Game_Manager.Scripts.GameManager;

namespace _Experimenation.K
{
    public class TestConsole : NetworkBehaviour
    {
        // [Networked] private NetworkButtons PreviousButton { get; set; }

        // private GamePhase _gamePhase = GamePhase.GAMESTART;

        // public override void FixedUpdateNetwork()
        // {
        //     if (!HasStateAuthority || !GetInput(out GameplayInput input)) return;

        //     if (_gamePhase != GamePhase.RUNPHASE || _gamePhase != GamePhase.ROUNDCHANGE)
        //     {
        //         StartBuyPhase();
        //     }

        //     if (input.Buttons.WasPressed(PreviousButton, InputButton.StartRunPhase))
        //         StartRunPhase();

        //     PreviousButton = input.Buttons;
        // }

        // private void StartRunPhase()
        // {

        //     EventBus.Raise(new RunPhaseStartsEvent());
        // }

        // private void StartBuyPhase()
        // {
        //     EventBus.Raise(new BuyPhaseStartEvent());
        // }
    }
}
