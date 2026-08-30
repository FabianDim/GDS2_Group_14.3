using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using TMPro;
using UnityEngine;

namespace _Experimenation.K.Game_Manager.Scripts
{
    public class RoundOver : MonoBehaviour
    {
        private GameObject _screen;
        private TextMeshProUGUI _text;

        private void Awake()
        {
            _screen = transform.GetChild(0).gameObject;
            _text = _screen.GetComponentInChildren<TextMeshProUGUI>();
            
            EventBus.Subscribe<RoundOverEvent>(OnRoundOver);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<RoundOverEvent>(OnRoundOver);
        }

        private void OnRoundOver(RoundOverEvent ev)
        {
            _text.SetText(ev.RunnerWins ? "Runner wins!" : "Chaser caught Runner!");
            _screen.SetActive(true);
        }
    }
}
