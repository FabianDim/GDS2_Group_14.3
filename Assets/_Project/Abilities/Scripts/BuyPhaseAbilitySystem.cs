using _Experimenation.Fabian.Scripts.PhaseController;
using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Game_Manager.Scripts;
using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using UnityEngine;

namespace _Project.Abilities.Scripts
{
    /// <summary>
    /// State-authoritative purchase handler used during the Buy Phase.
    /// Tool abilities are intentionally not applied as immediate effects.
    /// </summary>
    public sealed class BuyPhaseAbilitySystem : NetworkBehaviour
    {
        [SerializeField] private AbilityDatabase database;

        private PhaseManager _phaseManager;
        private bool _subscribed;

        public override void Spawned()
        {
            _phaseManager = GetComponentInChildren<PhaseManager>(true);

            if (!HasStateAuthority)
                return;

            EventBus.Subscribe<AbilityPurchasedEvent>(OnAbilityPurchased);
            _subscribed = true;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (!_subscribed)
                return;

            EventBus.Unsubscribe<AbilityPurchasedEvent>(OnAbilityPurchased);
            _subscribed = false;
        }

        private bool IsBuyPhase => _phaseManager != null && !_phaseManager.IsRunPhase;

        private void OnAbilityPurchased(AbilityPurchasedEvent ev)
        {
            if (!IsBuyPhase || ev is not { Accepted: true } ||
                database == null || database.allAbilities == null || !ev.Buyer.IsValid ||
                ev.AbilityIndex < 0 || ev.AbilityIndex >= database.allAbilities.Count)
            {
                return;
            }

            var ability = database.allAbilities[ev.AbilityIndex];
            if (ability == null)
                return;

            if (!GameManager.SpawnedPlayers.TryGetValue(ev.Buyer, out var playerObject) ||
                playerObject == null ||
                !playerObject.TryGetComponent<Player>(out var player))
            {
                Debug.LogWarning(
                    $"BuyPhaseAbilitySystem could not process '{ability.abilityName}' " +
                    "because the buyer's player object is unavailable.");
                return;
            }

            if (ability.abilityType == AbilityType.Tool)
                return;

            AbilityEffectApplier.ApplyAll(ability, player);
        }
    }
}
