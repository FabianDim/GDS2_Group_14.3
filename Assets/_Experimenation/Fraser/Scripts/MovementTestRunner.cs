using Fusion;
using UnityEngine;

public class MovementTestRunner : MonoBehaviour
{
    [SerializeField] private NetworkRunner runnerPrefab;

    private NetworkRunner runner;

    private async void Start()
    {
        runner = Instantiate(runnerPrefab);

        runner.name = "Movement Test Runner";
        runner.ProvideInput = false;

        NetworkSceneInfo sceneInfo = new NetworkSceneInfo();

        sceneInfo.AddSceneRef(
            SceneRef.FromIndex(
                gameObject.scene.buildIndex
            )
        );

        StartGameResult result = await runner.StartGame(
            new StartGameArgs
            {
                GameMode = GameMode.Single,
                Scene = sceneInfo,
                SceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>()
            }
        );

        if (!result.Ok)
        {
            Debug.LogError(
                $"Movement test failed to start: {result.ShutdownReason}"
            );
        }
    }
}