using System.Collections.Generic;
using System.Linq;
using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Multiplayer.Scripts;
using _Project.Utility_Scripts;
using Fusion;
using UnityEngine;

namespace _Project.Abilities.Scripts
{
    public class AbilitySystem : NetworkBehaviour
    {
        [SerializeField] private AbilityDatabase database;
        private AbilityUIManager _abilityUIManager;
        private List<Ability> _randomAbilitySet = new();
        private const int AbilityChoiceCount = 3;
        private List<Ability> _abilityChoices = new();
        private bool _isShowingAbilities;
        private bool _newSet;
        
        [Networked] private int AbilityIndex { get; set; }

        public override void Spawned()
        {
            _abilityUIManager = GetComponent<AbilityUIManager>();
            
            if (!HasStateAuthority) return;
            
            EventBus.Subscribe<TokenCollectedEvent>(OnTokenCollected);   
            EventBus.Subscribe<RoundOverEvent>(OnRoundOver);
            EventBus.Subscribe<AbilitySelectedEvent>(OnAbilitySelected);
            EventBus.Subscribe<AbilityPurchasedEvent>(OnAbilityPurchased);
            
            GenerateRandomAbilitySet();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (!HasStateAuthority) return;
            
            EventBus.Unsubscribe<TokenCollectedEvent>(OnTokenCollected);
            EventBus.Unsubscribe<RoundOverEvent>(OnRoundOver);
            EventBus.Unsubscribe<AbilitySelectedEvent>(OnAbilitySelected);
            EventBus.Unsubscribe<AbilityPurchasedEvent>(OnAbilityPurchased);
        }

        #region Utilities
        private void GenerateRandomAbilitySet() => 
            _randomAbilitySet = ListUtility.Shuffle(database.allAbilities);

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_ShowAbilities(PlayerRef chaser, int[] choiceIndices)
        {
            if (chaser != Runner.LocalPlayer) return;
            if (!HasStateAuthority)
            {
                _abilityChoices.Clear();
                foreach (var index in choiceIndices) 
                    _abilityChoices.Add(database.allAbilities[index]);
            }
            _abilityUIManager.ShowAbilities(_abilityChoices);
        }
        #endregion
        
        #region Event Bus Handlers
        private void OnTokenCollected(TokenCollectedEvent ev)
        {
            if (_isShowingAbilities || 
                ev.CollectedBy.Role != PlayerRole.Chaser) 
                return;

            _isShowingAbilities = true;
            _abilityChoices = _randomAbilitySet.GetRange(AbilityIndex, AbilityChoiceCount);
            AbilityIndex += AbilityChoiceCount;
            if (AbilityIndex + AbilityChoiceCount >= _randomAbilitySet.Count) 
                AbilityIndex = 0;
            
            var choiceIndices =
                _abilityChoices.Select(ability => database.allAbilities.IndexOf(ability)).ToArray();
            RPC_ShowAbilities(ev.Collector, choiceIndices);
        }

        private void OnRoundOver(RoundOverEvent ev)
        {
            AbilityIndex = 0;
            if (_newSet)
                GenerateRandomAbilitySet();
            _newSet = !_newSet;
        }

        private void OnAbilitySelected(AbilitySelectedEvent ev)
        {
            if (ev.Player == null || ev.SelectedAbility < 1 ||
                ev.SelectedAbility > _abilityChoices.Count)
                return;

            var ability = _abilityChoices[ev.SelectedAbility - 1];
            if (ability?.effects == null)
                return;

            foreach (var effect in ability.effects)
            {
                if (effect != null)
                    effect.ApplyEffect(ev.Player);
            }

            _isShowingAbilities = false;
        }

        private void OnAbilityPurchased(AbilityPurchasedEvent ev)
        {
            if (!HasStateAuthority || !ev.Accepted || database == null ||
                ev.Buyer == PlayerRef.None ||
                ev.AbilityIndex < 0 || ev.AbilityIndex >= database.allAbilities.Count)
                return;

            var player = FindObjectsByType<Player>()
                .FirstOrDefault(candidate => candidate != null &&
                    candidate.Object != null &&
                    candidate.Object.InputAuthority == ev.Buyer);
            var ability = database.allAbilities[ev.AbilityIndex];

            if (player == null || ability?.effects == null)
                return;

            foreach (var effect in ability.effects)
            {
                if (effect != null)
                    effect.ApplyEffect(player);
            }
        }
        #endregion
    }
}
