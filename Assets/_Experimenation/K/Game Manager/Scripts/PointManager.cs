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
            _pointText = 
                GameObject.FindGameObjectWithTag("Points").GetComponent<TextMeshProUGUI>();
            
            EventBus.Subscribe<TokenCollectedEvent>(OnPointsChanged);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<TokenCollectedEvent>(OnPointsChanged);
        }
        
        private void OnPointsChanged(TokenCollectedEvent ev)
        {
            _points += ev.points;
            _pointText.SetText("Points: " + _points);
        }
    }
}
