using System;
using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Experimenation.K.Game_Manager.Scripts
{
    public class RoundOver : NetworkBehaviour
    {
        private GameObject _screen;
        private TextMeshProUGUI _text;

        [SerializeField] private float duration = 5f;
        private TickTimer _timer;
        private bool _sceneLoadTriggered;
        private GameData _gameData;

        // Resolved lazily: this component can initialize before GameData.Spawned()
        // assigns the singleton, so a field initializer would cache a null reference.
        private GameData ResolveGameData()
        {
            if (_gameData == null)
                _gameData = GameData.Instance;
            return _gameData;
        }

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
            var gameData = ResolveGameData();
            if (gameData == null)
            {
                // GameData not available yet - keep the timer armed and retry next tick.
                _sceneLoadTriggered = false;
                return;
            }

            if(gameData.CurrentRound >= gameData.numberOfRounds)
                RPC_GameOver();
            else
            {
                gameData.CurrentRound++;
                Runner.LoadScene(SceneRef.FromIndex(1));
            }
        }

        private void OnRoundOver(RoundOverEvent ev)
        {
            ResolveGameData()?.UpdateScore(ev.RunnerWins);
            _timer = TickTimer.CreateFromSeconds(Runner, duration);
            RPC_ShowScreen(ev.RunnerWins);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_ShowScreen(bool runnerWins)
        {
            _text.SetText(runnerWins ? "Runner wins!" : "Chaser caught Runner!");
            _screen.SetActive(true);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_GameOver() => GameOver();

        private async void GameOver()
        {
            try
            {
                await Runner.Shutdown();
                SceneManager.LoadScene(2);
            }
            catch (Exception e)
            {
                Debug.LogError($"From RoundOver.cs: {e}");
            }
        }
    }
}
