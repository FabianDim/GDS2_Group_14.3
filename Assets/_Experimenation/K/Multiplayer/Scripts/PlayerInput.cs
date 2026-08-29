using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Experimenation.K.Multiplayer.Scripts
{
    public struct GameplayInput : INetworkInput
    {
        //Movement
        public Vector2 MoveInput;
        public Vector2 LookInput;
        public bool UsingGamepadLook;
        public bool Jump;
        public bool SprintHeld;
        public bool CrouchHeld;
        public bool Crouch;
        
        //Ability Selection
        public int SelectedAbility;
        
        //Test Console
        public bool StartRunPhase;
    }
    
    public sealed class PlayerInput : NetworkRunnerCallbacks
    {
        [Header("Movement")]
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
            Runner?.AddCallbacks(this);
            
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
            runner?.RemoveCallbacks(this);
            
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

        public override void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var myInput = new GameplayInput();
            
            //Movement
            myInput.MoveInput = moveAction?.action?.ReadValue<Vector2>() ?? default;
            myInput.LookInput = lookAction?.action?.ReadValue<Vector2>() ?? default;
            myInput.Jump = jumpAction?.action != null && jumpAction.action.ReadValue<float>() > 0.5f;
            myInput.SprintHeld = sprintAction?.action != null && sprintAction.action.IsPressed();
            myInput.CrouchHeld = crouchAction?.action != null && crouchAction.action.IsPressed();
            myInput.Crouch = crouchAction?.action != null && crouchAction.action.IsPressed();
            myInput.UsingGamepadLook = lookAction?.action?.activeControl?.device is Gamepad;

            //Ability Selection
            var selectedAbility = 0;
            if (ability1Action?.action != null && ability1Action.action.WasPressedThisFrame()) selectedAbility = 1;
            else if (ability2Action?.action != null && ability2Action.action.WasPressedThisFrame()) selectedAbility = 2;
            else if (ability3Action?.action != null && ability3Action.action.WasPressedThisFrame()) selectedAbility = 3;
            myInput.SelectedAbility = selectedAbility;
            
            //Test Console
            myInput.StartRunPhase = startRunPhase?.action != null && startRunPhase.action.WasPressedThisFrame();
            
            input.Set(myInput);
        }
    }
}
