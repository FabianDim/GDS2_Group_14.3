using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
namespace _Project.Menu.Splash_Screens
{
    public class SplashScreen : MonoBehaviour
    {
        private void Awake() => 
            GetComponentInChildren<VideoPlayer>().loopPointReached += OnSequenceFinished;

        private static void OnSequenceFinished(VideoPlayer vp) => 
            SceneManager.LoadScene("_Project/Scenes/Main Menu");
    }
}
