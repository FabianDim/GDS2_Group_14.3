using System;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using TMPro;
using UnityEngine;

namespace _Experimenation.K.Multiplayer.Scripts
{
    public class MenuConnector : MonoRunnerCallbacks
    {
        [SerializeField] private NetworkRunner runnerPrefab;
        [SerializeField] private TextMeshProUGUI connectionText;
        [SerializeField] private TMP_InputField roomId;
        [SerializeField] private TextMeshProUGUI multiplayerLog;

        private NetworkRunner _networkRunner;
        private bool _gameStarted;

        private void Awake()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            
            var log = MultiplayerLog.GetLog();
            if (log == null) return;
            multiplayerLog.SetText(log);
            multiplayerLog.transform.parent.gameObject.SetActive(true);
        }

        public void HostGame() => Connect(GameMode.Host);
        public void JoinGame() => Connect(GameMode.Client);

        private async void Connect(GameMode mode)
        {
            try
            {
                await StartGame(mode);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private async Task StartGame(GameMode mode)
        {
            _networkRunner = Instantiate(runnerPrefab);
            _networkRunner.name = "Network Runner";
            _networkRunner.AddCallbacks(this);
            _networkRunner.ProvideInput = true; // this client will feed input to Fusion

            // No scene is passed here on purpose. The host stays in the Menu
            // scene until the second player joins (see OnPlayerJoined below),
            // then explicitly transitions via Runner.LoadScene().
            var result = await _networkRunner.StartGame(new StartGameArgs
            {
                GameMode = mode,
                SessionName = roomId.text,
                SceneManager = _networkRunner.gameObject.AddComponent<NetworkSceneManagerDefault>(),
                ObjectProvider = _networkRunner.GetComponent<INetworkObjectProvider>()
            });

            if (!result.Ok)
            {
                Debug.LogError($"Failed to start: {result.ShutdownReason}");
            }
        }

        public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            // Only the scene authority (the host, in Host Mode) decides when
            // the match is ready to transition.
            if (!runner.IsSceneAuthority) return;
            if (_gameStarted) return;
            if (runner.ActivePlayers.Count() < 2)
            {
                connectionText.SetText("Waiting for the other player");
                return;
            }

            _gameStarted = true;
            runner.LoadScene(SceneRef.FromIndex(1));
        }
    }
}
