namespace _Experimenation.K.Event_Bus.Events
{
    public class TokenCollectedEvent
    {
        public readonly int Points;
        public readonly string CollectedBy;
        public TokenCollectedEvent(int points, string collectedBy)
        {
            Points = points;
            CollectedBy = collectedBy;
        }
    }
}