using System.Collections.Generic;
using System.Linq;
using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Game_Manager.Scripts;
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
        private List<Ability> _abilityChoices = new();
        private bool _isShowingAbilities;
        private bool _newSet;
        private int _abilityIndex;
        private PlayerRole _localPlayerRole;

        public override void Spawned()
        {
            if (!HasStateAuthority) return;
            
            EventBus.Subscribe<AllPlayersSpawnedEvent>(OnAllPlayersSpawned);
            EventBus.Subscribe<TokenCollectedEvent>(OnTokenCollected);
            EventBus.Subscribe<RoundOverEvent>(OnRoundOver);
            EventBus.Subscribe<AbilitySelectedEvent>(OnAbilitySelected);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (!HasStateAuthority) return;
            
            EventBus.Unsubscribe<AllPlayersSpawnedEvent>(OnAllPlayersSpawned);
            EventBus.Unsubscribe<TokenCollectedEvent>(OnTokenCollected);
            EventBus.Unsubscribe<RoundOverEvent>(OnRoundOver);
            EventBus.Unsubscribe<AbilitySelectedEvent>(OnAbilitySelected);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_ShowAbilities()
        {
            if (!_localPlayerRole.Equals(PlayerRole.Chaser)) return;
            _abilityUIManager.ShowAbilities(_abilityChoices);
        }

        private void OnAllPlayersSpawned(AllPlayersSpawnedEvent ev)
        {
            _abilityUIManager = GetComponent<AbilityUIManager>();
            _randomAbilitySet = ListUtility.Shuffle(database.allAbilities);
            _localPlayerRole = 
                FindObjectsByType<Player>().First(p => p.HasInputAuthority).Role;
        }

        private void OnTokenCollected(TokenCollectedEvent ev)
        {
            if (_isShowingAbilities || 
                !ev.CollectedBy.Equals(PlayerRole.Chaser)) 
                return;

            _isShowingAbilities = true;
            _abilityChoices = _randomAbilitySet.GetRange(_abilityIndex, 3);
            _abilityIndex += 3;
            if (_abilityIndex + 3 >= _randomAbilitySet.Count) 
                _abilityIndex = 0;
            RPC_ShowAbilities();
        }

        private void OnRoundOver(RoundOverEvent ev)
        {
            if (!_newSet) return;
            _abilityIndex = 0;
            _newSet = !_newSet;
            _randomAbilitySet = ListUtility.Shuffle(database.allAbilities);
        }

        private void OnAbilitySelected(AbilitySelectedEvent ev)
        {
            foreach(var effect in _abilityChoices[ev.SelectedAbility - 1].effects)
                effect.ApplyEffect();
            _isShowingAbilities = false;
        }
    }
}
