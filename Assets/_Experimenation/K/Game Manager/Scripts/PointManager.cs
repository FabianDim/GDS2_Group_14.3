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
            EventBus.Subscribe<AbilityPurchasedEvent>(OnAbilityPurchased);
            _subscribed = true;
            _initialization = StartCoroutine(InitializeWhenReady());
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (_initialization != null)
            {
                StopCoroutine(_initialization);
                _initialization = null;
            }

            if (!_subscribed)
                return;

            EventBus.Unsubscribe<TokenCollectedEvent>(OnTokenCollected);
            EventBus.Unsubscribe<AbilityPurchasedEvent>(OnAbilityPurchased);
            _subscribed = false;
        }

        private System.Collections.IEnumerator InitializeWhenReady()
        {
            while (GameData.Instance == null)
                yield return null;

            _gameData = GameData.Instance;
            RefreshDisplayedPoints();
            _initialization = null;
        }

        private void Update()
        {
            // GameData is authoritative. Reconcile this local display from the
            // replicated value so purchases and token rewards are both reflected.
            if (_gameData == null || Runner == null || !Runner.IsRunning)
                return;

            var networkedPoints = GetLocalPlayerPoints();
            if (networkedPoints != _points)
                SetDisplayedPoints(networkedPoints);
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

            RefreshDisplayedPoints();
        }

        private void OnAbilityPurchased(AbilityPurchasedEvent ev)
        {
            if (!ev.Accepted || Runner == null || ev.Buyer != Runner.LocalPlayer)
                return;

            // The purchase result is only a notification. GameData has already
            // deducted the price on State Authority; read that replicated value.
            RefreshDisplayedPoints();
        }

        private void RefreshDisplayedPoints()
        {
            if (_gameData != null)
                SetDisplayedPoints(GetLocalPlayerPoints());
        }

        private void SetDisplayedPoints(int points)
        {
            _points = points;
            if (_pointText != null)
                _pointText.SetText($"Points: {_points}");
        }
    }
}
