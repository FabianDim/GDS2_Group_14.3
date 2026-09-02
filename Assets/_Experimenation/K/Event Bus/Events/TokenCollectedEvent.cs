using _Experimenation.K.Multiplayer.Scripts;
using Fusion;

namespace _Experimenation.K.Event_Bus.Events
{
    public class TokenCollectedEvent
    {
        public readonly int Points;
        public readonly Player CollectedBy;
        public readonly PlayerRef Collector;

        public TokenCollectedEvent(int points, Player collectedBy, PlayerRef collector)
        {
            Points = points;
            CollectedBy = collectedBy;
            Collector = collector;
        }
    }
}