using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using UnityEngine;
using static _Experimenation.K.Game_Manager.Scripts.GameManager;

public class PhaseController : NetworkBehaviour
{
    [Networked] private NetworkButtons PreviousButton { get; set; }

    [Networked] private GamePhase CurrentPhase { get; set; }

    public override void Spawned()
    {
        CurrentPhase = GamePhase.GAMESTART;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        if (CurrentPhase == GamePhase.GAMESTART)
        {
            StartBuyPhase();
        }

        if (!GetInput(out GameplayInput input)) return;

        if (input.Buttons.WasPressed(PreviousButton, InputButton.StartRunPhase))
            StartRunPhase();

        PreviousButton = input.Buttons;
    }

    private void StartRunPhase()
    {

        EventBus.Raise(new RunPhaseStartsEvent());
    }

    private void StartBuyPhase()
    {
        CurrentPhase = GamePhase.BUYPHASE;
        EventBus.Raise(new BuyPhaseStartEvent());
    }
}
