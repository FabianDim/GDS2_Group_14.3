using _Experimenation.K.Multiplayer.Scripts;
using UnityEngine;

namespace _Experimenation.K.Game_Manager.Scripts
{
    [CreateAssetMenu(menuName = "Game Data", fileName = "GameData")]
    public class GameData : ScriptableObject
    {
        public int p1Points;
        public int p2Points;
        public int p1Score;
        public int p2Score;
        public PlayerRole p1Role;
        public PlayerRole p2Role;

        public int numberOfRounds;
        public int currentRound;

        public void GenerateRole()
        {
            if (p1Role == 0 || p2Role == 0)
            {
                p1Role = (PlayerRole)Random.Range(1, 3);
                p2Role = p1Role == PlayerRole.Chaser ? PlayerRole.Runner : PlayerRole.Chaser;
            }
            else
                (p1Role, p2Role) = (p2Role, p1Role);
        }

        public void UpdateScore(bool runnerScores)
        {
            var runnerScore = p1Role == PlayerRole.Runner ? p1Score : p2Score;
            var chaserScore = runnerScore == p1Score ? p2Score : p1Score;
            if(runnerScores) runnerScore++;
            else chaserScore++;
        }
    }
}
