namespace _Experimenation.K.Event_Bus.Events
{
    public class RoundOverEvent
    {
        public readonly bool RunnerWins;
        
        public RoundOverEvent(bool runnerWins)
        {
            RunnerWins = runnerWins;
        }
    }
}
