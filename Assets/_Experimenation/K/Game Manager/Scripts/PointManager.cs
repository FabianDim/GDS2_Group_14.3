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
        private GameData _gameData;
        private Coroutine _initialization;
        private bool _subscribed;

        public override void Spawned()
        {
            _pointText = GetComponentInChildren<TextMeshProUGUI>();
            EventBus.Subscribe<TokenCollectedEvent>(OnTokenCollected);
            _subscribed = true;
            _initialization = StartCoroutine(InitializeWhenReady());
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (_initialization != null)
                StopCoroutine(_initialization);

            if (!_subscribed) return;
            EventBus.Unsubscribe<TokenCollectedEvent>(OnTokenCollected);
            _subscribed = false;
        }

        private System.Collections.IEnumerator InitializeWhenReady()
        {
            while (GameData.Instance == null)
                yield return null;

            _gameData = GameData.Instance;
            SetDisplayedPoints(GetLocalPlayerPoints());
            _initialization = null;
        }

        private int GetLocalPlayerPoints()
        {
            if (_gameData == null)
                return _points;

            // GameManager deterministically maps host to P1 and client to P2.
            return HasStateAuthority ? _gameData.P1Data.Points : _gameData.P2Data.Points;
        }

        private void OnTokenCollected(TokenCollectedEvent ev)
        {
            if (Runner == null || ev.Collector != Runner.LocalPlayer)
                return;

            // The host has already applied the authoritative value before sending
            // the event. On the client, the event provides immediate UI feedback;
            // replicated GameData remains the source of truth after initialization.
            SetDisplayedPoints(_points + ev.Points);
        }

        private void SetDisplayedPoints(int points)
        {
            _points = points;
            if (_pointText != null)
                _pointText.SetText($"Points: {_points}");
        }
    }
}
