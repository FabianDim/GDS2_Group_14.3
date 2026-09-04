using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using Fusion;
using TMPro;

namespace _Experimenation.K.Game_Manager.Scripts
{
    public class PointManager : NetworkBehaviour
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
            if (!ev.CollectedBy.HasInputAuthority) return;
            _points += ev.Points;
            _pointText.SetText("Points: " + _points);
        }
    }
}
