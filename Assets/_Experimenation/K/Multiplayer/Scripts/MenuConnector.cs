using System;
using Fusion;
using UnityEngine;

namespace _Experimenation.K.Multiplayer.Scripts
{
    public class MenuConnector : NetworkRunnerCallbacks
    {
        [SerializeField] private NetworkRunner runnerPrefab;
        private NetworkRunner _runner;

        public async void HostGame()
        {
            try
            {
                await StartGame(GameMode.Host);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public async void JoinGame()
        {
            try
            {
                await StartGame(GameMode.Client);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private async System.Threading.Tasks.Task StartGame(GameMode mode)
        {
            _runner = Instantiate(runnerPrefab);
            _runner.name = "Network Runner";
            _runner.AddCallbacks(this);
            _runner.ProvideInput = true; // this client will feed input to Fusion

            var sceneInfo = new NetworkSceneInfo();
            // Only the host actually loads a new scene; clients are pulled in automatically
            if (mode == GameMode.Host)
            {
                sceneInfo.AddSceneRef(
                    SceneRef.FromIndex(1)
                );
            }

            var result = await _runner.StartGame(new StartGameArgs
            {
                GameMode = mode,
                SessionName = "TestRoom1v1", // hardcoded room name for a demo; later make this a text field
                Scene = sceneInfo,
                SceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>()
            });

            if (!result.Ok)
            {
                Debug.LogError($"Failed to start: {result.ShutdownReason}");
            }
        }
    }
}