using System;
using System.Collections.Generic;
using _Experimenation.Fabian.Scripts.PhaseController;
using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Game_Manager.Scripts;
using _Experimenation.K.Multiplayer.Scripts;
using _Project.Abilities.Scripts;
using Fusion;
using UnityEngine;
using UnityEngine.UI;

namespace _Experimenation.K.Tools.Scripts
{
    /// <summary>
    /// Stores each player's purchased tools and validates tool activation on the
    /// State Authority. This is a scene NetworkObject, so the inventory is
    /// explicitly keyed by PlayerRef instead of being shared between players.
    /// </summary>
    [RequireComponent(typeof(Animator), typeof(NetworkObject))]
    public sealed class ToolSlots : NetworkBehaviour
    {
        private const int MaxToolSlots = 4;

        public static ToolSlots Instance { get; private set; }

        [Header("References")]
        [SerializeField] private AbilityDatabase abilityDatabase;
        [SerializeField] private Transform slots;
        [SerializeField] private string selectParam = "Select";

        private readonly List<Ability> _localTools = new();
        private readonly Dictionary<PlayerRef, List<Ability>> _authoritativeTools = new();

        private Animator _animator;
        private PhaseManager _phaseManager;
        private string _resolvedSelectParam;
        private bool _subscribed;
        private bool _isSelecting;

        public override void Spawned()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Duplicate ToolSlots scene object detected; disabling this copy.", this);
                enabled = false;
                return;
            }

            Instance = this;
            _animator = GetComponent<Animator>();
            _phaseManager = transform.root.GetComponentInChildren<PhaseManager>(true);

            ResolveReferences();
            ResolveAnimatorParameter();

            EventBus.Subscribe<AbilityPurchasedEvent>(OnAbilityPurchased);
            _subscribed = true;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            UnsubscribeFromEvents();
            ClearState();

