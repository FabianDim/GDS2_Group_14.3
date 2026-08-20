namespace _Experimenation.K.Event_Bus.Events
{
    public class TokenCollectedEvent
    {
        public readonly int points;
        public readonly bool collectedByChaser;
        public TokenCollectedEvent(int points, bool collectedByChaser)
        {
            this.points = points;
            this.collectedByChaser = collectedByChaser;
        }
    }
}