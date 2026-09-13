using _Experimenation.Fabian.Scripts.PhaseController;
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
        private bool _runPhaseStarted;
        private bool _runPhaseStartRequested;
        private bool _subscribed;
        private int _lastDisplayedSecond = -1;

        public override void Spawned()
        {
            _timeText = GetComponentInChildren<TextMeshProUGUI>();

            if (!HasStateAuthority)
                return;

            EventBus.Subscribe<RunPhaseStartsEvent>(OnRunPhaseStarts);
            _subscribed = true;

            // Spawned can happen after the phase changed during a scene reload.
            // Start immediately in that case instead of waiting for a missed event.
            var phaseManager = GetComponentInParent<PhaseManager>();
            if (phaseManager != null && phaseManager.IsRunPhase)
            {
                _runPhaseStartRequested = true;
                StartRunTimer();
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (!_subscribed) return;
            EventBus.Unsubscribe<RunPhaseStartsEvent>(OnRunPhaseStarts);
            _subscribed = false;
        }

        public override void FixedUpdateNetwork()
        {
            if (!_runPhaseStarted)
            {
                var phaseManager = GetComponentInParent<PhaseManager>();
                if (phaseManager != null && phaseManager.IsRunPhase)
                    _runPhaseStartRequested = true;
            }

            // GameData may be assigned after the phase event during a scene reload.
            // Retry from the network tick until the authoritative timer can start.
            if (_runPhaseStartRequested && !_timerInitialized)
                StartRunTimer();

            if (!_runPhaseStarted || !_timerInitialized || !TimerExpired())
                return;

            Timer = default;
            _timerInitialized = false;
            EventBus.Raise(new TimeRunsOutEvent());
        }

        private void OnRunPhaseStarts(RunPhaseStartsEvent ev)
        {
            _runPhaseStartRequested = true;
            StartRunTimer();
        }

        private void StartRunTimer()
        {
            if (_runPhaseStarted || GameData.Instance == null || Runner == null ||
                !Runner.IsRunning || Runner.IsShutdown)
            {
                return;
            }

            _runPhaseStarted = true;
            _runPhaseStartRequested = false;
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
