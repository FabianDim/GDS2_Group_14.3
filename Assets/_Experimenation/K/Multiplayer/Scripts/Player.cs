using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using Fusion;
using UnityEngine;

namespace _Experimenation.K.Multiplayer.Scripts
{
    public enum PlayerRole {Runner, Chaser}
    
    public class Player : NetworkBehaviour
    {
        [OnChangedRender(nameof(OnRoleChanged))]
        [Networked] public PlayerRole Role { get; set; }
        [Networked] private NetworkButtons PreviousButtons { get; set; }
        private bool _roundEnded;

        public override void Spawned()
        {
            // OnChangedRender is not guaranteed to run for the initial value,
            // so apply the current role when this instance is spawned as well.
            OnRoleChanged();

            var playerCamera = GetComponentInChildren<Camera>(true);
            var audioListener = GetComponentInChildren<AudioListener>(true);

            var isLocalPlayer = HasInputAuthority;

            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(isLocalPlayer);
                if (isLocalPlayer)
                {
                    playerCamera.tag = "MainCamera";
                    playerCamera.enabled = true;
                }
            }

            if (audioListener != null)
                audioListener.enabled = isLocalPlayer;
        }

        private void OnRoleChanged()
        {
            var meshRenderer = GetComponentInChildren<MeshRenderer>();
            if (meshRenderer == null)
                return;

            meshRenderer.material.color =
                Role == PlayerRole.Runner ? Color.cyan : Color.red;
        }
        
        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority)
                CheckForChaserCapture();

            if (!GetInput(out GameplayInput input)) return;
            HandleAbilitySelection(input);
        }

        private void CheckForChaserCapture()
        {
            if (_roundEnded || Role != PlayerRole.Chaser)
                return;

            var colliders = Physics.OverlapSphere(
                transform.position,
                1f,
                ~0,
                QueryTriggerInteraction.Ignore
            );

            foreach (var collider in colliders)
            {
                var runner = collider.GetComponentInParent<Player>();
                if (runner == null || runner == this || runner.Role != PlayerRole.Runner)
                    continue;

                _roundEnded = true;
                EventBus.Raise(new RoundOverEvent(false));
                return;
            }
        }

        private void HandleAbilitySelection(GameplayInput input)
        {   
            var selectedAbility = 0;
            if(input.Buttons.WasPressed(PreviousButtons, InputButton.Ability1)) 
                selectedAbility = 1;
            if (input.Buttons.WasPressed(PreviousButtons, InputButton.Ability2)) 
                selectedAbility = 2;
            else if (input.Buttons.WasPressed(PreviousButtons, InputButton.Ability3)) 
                selectedAbility = 3;
            
            if(selectedAbility != 0) 
                EventBus.Raise(new AbilitySelectedEvent(selectedAbility));
            
            PreviousButtons = input.Buttons;
        }
    }
}