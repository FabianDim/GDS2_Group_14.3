using UnityEngine;
using Fusion;
using System.Threading.Tasks;
using UnityEngine.SceneManagement; 

public class MultiPeerTest : MonoBehaviour
{
    public NetworkRunner runnerPrefab; 

    async void Start()
    {
        int currSceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
        
        await StartInstance(GameMode.Host, "LocalTestRoom", currSceneBuildIndex);
        await StartInstance(GameMode.Client, "LocalTestRoom", currSceneBuildIndex);
    }

    async Task StartInstance(GameMode mode, string roomName, int buildIndex)
    {
        NetworkRunner newRunner = Instantiate(runnerPrefab);

        await newRunner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = roomName,
            Scene = SceneRef.FromIndex(buildIndex), 
            SceneManager = newRunner.GetComponent<NetworkSceneManagerDefault>()
        });
    }
}