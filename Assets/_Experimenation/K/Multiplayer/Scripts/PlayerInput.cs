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
    }
    
    public sealed class PlayerInput : NetworkRunnerCallbacks
    {
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference sprintAction;
        [SerializeField] private InputActionReference crouchAction;
        [SerializeField] private InputActionReference ability1Action;
        [SerializeField] private InputActionReference ability2Action;
        [SerializeField] private InputActionReference ability3Action;
        
        public override void Spawned()
        {
            Runner?.AddCallbacks(this);
            
            moveAction.action.Enable();
            jumpAction.action.Enable();
            lookAction.action.Enable();
            sprintAction.action.Enable();
            crouchAction.action.Enable();
            ability1Action.action.Enable();
            ability2Action.action.Enable();
            ability3Action.action.Enable();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            runner?.RemoveCallbacks(this);
            
            moveAction.action.Disable();
            jumpAction.action.Disable();
            lookAction.action.Disable();
            sprintAction.action.Disable();
            crouchAction.action.Disable();
            ability1Action.action.Disable();
            ability2Action.action.Disable();
            ability3Action.action.Disable();
        }

        public override void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var myInput = new GameplayInput();
            
            //Movement
            myInput.MoveInput = moveAction.action.ReadValue<Vector2>();
            myInput.LookInput = lookAction.action.ReadValue<Vector2>();
            myInput.Jump = jumpAction.action.ReadValue<float>() > 0.5f;
            myInput.SprintHeld = sprintAction.action.IsPressed();
            myInput.CrouchHeld = crouchAction.action.IsPressed();
            myInput.Crouch = crouchAction.action.IsPressed();
            myInput.UsingGamepadLook = lookAction.action.activeControl?.device is Gamepad;

            //Ability Selection
            var selectedAbility = 0;
            if(ability1Action.action.IsPressed()) selectedAbility = 1;
            else if(ability2Action.action.IsPressed()) selectedAbility = 2;
            else if(ability3Action.action.IsPressed()) selectedAbility = 3;
            myInput.SelectedAbility = selectedAbility;
            
            input.Set(myInput);
        }
    }
}
