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
        private GameData _gameData;

        // Resolved lazily: Awake can run before GameData.Spawned() assigns the
        // singleton, so a field initializer would cache a null reference.
        private GameData ResolveGameData()
        {
            if (_gameData == null)
                _gameData = GameData.Instance;
            return _gameData;
        }

        private void Awake()
        {
            _pointText = GetComponentInChildren<TextMeshProUGUI>();

            var gameData = ResolveGameData();
            if (gameData != null)
                _points = HasStateAuthority ? gameData.P1Data.Points : gameData.P2Data.Points;

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

            var gameData = ResolveGameData();
            if (gameData == null) return;

            if(HasStateAuthority) gameData.P1Data.Points = _points;
            else gameData.P2Data.Points = _points;
        }
    }
}
