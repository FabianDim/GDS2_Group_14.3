using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using UnityEngine;

namespace _Experimenation.Fabian.Scripts.Buy_Menu
{
    /// <summary>
    /// Controls the local buy-menu camera. The menu is visible when the match
    /// starts in the Buy Phase and is hidden when the Run Phase begins.
    /// </summary>
    public sealed class BuyMenuCamera : MonoBehaviour
    {
        private void Awake()
        {
            EventBus.Subscribe<BuyZoneEnteredEvent>(OnBuyZoneEntered);
            EventBus.Subscribe<RunPhaseStartsEvent>(OnRunPhaseStarts);

            // Do not disable this object in Awake. The previous implementation
            // disabled itself before the player had a chance to enter the game,
            // so the initial Buy Phase UI was never visible.
            SetVisible(true);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<BuyZoneEnteredEvent>(OnBuyZoneEntered);
            EventBus.Unsubscribe<RunPhaseStartsEvent>(OnRunPhaseStarts);
        }

        private void OnBuyZoneEntered(BuyZoneEnteredEvent ev)
        {
            if (ev == null)
                return;

            SetVisible(ev.Entered);
        }

        private void OnRunPhaseStarts(RunPhaseStartsEvent ev)
        {
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            if (gameObject.activeSelf != visible)
                gameObject.SetActive(visible);
        }
    }
}
