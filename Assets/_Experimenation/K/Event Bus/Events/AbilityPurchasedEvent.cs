using Fusion;

namespace _Experimenation.K.Event_Bus.Events
{
    public sealed class AbilityPurchasedEvent
    {
        public readonly PlayerRef Buyer;
        public readonly int AbilityIndex;
        public readonly bool Accepted;

        public AbilityPurchasedEvent(PlayerRef buyer, int abilityIndex, bool accepted)
        {
            Buyer = buyer;
            AbilityIndex = abilityIndex;
            Accepted = accepted;
        }
    }
}
