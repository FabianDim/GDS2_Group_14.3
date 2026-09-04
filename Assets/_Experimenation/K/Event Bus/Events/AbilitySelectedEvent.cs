namespace _Experimenation.K.Event_Bus.Events
{
    public class AbilitySelectedEvent
    {
        public readonly int SelectedAbility;
        
        public AbilitySelectedEvent(int selectedAbility)
        {
            SelectedAbility = selectedAbility;
        }
    }
}
