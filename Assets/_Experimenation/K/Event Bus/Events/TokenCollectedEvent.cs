using _Experimenation.K.Multiplayer.Scripts;
using Fusion;

namespace _Experimenation.K.Event_Bus.Events
{
    public class TokenCollectedEvent
    {
        public readonly int Points;
        public readonly PlayerRole CollectedBy;
        public readonly PlayerRef Collector;

        public TokenCollectedEvent(
            int points,
            PlayerRole collectedBy,
            PlayerRef collector = default)
        {
            Points = points;
            CollectedBy = collectedBy;
            Collector = collector;
        }
    }
}