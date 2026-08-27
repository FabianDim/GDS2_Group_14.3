using _Experimenation.K.Multiplayer.Scripts;
using Fusion;

namespace _Experimenation.K.Event_Bus.Events
{
    public class TokenCollectedEvent
    {
        public readonly int Points;
        public readonly PlayerRole CollectedBy;
        public TokenCollectedEvent(int points, PlayerRole collectedBy)
        {
            Points = points;
            CollectedBy = collectedBy;
        }
    }
}