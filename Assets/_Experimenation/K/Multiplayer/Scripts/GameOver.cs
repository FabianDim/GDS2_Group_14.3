using _Experimenation.K.Game_Manager.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Experimenation.K.Multiplayer.Scripts
{
    public class GameOver : MonoBehaviour
    {
        private TextMeshProUGUI _p1Name;
        private TextMeshProUGUI _p2Name;
        private TextMeshProUGUI _p1Score;
        private TextMeshProUGUI _p2Score;
        private TextMeshProUGUI _winner;
        private readonly GameData _gameData = GameData.Instance;

        private void Awake()
        {
            _p1Name = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            _p2Name = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            _p1Score = _p1Name.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            _p2Score = _p2Name.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            _winner = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            
            _p1Name.SetText(_gameData.P1Data.Username.Value);
            _p2Name.SetText(_gameData.P2Data.Username.Value);
            _p1Score.SetText(_gameData.P1Data.Score.ToString());
            _p2Score.SetText(_gameData.P2Data.Score.ToString());
            _winner.SetText(
                (_gameData.P1Data.Score > _gameData.P2Data.Score ? 
                    _p1Name.text : _p2Name.text) 
                + " wins!");
        }

        public void BackToMenu() => SceneManager.LoadScene(0);
    }
}
