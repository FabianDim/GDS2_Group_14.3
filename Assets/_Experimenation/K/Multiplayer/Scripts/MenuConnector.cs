using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine;

namespace _Experimenation.K.Multiplayer.Scripts
{
    public class MenuConnector : MonoRunnerCallbacks
    {
        [SerializeField] private NetworkRunner runnerPrefab;
        [SerializeField] private TextMeshProUGUI connectionText;

        private NetworkRunner _networkRunner;
        private bool _gameStarted;
        private bool _isConnecting;

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
                Debug.LogError($"MenuConnector: connection failed. {e}");
                _isConnecting = false;
            }
        }

        private async Task StartGame(GameMode mode)
        {
            if (_isConnecting || (_networkRunner != null && _networkRunner.IsRunning))
            {
                Debug.LogWarning("A network connection is already running.");
                return;
            }

            _isConnecting = true;
            _gameStarted = false;

            if (runnerPrefab == null)
            {
                _isConnecting = false;
                Debug.LogError("MenuConnector: NetworkRunner prefab is not assigned.");
                return;
            }

            _networkRunner = Instantiate(runnerPrefab);
            _networkRunner.name = "Network Runner";
            _networkRunner.AddCallbacks(this);
            _networkRunner.ProvideInput = true; // this client will feed input to Fusion

            var sceneManager = _networkRunner.gameObject.AddComponent<NetworkSceneManagerDefault>();
            var result = await _networkRunner.StartGame(new StartGameArgs
            {
                GameMode = mode,
                SessionName = "TestRoom1v1", // hardcoded room name for a demo; later make this a text field
                PlayerCount = 2,
                Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
                SceneManager = sceneManager,
                ObjectProvider = _networkRunner.GetComponent<INetworkObjectProvider>()
            });

            if (!result.Ok)
            {
                Debug.LogError($"Failed to start: {result.ShutdownReason}");
                Destroy(_networkRunner.gameObject);
                _networkRunner = null;
                _isConnecting = false;
            }
            else
            {
                _isConnecting = false;
            }
        }

        public override void OnConnectedToServer(NetworkRunner runner)
        {
            Debug.Log($"MenuConnector: connected as {runner.LocalPlayer} ({runner.GameMode}).");
        }

        public override void OnSceneLoadStart(NetworkRunner runner)
        {
            Debug.Log("MenuConnector: scene load started.");
        }

        public override void OnSceneLoadDone(NetworkRunner runner)
        {
            Debug.Log($"MenuConnector: scene load completed. ActiveScene={SceneManager.GetActiveScene().buildIndex}");
        }

        public override void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Debug.LogWarning($"MenuConnector: runner shut down. Reason={shutdownReason}");
            _isConnecting = false;
            _gameStarted = false;
            _networkRunner = null;
        }

        public override void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            Debug.LogWarning($"MenuConnector: disconnected from server. Reason={reason}");
        }

        public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            // Only the scene authority (the host, in Host Mode) decides when
            // the match is ready to transition.
            if (!runner.IsSceneAuthority || _gameStarted)
                return;

            if (runner.ActivePlayers.Count() != 2)
                return;
            if (connectionText != null)
                connectionText.SetText("Both players connected. Loading game...");

            _gameStarted = true;
            var gameSceneIndex = 1;
            if (gameSceneIndex >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogError($"Game scene index {gameSceneIndex} is not present in Build Settings.");
                return;
            }

            Debug.Log($"MenuConnector: both players joined. Loading game scene index {gameSceneIndex}.");
            runner.LoadScene(SceneRef.FromIndex(gameSceneIndex));
        }
    }
}
