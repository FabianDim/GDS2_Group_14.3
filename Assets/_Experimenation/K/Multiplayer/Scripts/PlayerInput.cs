using System.Collections.Generic;
using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Tools.Scripts;
using _Project.Menu.Scripts;
using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Experimenation.K.Multiplayer.Scripts
{
    public struct GameplayInput : INetworkInput
    {
        public Vector2 MoveInput;
        public Vector2 LookRotationDelta;
        public NetworkButtons Buttons;
    }

    public enum InputButton
    {
        Jump, SprintHeld, CrouchHeld, Crouch,
        Ability1, Ability2, Ability3, Ability4,
        ToolSelect, Fire,
        QteFight, CatchUp, CatchDown, CatchLeft, CatchRight,
        StartRunPhase,
    }

    public interface IGameplayInputConsumer
    {
        void ProcessInput(GameplayInput input, NetworkButtons previousButtons);
    }

    public sealed class PlayerInput : NetworkBehaviour, IBeforeUpdate
    {
        [Header("Settings")]
        [SerializeField] private MenuSettings menuSettings;
        private GameplayInput _accumulatedInput;
        private readonly List<IGameplayInputConsumer> _inputConsumers = new();
        [Networked] private NetworkButtons PreviousButtons { get; set; }
        private bool _ownsLocalInput;
        private bool _inputCallbackRegistered;
        private readonly Vector2Accumulator _lookRotationAccumulator = new(0.02f, true);

        [Space, Header("Movement")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference sprintAction;
        [SerializeField] private InputActionReference crouchAction;

        [Space, Header("Ability Selection")]
        [SerializeField] private InputActionReference ability1Action;
        [SerializeField] private InputActionReference ability2Action;
        [SerializeField] private InputActionReference ability3Action;
        [SerializeField] private InputActionReference ability4Action;

        [Space, Header("Tools")]
        [SerializeField] private InputActionReference toolSelectAction;
        [SerializeField] private InputActionReference fireAction;

        [Space, Header("QTE Fight")]
        [SerializeField] private InputActionReference qteFightAction;
        [SerializeField] private InputActionReference catchUpAction;
        [SerializeField] private InputActionReference catchDownAction;
        [SerializeField] private InputActionReference catchLeftAction;
        [SerializeField] private InputActionReference catchRightAction;

        [Space, Header("Test Console")]
        [SerializeField] private InputActionReference startRunPhase;

        public override void Spawned()
        {
            CacheInputConsumers();
            if (!HasInputAuthority)
                return;

            EnableInput();
            SetActionsEnabled(true);
            _ownsLocalInput = true;
            EventBus.Subscribe<BuyZoneEnteredEvent>(OnBuyZoneEntered);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (!_ownsLocalInput)
                return;

            DisableInput();
            SetActionsEnabled(false);
            EventBus.Unsubscribe<BuyZoneEnteredEvent>(OnBuyZoneEntered);
            _ownsLocalInput = false;
        }

        private void CacheInputConsumers()
        {
            _inputConsumers.Clear();
            foreach (var behaviour in GetComponents<MonoBehaviour>())
            {
                if (behaviour is IGameplayInputConsumer consumer)
                    _inputConsumers.Add(consumer);
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!GetInput(out GameplayInput input))
                return;

            var previousButtons = PreviousButtons;
            var toolInputConsumed = ToolSlots.Instance != null &&
                ToolSlots.Instance.ProcessLocalInput(
                    Object != null ? Object.InputAuthority : Runner.LocalPlayer,
                    input,
                    previousButtons);

            foreach (var consumer in _inputConsumers)
            {
                if (consumer is Player && toolInputConsumed)
                    continue;

                consumer.ProcessInput(input, previousButtons);
            }

            PreviousButtons = input.Buttons;
        }

        private void SetActionsEnabled(bool enable)
        {
            SetActionEnabled(moveAction, enable);
            SetActionEnabled(lookAction, enable);
            SetActionEnabled(jumpAction, enable);
            SetActionEnabled(sprintAction, enable);
            SetActionEnabled(crouchAction, enable);
            SetActionEnabled(ability1Action, enable);
            SetActionEnabled(ability2Action, enable);
            SetActionEnabled(ability3Action, enable);
            SetActionEnabled(ability4Action, enable);
            SetActionEnabled(toolSelectAction, enable);
            SetActionEnabled(fireAction, enable);
            SetActionEnabled(startRunPhase, enable);
            SetActionEnabled(qteFightAction, enable);
            SetActionEnabled(catchUpAction, enable);
            SetActionEnabled(catchDownAction, enable);
            SetActionEnabled(catchLeftAction, enable);
            SetActionEnabled(catchRightAction, enable);
        }

        private static void SetActionEnabled(InputActionReference actionReference, bool enabled)
        {
            if (actionReference?.action == null)
                return;

            if (enabled)
                actionReference.action.Enable();
            else
                actionReference.action.Disable();
        }

        private void EnableInput()
        {
            if (_inputCallbackRegistered || Runner == null)
                return;

            var networkEvents = Runner.GetComponent<NetworkEvents>();
            if (networkEvents == null)
                return;

            _accumulatedInput = default;
            networkEvents.OnInput.AddListener(OnInput);
            _inputCallbackRegistered = true;
        }

        private void DisableInput()
        {
            if (!_inputCallbackRegistered || Runner == null)
                return;

            Runner.GetComponent<NetworkEvents>()?.OnInput.RemoveListener(OnInput);
            _inputCallbackRegistered = false;
            _accumulatedInput = default;
        }

        private void MapButton(InputButton button, InputActionReference mapping)
        {
            _accumulatedInput.Buttons.Set(button, mapping?.action != null && mapping.action.IsPressed());
        }

        void IBeforeUpdate.BeforeUpdate()
        {
            if (!HasInputAuthority || Cursor.lockState != CursorLockMode.Locked)
            {
                _accumulatedInput = default;
                return;
            }

            _accumulatedInput.MoveInput = moveAction?.action?.ReadValue<Vector2>() ?? default;

            var lookValue = lookAction?.action?.ReadValue<Vector2>() ?? default;
            var activeControl = lookAction?.action?.activeControl;
            var isGamepad = activeControl is { device: Gamepad };

            var mouseSensitivity = menuSettings != null ? menuSettings.mouseSensitivity : 5f;
            var gamepadSensitivity = menuSettings != null ? menuSettings.gamepadSensitivity : 200f;
            var lookDelta = isGamepad
                ? new Vector2(-lookValue.y, lookValue.x) * gamepadSensitivity * Time.deltaTime
                : new Vector2(-lookValue.y, lookValue.x) * mouseSensitivity / 60f;
            _lookRotationAccumulator.Accumulate(lookDelta);

            MapButton(InputButton.Jump, jumpAction);
            MapButton(InputButton.SprintHeld, sprintAction);
            MapButton(InputButton.CrouchHeld, crouchAction);
            MapButton(InputButton.Crouch, crouchAction);
            MapButton(InputButton.Ability1, ability1Action);
            MapButton(InputButton.Ability2, ability2Action);
            MapButton(InputButton.Ability3, ability3Action);
            MapButton(InputButton.Ability4, ability4Action);
            MapButton(InputButton.ToolSelect, toolSelectAction);
            MapButton(InputButton.Fire, fireAction);
            MapButton(InputButton.QteFight, qteFightAction);
            MapButton(InputButton.CatchUp, catchUpAction);
            MapButton(InputButton.CatchDown, catchDownAction);
            MapButton(InputButton.CatchLeft, catchLeftAction);
            MapButton(InputButton.CatchRight, catchRightAction);
            MapButton(InputButton.StartRunPhase, startRunPhase);
        }

        private void OnInput(NetworkRunner runner, NetworkInput input)
        {
            _accumulatedInput.LookRotationDelta = _lookRotationAccumulator.ConsumeTickAligned(runner);
            input.Set(_accumulatedInput);
        }

        private void OnBuyZoneEntered(BuyZoneEnteredEvent ev)
        {
            if (ev.Entered)
                DisableInput();
            else
                EnableInput();
        }
    }
}
