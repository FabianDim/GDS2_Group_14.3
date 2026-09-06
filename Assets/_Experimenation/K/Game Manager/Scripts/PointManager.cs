using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using Fusion;
using TMPro;
using UnityEngine;

namespace _Experimenation.K.Game_Manager.Scripts
{
    public class PointManager : NetworkBehaviour
    {
        private TextMeshProUGUI _pointText;
        private int _points;
        [SerializeField] private GameData gameData;

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
            if (!ev.CollectedBy.HasInputAuthority) return;
            _points += ev.Points;
            _pointText.SetText("Points: " + _points);
            
            if(HasStateAuthority) gameData.p1Points = _points;
            else gameData.p2Points = _points;
        }
    }
}
