using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Multiplayer.Scripts;
using UnityEngine;

namespace _Experimenation.Fabian.Scripts.Buy_Menu
{
    public class BuyZone : MonoBehaviour
    {
        private bool _shopOpen;

        private void OnEnable() => EventBus.Subscribe<RunPhaseStartsEvent>(OnRunPhaseStarts);

        private void OnDisable()
        {
            EventBus.Unsubscribe<RunPhaseStartsEvent>(OnRunPhaseStarts);
            // PhaseManager disables the buy-phase objects before raising its event.
            if (_shopOpen)
                ExitShop();
        }

        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponentInParent<Player>();
            if (player == null || player.Object == null || !player.Object.IsValid ||
                !player.HasInputAuthority)
                return;

            _shopOpen = true;
            EventBus.Raise(new BuyZoneEnteredEvent(true));
        }

        public void ExitShop()
        {
            _shopOpen = false;
            EventBus.Raise(new BuyZoneEnteredEvent(false));
        }

        private void OnRunPhaseStarts(RunPhaseStartsEvent ev) => ExitShop();
    }
}
