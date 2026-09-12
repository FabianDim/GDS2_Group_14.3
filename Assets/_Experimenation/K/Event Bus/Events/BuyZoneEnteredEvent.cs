namespace _Experimenation.K.Event_Bus.Events
{
    public class BuyZoneEnteredEvent
    {
        public readonly bool Entered;
        
        public BuyZoneEnteredEvent(bool entered)
        {
            Entered = entered;
        }
    }
}
