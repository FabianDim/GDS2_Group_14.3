using _Experimenation.Fabian.Scripts.PhaseController;
using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Game_Manager.Scripts;
using Fusion;

namespace _Project.Abilities.Scripts
{
    /// <summary>Activates only the requesting player's purchased tools.</summary>
    public sealed class RunPhaseAbilitySystem : NetworkBehaviour
    {
        private PhaseManager _phaseManager;

        public override void Spawned()
        {
            _phaseManager = GetComponentInChildren<PhaseManager>(true);
            if (HasStateAuthority)
                EventBus.Subscribe<AbilitySelectedEvent>(OnAbilitySelected);
        }

        public override void Despawned(NetworkRunner runner, bool hasState) => Unsubscribe();
        private void OnDestroy() => Unsubscribe();
        private void Unsubscribe() => EventBus.Unsubscribe<AbilitySelectedEvent>(OnAbilitySelected);

        private void OnAbilitySelected(AbilitySelectedEvent ev)
        {
            var data = GameData.Instance;
            if (!HasStateAuthority || _phaseManager == null || !_phaseManager.IsRunPhase ||
                data == null || ev?.Player == null || ev.Player.Object == null ||
                !ev.Player.HasStateAuthority || ev.Player.Runner != Runner ||
                ev.SelectedAbility < 1 || ev.SelectedAbility > 4)
                return;

            var index = data.GetToolAbilityIndex(ev.Player.Object.InputAuthority, ev.SelectedAbility - 1);
            if (index >= 0)
                AbilityEffectApplier.ApplyAll(data.AbilityDatabase.allAbilities[index], ev.Player);
        }
    }
}
