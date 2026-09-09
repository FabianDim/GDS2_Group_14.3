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
            _timer = TickTimer.CreateFromSeconds(Runner, GameData.Instance.roundDuration * 0.25f);
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || !_timer.Expired(Runner)) return;
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
