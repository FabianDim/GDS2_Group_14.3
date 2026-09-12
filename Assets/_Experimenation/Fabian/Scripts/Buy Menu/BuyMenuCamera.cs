using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using UnityEngine;

namespace _Experimenation.Fabian.Scripts.Buy_Menu
{
    public class BuyMenuCamera : MonoBehaviour
    {
        private void Awake()
        {
            EventBus.Subscribe<BuyZoneEnteredEvent>(OnBuyZoneEnetered);
            gameObject.SetActive(false);
        }
        
        private void OnDestroy() => EventBus.Unsubscribe<BuyZoneEnteredEvent>(OnBuyZoneEnetered);
        
        private void OnBuyZoneEnetered(BuyZoneEnteredEvent ev) => gameObject.SetActive(ev.Entered);
    }
}
