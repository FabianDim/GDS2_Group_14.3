using _Experimenation.K.Multiplayer.Scripts;

namespace _Experimenation.K.Event_Bus.Events
{
    public class AbilitySelectedEvent
    {
        public readonly int SelectedAbility;
        public readonly Player Player;
        
        public AbilitySelectedEvent(int selectedAbility, Player player)
        {
            SelectedAbility = selectedAbility;
            Player = player;
        }
    }
}