            if (Instance == this)
                Instance = null;
        }

        private void OnDestroy()
        {
            // Covers scene teardown paths where Fusion cannot invoke Despawned.
            UnsubscribeFromEvents();

            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// Processes one local input snapshot and returns whether a tool consumed
        /// one of the shared ability buttons. PlayerInput uses the return value
        /// only for the Player component on this same networked player.
        /// </summary>
        public bool ProcessLocalInput(
            PlayerRef inputOwner,
            GameplayInput input,
            NetworkButtons previousButtons)
        {
            if (!IsLocalOwner(inputOwner) || !IsRunPhase())
                return false;

            var consumed = false;

            if (WasPressed(input, previousButtons, InputButton.ToolSelect))
            {
                SetSelecting(!_isSelecting);
                consumed = true;
            }

            var selectedSlot = GetSelectedSlot(input, previousButtons);
            if (selectedSlot < 0)
                return consumed;

            // Ability buttons are shared with Run Phase ability selection. Only
            // consume one when this player actually owns a tool in that slot.
            if (!_isSelecting && !HasLocalTool(selectedSlot))
                return consumed;

            if (!RequestToolActivation(selectedSlot))
                return consumed;

            SetSelecting(false);
            return true;
        }

        private static int GetSelectedSlot(GameplayInput input, NetworkButtons previousButtons)
        {
            if (WasPressed(input, previousButtons, InputButton.Ability1))
                return 0;

            if (WasPressed(input, previousButtons, InputButton.Ability2))
                return 1;

            if (WasPressed(input, previousButtons, InputButton.Ability3))
                return 2;

            if (WasPressed(input, previousButtons, InputButton.Ability4))
                return 3;

            return -1;
        }

        private bool IsLocalOwner(PlayerRef inputOwner)
        {
            return Runner != null && Runner.IsRunning &&
                   Runner.LocalPlayer.IsValid && inputOwner == Runner.LocalPlayer;
        }

        private bool RequestToolActivation(int slotIndex)
        {
            if (!HasLocalTool(slotIndex))
                return false;

            if (HasStateAuthority)
                return ActivateAuthoritatively(Runner.LocalPlayer, slotIndex);

            RPC_ActivateTool(slotIndex);
            return true;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        private void RPC_ActivateTool(int slotIndex, RpcInfo info = default)
        {
            if (!HasStateAuthority || info.Source == PlayerRef.None)
                return;

            ActivateAuthoritatively(info.Source, slotIndex);
        }

        private bool ActivateAuthoritatively(PlayerRef playerRef, int slotIndex)
        {
            if (!IsValidActivationRequest(playerRef, slotIndex, out var ability, out var player))
                return false;

            AbilityEffectApplier.ApplyAll(ability, player);
            return true;
        }

        private bool IsValidActivationRequest(
            PlayerRef playerRef,
            int slotIndex,
            out Ability ability,
            out Player player)
        {
            ability = null;
            player = null;

            if (!HasStateAuthority || !playerRef.IsValid || !IsRunPhase())
                return false;

            if (!_authoritativeTools.TryGetValue(playerRef, out var tools) ||
                slotIndex < 0 || slotIndex >= tools.Count)
            {
                return false;
            }

            ability = tools[slotIndex];
            if (ability == null || ability.abilityType != AbilityType.Tool)
                return false;

            if (GameManager.SpawnedPlayers.TryGetValue(playerRef, out var playerObject) &&
                playerObject != null &&
                playerObject.TryGetComponent(out player) &&
                player.HasStateAuthority &&
                player.Object != null &&
                player.Object.InputAuthority == playerRef) return true;

            Debug.LogWarning($"ToolSlots could not resolve the authoritative player {playerRef}.");
            ability = null;
            player = null;
            return false;

        }

        private bool IsRunPhase()
        {
            // Tool activation is valid only during the Run Phase. A missing phase
            // manager is allowed for isolated component tests.
            return _phaseManager == null || _phaseManager.IsRunPhase;
        }

        private void OnAbilityPurchased(AbilityPurchasedEvent ev)
        {
            if (ev is not { Accepted: true } || !ev.Buyer.IsValid ||
                !TryGetAbility(ev.AbilityIndex, out var ability) ||
                ability.abilityType != AbilityType.Tool)
            {
                return;
            }

            if (HasStateAuthority)
            {
                if (!_authoritativeTools.TryGetValue(ev.Buyer, out var tools))
                {
                    tools = new List<Ability>(MaxToolSlots);
                    _authoritativeTools.Add(ev.Buyer, tools);
                }

                TryAddTool(tools, ability, ev.Buyer);
            }

            // Purchase results are broadcast to every peer. Only the buyer builds
            // the local UI list; the other player's tools stay private.
            if (Runner != null && ev.Buyer == Runner.LocalPlayer)
                TryAddTool(_localTools, ability, ev.Buyer);
        }

        private bool TryAddTool(List<Ability> tools, Ability ability, PlayerRef owner)
        {
            if (tools == null || ability == null || tools.Contains(ability))
                return false;

            if (tools.Count >= MaxToolSlots)
            {
                Debug.LogWarning(
                    $"ToolSlots cannot add '{ability.abilityName}' for {owner}: all slots are full.");
                return false;
            }

            tools.Add(ability);
            if (ReferenceEquals(tools, _localTools))
                UpdateLocalSlotIcon(tools.Count - 1, ability);

            return true;
        }

        private bool HasLocalTool(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < _localTools.Count && _localTools[slotIndex] != null;
        }

        private bool TryGetAbility(int abilityIndex, out Ability ability)
        {
            ability = null;
            return abilityDatabase != null && abilityDatabase.allAbilities != null &&
                   abilityIndex >= 0 && abilityIndex < abilityDatabase.allAbilities.Count &&
                   (ability = abilityDatabase.allAbilities[abilityIndex]) != null;
        }

        private void ResolveReferences()
        {
            if (slots != null)
                return;

            for (var index = 0; index < transform.childCount; index++)
            {
                var child = transform.GetChild(index);
                if (string.Equals(child.name, "Slots", StringComparison.OrdinalIgnoreCase))
                {
                    slots = child;
                    return;
                }
            }

            Debug.LogWarning("ToolSlots could not find a child named 'Slots'.", this);
        }

        private void ResolveAnimatorParameter()
        {
            if (_animator == null)
                return;

            foreach (var parameter in _animator.parameters)
            {
                if (!string.Equals(parameter.name, selectParam, StringComparison.OrdinalIgnoreCase)) continue;
                _resolvedSelectParam = parameter.name;
                return;
            }

            Debug.LogWarning($"ToolSlots animator has no '{selectParam}' parameter.", this);
        }

        private void SetSelecting(bool selecting)
        {
            _isSelecting = selecting;
            if (_animator != null && !string.IsNullOrEmpty(_resolvedSelectParam))
                _animator.SetBool(_resolvedSelectParam, selecting);
        }

        private void UpdateLocalSlotIcon(int slotIndex, Ability ability)
        {
            if (slots == null || ability == null || slotIndex < 0 || slotIndex >= slots.childCount)
                return;

            var image = slots.GetChild(slotIndex).GetComponentInChildren<Image>(true);
            if (image != null)
                image.sprite = ability.abilitySprite;
        }

        private void UnsubscribeFromEvents()
        {
            if (!_subscribed)
                return;

            EventBus.Unsubscribe<AbilityPurchasedEvent>(OnAbilityPurchased);
            _subscribed = false;
        }

        private void ClearState()
        {
            _localTools.Clear();
            _authoritativeTools.Clear();
            _isSelecting = false;
        }

        private static bool WasPressed(
            GameplayInput input,
            NetworkButtons previousButtons,
            InputButton button)
        {
            return input.Buttons.WasPressed(previousButtons, button);
        }
    }
}
