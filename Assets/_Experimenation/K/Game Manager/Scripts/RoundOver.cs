using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using Fusion;
using TMPro;
using UnityEngine;

namespace _Experimenation.K.Game_Manager.Scripts
{
    public class RoundOver : NetworkBehaviour
    {
        private GameObject _screen;
        private TextMeshProUGUI _text;

        [SerializeField] private float duration = 5f;
        private TickTimer _timer;
        private bool _sceneLoadTriggered;
        
        [SerializeField] private GameData gameData;

        public override void Spawned()
        {
            _screen = transform.GetChild(0).gameObject;
            _text = _screen.GetComponentInChildren<TextMeshProUGUI>();

            if (!HasStateAuthority) return;
            EventBus.Subscribe<RoundOverEvent>(OnRoundOver);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (!HasStateAuthority) return;
            EventBus.Unsubscribe<RoundOverEvent>(OnRoundOver);
        }

        public override void FixedUpdateNetwork()
        {
            // Only the host (scene authority) triggers the reload — clients follow the
            // server's scene change automatically via the network scene manager.
            if (!HasStateAuthority || _sceneLoadTriggered)
                return;

            if (!_timer.IsRunning || !_timer.Expired(Runner))
                return;

            _sceneLoadTriggered = true;
            if(gameData.currentRound >= gameData.numberOfRounds)
                Runner.LoadScene(SceneRef.FromIndex(2));
            else
            {
                gameData.currentRound++;
                Runner.LoadScene(SceneRef.FromIndex(1));
            }
        }

        private void OnRoundOver(RoundOverEvent ev)
        {
            gameData.UpdateScore(ev.RunnerWins);
            _timer = TickTimer.CreateFromSeconds(Runner, duration);
            RPC_ShowScreen(ev.RunnerWins);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_ShowScreen(bool runnerWins)
        {
            _text.SetText(runnerWins ? "Runner wins!" : "Chaser caught Runner!");
            _screen.SetActive(true);
        }
    }
}
