using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using Fusion;
using TMPro;
using UnityEngine;

namespace _Experimenation.K.Game_Manager.Scripts
{
    /// <summary>
    /// Displays the replicated round timer and raises the timeout event once on
    /// State Authority. Gameplay systems must react to that authoritative event,
    /// not to a locally-expired UI timer.
    /// </summary>
    public class TimeManager : NetworkBehaviour
    {
        [Networked] private TickTimer Timer { get; set; }

        private TextMeshProUGUI _timeText;
        private bool _timerInitialized;
        private bool _subscribed;
        private int _lastDisplayedSecond = -1;

        public override void Spawned()
        {
            _timeText = GetComponentInChildren<TextMeshProUGUI>();

            if (!HasStateAuthority)
                return;

            EventBus.Subscribe<AllPlayersSpawnedEvent>(OnAllPlayersSpawned);
            _subscribed = true;
            TryInitializeTimer();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (_subscribed)
            {
                EventBus.Unsubscribe<AllPlayersSpawnedEvent>(OnAllPlayersSpawned);
                _subscribed = false;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority)
                return;

            // This fallback is safe for inactive Unity hierarchies because it does
            // not rely on StartCoroutine or Update being dispatched by Unity.
            TryInitializeTimer();

            if (!_timerInitialized || !TimerExpired())
                return;

            Timer = default;
            _timerInitialized = false;
            EventBus.Raise(new TimeRunsOutEvent());
        }

        private void OnAllPlayersSpawned(AllPlayersSpawnedEvent ev)
        {
            TryInitializeTimer();
        }

        private void TryInitializeTimer()
        {
            if (_timerInitialized || GameData.Instance == null || Runner == null ||
                !Runner.IsRunning || Runner.IsShutdown)
            {
                return;
            }

            Timer = TickTimer.CreateFromSeconds(
                Runner,
                Mathf.Max(0f, GameData.Instance.roundDuration * 0.75f));
            _timerInitialized = true;
        }

        private bool TimerExpired()
        {
            return Timer.IsRunning && Timer.Expired(Runner);
        }

        private void Update()
        {
            if (_timeText == null || Runner == null || !Runner.IsRunning)
                return;

            var remaining = Timer.IsRunning
                ? Mathf.Max(0, Mathf.CeilToInt((float)(Timer.RemainingTime(Runner) ?? 0d)))
                : 0;

            if (remaining == _lastDisplayedSecond)
                return;

            _lastDisplayedSecond = remaining;
            _timeText.SetText($"{remaining / 60}:{remaining % 60:00}");
        }
    }
}
