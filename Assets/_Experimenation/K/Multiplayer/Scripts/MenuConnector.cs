using System;
using System.Linq;
using System.Threading.Tasks;
using _Experimenation.K.Game_Manager.Scripts;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Experimenation.K.Multiplayer.Scripts
{
    public class MenuConnector : MonoRunnerCallbacks
    {
        private const string GameplayScenePath = "Assets/_Project/Scenes/Multiplayer Test Scene.unity";

        [SerializeField] private NetworkRunner runnerPrefab;
        [SerializeField] private GameData gameDataPrefab;

        [Space, Header("Performance")]
        [Tooltip("Caps the frame rate. VSync is off in this project, so without a cap the build runs uncapped and frame times get noisy (client-side jitter). Match this to the Fusion tick rate (60) unless you have a reason not to.")]
        [SerializeField, Min(0)] private int targetFrameRate = 60;

        [Space, SerializeField] private TextMeshProUGUI connectionText;
        [SerializeField] private TMP_InputField roomId;
        [SerializeField] private TextMeshProUGUI multiplayerLog;
        [SerializeField] private TextMeshProUGUI username;
        [SerializeField] private TMP_InputField numberOfRounds;
        [SerializeField] private TMP_InputField roundDuration;

        private NetworkRunner _networkRunner;
        private GameData _gameData;
        private bool _gameStarted;
        private bool _callbacksRegistered;
        private int _gameplaySceneIndex;

        private void Awake()
        {
            // VSync is disabled in QualitySettings; without an explicit cap the
            // build renders uncapped and the unstable frame times show up as
            // client-side jitter. 0 = uncapped (leave the field at 0 to disable).
            Application.targetFrameRate = targetFrameRate;

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
            // A second click must not create a competing runner for this menu.
            if (_networkRunner != null && !_networkRunner.IsShutdown) return;

            try
            {
                await StartGame(mode);
            }
            catch (Exception e)
            {
                SetConnectionStatus("Connection failed. See the Console for details.");
                Debug.LogError(e);
            }
        }

        private async Task StartGame(GameMode mode)
        {
            _gameplaySceneIndex = SceneUtility.GetBuildIndexByScenePath(GameplayScenePath);
            if (_gameplaySceneIndex < 0)
            {
                SetConnectionStatus("Cannot start: gameplay scene is missing from the build.");
                Debug.LogError($"Enable {GameplayScenePath} in the build scene list.");
                return;
            }

            _gameStarted = false;
            _gameData = null;
            SetConnectionStatus(mode == GameMode.Host ? "Creating room..." : "Joining room...");
            _networkRunner = Instantiate(runnerPrefab);
            _networkRunner.name = "Network Runner";
            _networkRunner.AddCallbacks(this);
            _callbacksRegistered = true;
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
                SetConnectionStatus($"Connection failed: {result.ShutdownReason}");
                Debug.LogError($"Failed to start: {result.ShutdownReason}");
            }
            else
            {
                Debug.Log($"Connected as {mode} to room '{_networkRunner.SessionInfo.Name}' " +
                          $"in region '{_networkRunner.SessionInfo.Region}'.");
                if (!_gameStarted)
                    SetConnectionStatus(mode == GameMode.Host
                        ? "Waiting for the other player"
                        : "Connected. Waiting for the host to start the match...");
            }
        }

        public override void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            SetConnectionStatus($"Disconnected: {shutdownReason}");
            Debug.Log($"Menu connection shut down: {shutdownReason}");
            RemoveRunnerCallbacks(runner);
        }

        private void SetConnectionStatus(string message)
        {
            if (connectionText != null)
                connectionText.SetText(message);
        }

        private void OnDestroy()
        {
            RemoveRunnerCallbacks(_networkRunner);
        }

        private void RemoveRunnerCallbacks(NetworkRunner runner)
        {
            if (!_callbacksRegistered)
                return;

            runner?.RemoveCallbacks(this);
            _callbacksRegistered = false;
        }

        public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"{player} joined. Host: {runner.IsServer}, players: {runner.ActivePlayers.Count()}/2.");
            // Host Mode clients receive GameData through replication; they cannot
            // spawn it. This callback also runs again when another player joins.
            if (!runner.IsServer || _gameStarted) return;

            if (_gameData == null)
            {
                _gameData = runner.Spawn(gameDataPrefab);
                _gameData.SetUsername(username.text ?? $"Player_{runner.ActivePlayers.Count()}");

                // Empty/invalid inputs keep the prefab defaults instead of aborting
                // the join callback. The match requires an even number of rounds.
                if (numberOfRounds != null && int.TryParse(numberOfRounds.text, out var nRound) &&
                    nRound > 0 && nRound < int.MaxValue)
                    _gameData.numberOfRounds = nRound % 2 == 0 ? nRound : nRound + 1;

                if (roundDuration != null && int.TryParse(roundDuration.text, out var duration) &&
                    duration > _gameData.roundDuration)
                    _gameData.roundDuration = duration;
            }

            // Only the scene authority (the host, in Host Mode) decides when
            // the match is ready to transition.
            if (runner.ActivePlayers.Count() < 2)
            {
                SetConnectionStatus("Waiting for the other player");
                return;
            }

            _gameStarted = true;
            SetConnectionStatus("Both players connected. Loading match...");
            Debug.Log($"Loading gameplay scene '{GameplayScenePath}' (build index {_gameplaySceneIndex}).");
            runner.LoadScene(SceneRef.FromIndex(_gameplaySceneIndex));
        }
    }
}
