using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using TMPro;
using UnityEngine;

namespace _Experimenation.K.Game_Manager.Scripts
{
    public class PointManager : MonoBehaviour
    {
        private TextMeshProUGUI _pointText;
        private int _points;

        private void Awake()
        {
            _pointText = GetComponentInChildren<TextMeshProUGUI>();
            _pointText.SetText("Points: " + _points);
            EventBus.Subscribe<TokenCollectedEvent>(OnTokenCollected);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<TokenCollectedEvent>(OnTokenCollected);
        }
        
        private void OnTokenCollected(TokenCollectedEvent ev)
        {
            _points += ev.Points;
            _pointText.SetText("Points: " + _points);
        }
    }
}
