using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Experimenation.K.Multiplayer.Scripts
{
    public struct GameplayInput : INetworkInput
    {
        //Movement
        public Vector2 MoveDirection;
        public bool Jump;
        
        //Ability Selection
        public int SelectedAbility;
    }
    
    public sealed class PlayerInput : NetworkRunnerCallbacks
    {
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference ability1Action;
        [SerializeField] private InputActionReference ability2Action;
        [SerializeField] private InputActionReference ability3Action;
        
        public override void Spawned()
        {
            Runner?.AddCallbacks(this);
            
            moveAction.action.Enable();
            jumpAction.action.Enable();
            ability1Action.action.Enable();
            ability2Action.action.Enable();
            ability3Action.action.Enable();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            runner?.RemoveCallbacks(this);
            
            moveAction.action.Disable();
            jumpAction.action.Disable();
            ability1Action.action.Disable();
            ability2Action.action.Disable();
            ability3Action.action.Disable();
        }

        public override void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var myInput = new GameplayInput();
            
            //Movement
            myInput.MoveDirection = moveAction.action.ReadValue<Vector2>();
            myInput.Jump = jumpAction.action.ReadValue<float>() > 0.5f;

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
