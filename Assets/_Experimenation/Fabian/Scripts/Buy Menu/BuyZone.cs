using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Multiplayer.Scripts;
using UnityEngine;

namespace _Experimenation.Fabian.Scripts.Buy_Menu
{
    public class BuyZone : MonoBehaviour
    {
        private void Awake() => EventBus.Subscribe<RunPhaseStartsEvent>(OnRunPhaseStarts);
        private void OnDestroy() => EventBus.Unsubscribe<RunPhaseStartsEvent>(OnRunPhaseStarts);
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.GetComponentInParent<Player>()) return;
            EventBus.Raise(new BuyZoneEnteredEvent(true));
        }
        
        public void ExitShop() => 
            EventBus.Raise(new BuyZoneEnteredEvent(false));
        
        private void OnRunPhaseStarts(RunPhaseStartsEvent ev) => 
            EventBus.Raise(new BuyZoneEnteredEvent(false));
    }
}
