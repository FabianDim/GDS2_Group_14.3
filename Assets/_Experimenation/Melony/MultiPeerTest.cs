using System;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Experimenation.Melony
{
    public class MultiPeerTest : MonoBehaviour
    {
        public NetworkRunner runnerPrefab;

        private async void Start()
        {
            try
            {
                int currSceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
        
                await StartInstance(GameMode.Host, "LocalTestRoom", currSceneBuildIndex);
                await StartInstance(GameMode.Client, "LocalTestRoom", currSceneBuildIndex);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private async Task StartInstance(GameMode mode, string roomName, int buildIndex)
        {
            var newRunner = Instantiate(runnerPrefab);

            await newRunner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = roomName,
                Scene = SceneRef.FromIndex(buildIndex), 
                SceneManager = newRunner.GetComponent<NetworkSceneManagerDefault>()
            });
        }
    }
}