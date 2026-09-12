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
        private TickTimer _timer;

        public override void Spawned()
        {
            if(!HasStateAuthority) return;
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
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || !_timer.Expired(Runner)) return;

            _timer = TickTimer.None; // fire the phase transition exactly once
            RPC_StartRunPhase();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority, Channel = RpcChannel.Reliable)]
        private void RPC_StartRunPhase()
        {
            runPhaseItems.SetActive(true);
            buyPhaseEndsPS.Play();
            buyPhaseItems.SetActive(false);
            enabled = false;
        }
    }
}
