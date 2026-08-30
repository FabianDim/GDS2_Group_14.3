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
        //Movement
        Jump,
        SprintHeld,
        CrouchHeld,
        Crouch,
        
        //Ability Selection
        Ability1, Ability2, Ability3,
        
        //Test Consol
        StartRunPhase
    }
    
    public sealed class PlayerInput : NetworkBehaviour, IBeforeUpdate
    {
        [Header("Settings")]
        [SerializeField] private MenuSettings menuSettings;
        private GameplayInput _accumulatedInput;
        private readonly Vector2Accumulator _lookRotationAccumulator = 
            new(0.02f, true);
        
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
        
        [Space, Header("Test Console")] 
        [SerializeField] private InputActionReference startRunPhase;

        public override void Spawned()
        {
            if (!HasInputAuthority) return;
            Runner.GetComponent<NetworkEvents>()?.OnInput.AddListener(OnInput);

            EnableAction(moveAction);
            EnableAction(jumpAction);
            EnableAction(lookAction);
            EnableAction(sprintAction);
            EnableAction(crouchAction);
            EnableAction(ability1Action);
            EnableAction(ability2Action);
            EnableAction(ability3Action);
            EnableAction(startRunPhase);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            Runner.GetComponent<NetworkEvents>()?.OnInput.RemoveListener(OnInput);

            DisableAction(moveAction);
            DisableAction(jumpAction);
            DisableAction(lookAction);
            DisableAction(sprintAction);
            DisableAction(crouchAction);
            DisableAction(ability1Action);
            DisableAction(ability2Action);
            DisableAction(ability3Action);
            DisableAction(startRunPhase);
        }

        private static void EnableAction(InputActionReference actionReference)
        {
            actionReference?.action?.Enable();
        }

        private static void DisableAction(InputActionReference actionReference)
        {
            actionReference?.action?.Disable();
        }

        void IBeforeUpdate.BeforeUpdate()
        {
            if (!HasInputAuthority) return;
            
            // Accumulate input only if the cursor is locked.
            if (Cursor.lockState != CursorLockMode.Locked)
                return;
            
            //Move and Look
            _accumulatedInput.MoveInput = moveAction?.action?.ReadValue<Vector2>() ?? default;
            var lookValue = lookAction.action.ReadValue<Vector2>();
            var lookSensitivity = Gamepad.current != null ? 
                    menuSettings.gamepadSensitivity : menuSettings.mouseSensitivity;
            var lookRotationDelta = new Vector2(-lookValue.y, lookValue.x) * lookSensitivity / 60f;
            _lookRotationAccumulator.Accumulate(lookRotationDelta);
            
            //Movement Buttons
            _accumulatedInput.Buttons.Set(InputButton.Jump, jumpAction.action.IsPressed());
            _accumulatedInput.Buttons.Set(InputButton.SprintHeld, sprintAction.action.IsPressed());
            _accumulatedInput.Buttons.Set(InputButton.CrouchHeld, crouchAction.action.IsPressed());
            _accumulatedInput.Buttons.Set(InputButton.Crouch, crouchAction.action.IsPressed());
            
            //Ability Selection
            _accumulatedInput.Buttons.Set(InputButton.Ability1, ability1Action.action.IsPressed());
            _accumulatedInput.Buttons.Set(InputButton.Ability2, ability2Action.action.IsPressed());
            _accumulatedInput.Buttons.Set(InputButton.Ability3, ability3Action.action.IsPressed());
            
            //Test Console
            _accumulatedInput.Buttons.Set(InputButton.StartRunPhase, startRunPhase.action.IsPressed());
        }

        private void OnInput(NetworkRunner runner, NetworkInput input)
        {
            // Mouse movement (delta values) is aligned to engine update.
            // To get perfectly smooth interpolated look, we need to align the mouse input with Fusion ticks.
            _accumulatedInput.LookRotationDelta = _lookRotationAccumulator.ConsumeTickAligned(runner);

            // Fusion polls accumulated input. This callback can be executed multiple times in a row if there is a performance spike.
            input.Set(_accumulatedInput);
        }
    }
}
