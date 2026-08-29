using System.Linq;
using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using UnityEngine;

namespace _Project.Abilities.Scripts
{
    public class AbilityRoundState : NetworkBehaviour
    {
        private const int AbilityCapacity = 128;
        private const int ChoicesPerToken = 3;

        [SerializeField] private AbilityDatabase database;

        [Networked, Capacity(AbilityCapacity)]
        private NetworkArray<int> AbilityIds
        {
            get { return default; }
        }

        [Networked] private int AbilitySetLength { get; set; }
        [Networked] private int NextAbilityIndex { get; set; }
        [Networked] public int CurrentChoiceStart { get; private set; }
        [Networked] public PlayerRef SelectionOwner { get; private set; }
        [Networked] private int SelectionSequence { get; set; }
        [Networked] private int RoundIndex { get; set; }

        public override void Spawned()
        {
            EventBus.Subscribe<RoundOverEvent>(OnRoundOver);
            EventBus.Subscribe<TokenCollectedEvent>(OnTokenCollected);

            if (HasStateAuthority)
                GenerateAbilitySet();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            EventBus.Unsubscribe<RoundOverEvent>(OnRoundOver);
            EventBus.Unsubscribe<TokenCollectedEvent>(OnTokenCollected);
        }

        public int GetAbilityId(int index)
        {
            if (index < 0 || index >= AbilitySetLength)
                return -1;

            return AbilityIds[index];
        }

        public bool HasSelectionFor(PlayerRef player, out int sequence)
        {
            sequence = SelectionSequence;
            return SelectionOwner != PlayerRef.None &&
                   SelectionOwner == player;
        }

        public bool IsSelectionActiveFor(PlayerRef player)
        {
            return SelectionOwner != PlayerRef.None &&
                   SelectionOwner == player;
        }

        private void OnTokenCollected(TokenCollectedEvent ev)
        {
            if (!HasStateAuthority || ev.CollectedBy != PlayerRole.Chaser)
                return;

            // A Chaser can only have one active choice. Additional tokens are
            // intentionally ignored until the current choice is completed.
            if (SelectionOwner != PlayerRef.None)
                return;

            if (NextAbilityIndex + ChoicesPerToken > AbilitySetLength)
                return;

            CurrentChoiceStart = NextAbilityIndex;
            NextAbilityIndex += ChoicesPerToken;
            SelectionOwner = ev.Collector;
            SelectionSequence++;
        }

        public bool TrySelectAbility(Player player, int slot)
        {
            if (!HasStateAuthority || player == null || player.Role != PlayerRole.Chaser)
                return false;

            if (SelectionOwner != player.Object.InputAuthority)
                return false;

            if (slot < 0 || slot >= ChoicesPerToken)
                return false;

            var abilityIndex = CurrentChoiceStart + slot;
            if (abilityIndex < 0 || abilityIndex >= AbilitySetLength)
                return false;

            if (database == null || database.allAbilities == null)
                return false;

            var abilityId = AbilityIds[abilityIndex];
            if (abilityId < 0 || abilityId >= database.allAbilities.Count)
                return false;

            var ability = database.allAbilities[abilityId];
            if (ability == null || ability.effects == null)
                return false;

            foreach (var effect in ability.effects.Where(effect => effect != null))
            {
                effect.ApplyEffect(player);
            }

            SelectionOwner = PlayerRef.None;
            return true;
        }

        private void OnRoundOver(RoundOverEvent ev)
        {
            if (!HasStateAuthority)
                return;

            RoundIndex++;
            NextAbilityIndex = 0;
            CurrentChoiceStart = 0;
            SelectionOwner = PlayerRef.None;

            // Alternate between reusing the existing ordered set and generating
            // a completely new random set.
            if (RoundIndex % 2 == 0)
                GenerateAbilitySet();
        }

        private void GenerateAbilitySet()
        {
            if (database == null || database.allAbilities == null)
                return;

            var candidates = database.allAbilities
                .Select((ability, index) => new { ability, index })
                .Where(entry => entry.ability != null && entry.ability.abilityScope != AbilityScope.Runner)
                .Select(entry => entry.index)
                .ToList();

            for (var i = candidates.Count - 1; i > 0; i--)
            {
                var swapIndex = Random.Range(0, i + 1);
                (candidates[i], candidates[swapIndex]) =
                    (candidates[swapIndex], candidates[i]);
            }

            AbilitySetLength = Mathf.Min(candidates.Count, AbilityCapacity);
            for (var i = 0; i < AbilitySetLength; i++)
                AbilityIds.Set(i, candidates[i]);
        }
    }
}
