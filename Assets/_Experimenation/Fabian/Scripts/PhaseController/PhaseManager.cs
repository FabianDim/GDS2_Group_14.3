using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Game_Manager.Scripts;
using Fusion;
using UnityEngine;

namespace _Experimenation.Fabian.Scripts.PhaseController
{
    
    public class PhaseManager : NetworkBehaviour
    {
        [SerializeField] private GameObject buyPhaseItems;
        [SerializeField] private ParticleSystem buyPhaseEndsPS;
        [SerializeField] private GameObject runPhaseItems;

        [OnChangedRender(nameof(OnRunPhaseChanged))]
        [Networked] private NetworkBool IsRunPhase { get; set; }

        private TickTimer _timer;
        private bool _timerInitialized;

        public override void Spawned()
        {
            ApplyPhaseVisuals();

            if (!HasStateAuthority)
                return;

            IsRunPhase = false;
            StartCoroutine(InitTimerWhenGameDataReady());
        }

        // GameData.Instance is assigned in GameData.Spawned(), which may run
        // after this component's Spawned() - wait for it instead of NRE-ing
        // and leaving _timer at its default (never expires).
        private System.Collections.IEnumerator InitTimerWhenGameDataReady()
        {
            while (GameData.Instance == null)
                yield return null;

            _timer = TickTimer.CreateFromSeconds(Runner, GameData.Instance.roundDuration * 0.25f);
            _timerInitialized = true;
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || !_timerInitialized || !_timer.Expired(Runner))
                return;

            _timer = TickTimer.None;
            _timerInitialized = false;
            IsRunPhase = true;
        }

        private void OnRunPhaseChanged()
        {
            ApplyPhaseVisuals();
            EventBus.Raise(new RunPhaseStartsEvent());
        }

        private void ApplyPhaseVisuals()
        {
            var isRunPhase = IsRunPhase;

            if (runPhaseItems != null)
                runPhaseItems.SetActive(isRunPhase);
            else
                Debug.LogError("PhaseManager: Run Phase UI reference is not assigned.", this);

            if (buyPhaseItems != null)
                buyPhaseItems.SetActive(!isRunPhase);
            else
                Debug.LogError("PhaseManager: Buy Phase UI reference is not assigned.", this);

            if (isRunPhase && buyPhaseEndsPS != null)
                buyPhaseEndsPS.Play();
            else if (isRunPhase)
                Debug.LogError("PhaseManager: buy phase ending particle reference is not assigned.", this);
        }
    }
}
