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
            
            EventBus.Subscribe<PointChangeEvent>(OnPointsChanged);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<PointChangeEvent>(OnPointsChanged);
        }
        
        private void OnPointsChanged(PointChangeEvent ev)
        {
            _points += ev.points;
            _pointText.SetText("Points: " + _points);
        }
    }
}
