using System.Collections.Generic;
using System.Linq;
using _Experimenation.Fabian.Scripts.PhaseController;
using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Multiplayer.Scripts;
using _Project.Utility_Scripts;
using Fusion;
using UnityEngine;

namespace _Project.Abilities.Scripts
{
    /// <summary>
    /// State-authoritative ability selection used during the Run Phase.
    /// </summary>
    public sealed class RunPhaseAbilitySystem : NetworkBehaviour
    {
        private const int AbilityChoiceCount = 3;

        [SerializeField] private AbilityDatabase database;

        private PhaseManager _phaseManager;
        private AbilityUIManager _abilityUIManager;
        private List<Ability> _randomAbilitySet = new();
        private List<Ability> _abilityChoices = new();
        private bool _isShowingAbilities;
        private PlayerRef _choiceOwner;
        private bool _newSet;

        [Networked] private int AbilityIndex { get; set; }

        public override void Spawned()
        {
            _abilityUIManager = GetComponentInChildren<AbilityUIManager>(true);
            _phaseManager = GetComponentInChildren<PhaseManager>(true);

            if (!HasStateAuthority)
                return;

            EventBus.Subscribe<TokenCollectedEvent>(OnTokenCollected);
            EventBus.Subscribe<RoundOverEvent>(OnRoundOver);
            EventBus.Subscribe<AbilitySelectedEvent>(OnAbilitySelected);
            GenerateRandomAbilitySet();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (!HasStateAuthority)
                return;

            EventBus.Unsubscribe<TokenCollectedEvent>(OnTokenCollected);
            EventBus.Unsubscribe<RoundOverEvent>(OnRoundOver);
            EventBus.Unsubscribe<AbilitySelectedEvent>(OnAbilitySelected);
        }

        private bool IsRunPhase => _phaseManager != null && _phaseManager.IsRunPhase;

        private bool TryGetAbilityChoices(out List<Ability> choices)
        {
            choices = null;
            if (_randomAbilitySet == null || _randomAbilitySet.Count < AbilityChoiceCount)
                return false;

            var startIndex = AbilityIndex;
            if (startIndex < 0 || startIndex >= _randomAbilitySet.Count)
                startIndex = 0;

            choices = _randomAbilitySet
                .Skip(startIndex)
                .Take(AbilityChoiceCount)
                .ToList();

            if (choices.Count == AbilityChoiceCount) return choices.Count == AbilityChoiceCount;
            AbilityIndex = 0;
            choices = _randomAbilitySet.Take(AbilityChoiceCount).ToList();

            return choices.Count == AbilityChoiceCount;
        }

        private void GenerateRandomAbilitySet()
        {
            if (database == null || database.allAbilities == null)
            {
                _randomAbilitySet.Clear();
                return;
            }

            var chaserAbilityPool = database.allAbilities
                .Where(ability => 
                    ability &&
                    (ability.abilityScope == AbilityScope.General ||
                     ability.abilityScope == AbilityScope.Chaser) &&
                    ability.abilityType != AbilityType.Tool)
                .ToList();

            if (chaserAbilityPool.Count == 0)
            {
                _randomAbilitySet.Clear();
                Debug.LogWarning("RunPhaseAbilitySystem: no General or Chaser non-tool abilities are available.", this);
                return;
            }

            var extraIndex = 0;
            while (chaserAbilityPool.Count % AbilityChoiceCount != 0)
                chaserAbilityPool.Add(chaserAbilityPool[extraIndex++]);
                
            _randomAbilitySet = ListUtility.Shuffle(chaserAbilityPool);
        }

        private void OnTokenCollected(TokenCollectedEvent ev)
        {
            if (!IsRunPhase || _isShowingAbilities || ev == null ||
                ev.CollectedBy == null || ev.CollectedBy.Role != PlayerRole.Chaser ||
                ev.CollectedBy.Runner != Runner || ev.CollectedBy.Object == null ||
                !ev.Collector.IsValid || ev.CollectedBy.Object.InputAuthority != ev.Collector ||
                _randomAbilitySet.Count < AbilityChoiceCount)
            {
                return;
            }

            if (!TryGetAbilityChoices(out _abilityChoices))
                return;

            _isShowingAbilities = true;
            _choiceOwner = ev.Collector;
            AbilityIndex += AbilityChoiceCount;
            if (AbilityIndex + AbilityChoiceCount > _randomAbilitySet.Count)
                AbilityIndex = 0;

            var choiceIndices = _abilityChoices
                .Select(ability => database.allAbilities.IndexOf(ability))
                .ToArray();
            RPC_ShowAbilities(ev.Collector, choiceIndices);
        }

        private void OnRoundOver(RoundOverEvent ev)
        {
            AbilityIndex = 0;
            _isShowingAbilities = false;
            _choiceOwner = PlayerRef.None;
            _abilityChoices.Clear();

            if (_newSet)
                GenerateRandomAbilitySet();
            _newSet = !_newSet;
        }

        private void OnAbilitySelected(AbilitySelectedEvent ev)
        {
            if (!IsRunPhase || !_isShowingAbilities || ev == null || ev.Player == null ||
                ev.Player.Object == null || !ev.Player.HasStateAuthority ||
                ev.Player.Runner != Runner || ev.Player.Object.InputAuthority != _choiceOwner ||
                ev.Player.Role != PlayerRole.Chaser ||
                ev.SelectedAbility < 1 || ev.SelectedAbility > _abilityChoices.Count)
            {
                return;
            }

            var ability = _abilityChoices[ev.SelectedAbility - 1];
            _isShowingAbilities = false;
            _choiceOwner = PlayerRef.None;
            _abilityChoices.Clear();
            AbilityEffectApplier.ApplyAll(ability, ev.Player);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_ShowAbilities(PlayerRef chaser, int[] choiceIndices)
        {
            if (database == null || database.allAbilities == null ||
                choiceIndices == null || choiceIndices.Length != AbilityChoiceCount)
            {
                return;
            }

            if (Runner == null || chaser != Runner.LocalPlayer || _abilityUIManager == null)
                return;

            if (!HasStateAuthority)
            {
                _abilityChoices.Clear();
                foreach (var index in choiceIndices)
                {
                    if (index >= 0 && index < database.allAbilities.Count)
                        _abilityChoices.Add(database.allAbilities[index]);
                }
            }

            _abilityUIManager.ShowAbilities(_abilityChoices);
        }
    }
}
