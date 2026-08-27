using System.Linq;
using _Experimenation.K.Abilities.Scripts;
using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using UnityEngine;

namespace _Experimenation.K.Game_Manager.Abilities.Scripts
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
        [Networked] private PlayerRef SelectionOwner { get; set; }
        [Networked] private int SelectionSequence { get; set; }
        [Networked] private int RoundIndex { get; set; }

        public override void Spawned()
        {
            EventBus.Subscribe<RoundOverEvent>(OnRoundOver);

            if (HasStateAuthority)
                GenerateAbilitySet();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            EventBus.Unsubscribe<RoundOverEvent>(OnRoundOver);
        }

        public int GetAbilityId(int index)
        {
            if (index < 0 || index >= AbilitySetLength)
                return -1;

            return AbilityIds[index];
        }

        public bool HasNewSelectionFor(PlayerRef player, out int sequence)
        {
            sequence = SelectionSequence;
            return SelectionOwner == player;
        }

        public void RegisterTokenCollection(PlayerRef collector, PlayerRole role)
        {
            if (!HasStateAuthority || role != PlayerRole.Chaser)
                return;

            if (SelectionOwner != PlayerRef.None)
                return;

            if (NextAbilityIndex + ChoicesPerToken > AbilitySetLength)
                return;

            CurrentChoiceStart = NextAbilityIndex;
            NextAbilityIndex += ChoicesPerToken;
            SelectionOwner = collector;
            SelectionSequence++;
        }

        public void TrySelectAbility(Player player, int slot)
        {
            if (!HasStateAuthority || player == null || player.Role != PlayerRole.Chaser)
                return;

            if (SelectionOwner != player.Object.InputAuthority)
                return;

            if (slot < 0 || slot >= ChoicesPerToken)
                return;

            var abilityIndex = CurrentChoiceStart + slot;
            if (abilityIndex < 0 || abilityIndex >= AbilitySetLength)
                return;

            var ability = database.allAbilities[AbilityIds[abilityIndex]];
            foreach (var effect in ability.effects)
                effect.ApplyEffect();

            SelectionOwner = PlayerRef.None;
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
            var candidates = database.allAbilities
                .Select((ability, index) => new { ability, index })
                .Where(entry => entry.ability.abilityScope != AbilityScope.Runner)
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
