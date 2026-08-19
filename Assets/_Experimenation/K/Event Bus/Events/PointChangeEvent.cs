namespace _Experimenation.K.Event_Bus.Events
{
    public class PointChangeEvent
    {
        public int points;
        public PointChangeEvent(int points)
        {
            this.points = points;
        }
    }
}