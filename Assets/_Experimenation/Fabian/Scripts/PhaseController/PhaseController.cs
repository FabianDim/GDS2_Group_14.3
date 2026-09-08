using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using Fusion;
using static _Experimenation.K.Game_Manager.Scripts.GameManager;

namespace _Experimenation.Fabian.Scripts
{
    public class PhaseController : NetworkBehaviour
    {
        private float buyPhaseDuration = 10f;

        [Networked]
        private GamePhase CurrentPhase { get; set; }

        private TickTimer _buyPhaseTimer;

        public override void Spawned()
        {
            if (HasStateAuthority)
                CurrentPhase = GamePhase.GAMESTART;
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority)
                return;

            if (CurrentPhase == GamePhase.GAMESTART)
            {
                CurrentPhase = GamePhase.BUYPHASE;
                _buyPhaseTimer = TickTimer.CreateFromSeconds(Runner, buyPhaseDuration);
                EventBus.Raise(new BuyPhaseStartEvent());
                return;
            }

            if (CurrentPhase == GamePhase.BUYPHASE &&
                _buyPhaseTimer.IsRunning &&
                _buyPhaseTimer.Expired(Runner))
            {
                CurrentPhase = GamePhase.RUNPHASE;
                EventBus.Raise(new RunPhaseStartsEvent());
            }
        }
    }
}
