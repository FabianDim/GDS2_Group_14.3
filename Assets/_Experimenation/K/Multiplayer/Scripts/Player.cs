using Fusion;
using Fusion.Addons.SimpleKCC;
using _Experimenation.K.Game_Manager.Abilities.Scripts;
using UnityEngine;

namespace _Experimenation.K.Multiplayer.Scripts
{
    public enum PlayerRole {Runner, Chaser}
    
    public class Player : NetworkBehaviour
    {
        [OnChangedRender(nameof(OnRoleChanged))]
        [Networked] public PlayerRole Role { get; set; }

        private SimpleKCC _simpleKcc;
        private AbilityRoundState _abilityRoundState;
        private int _previousSelectedAbility;

        public override void Spawned()
        {
            _simpleKcc = GetComponent<SimpleKCC>();
            _simpleKcc.SetGravity(-25.0f);

            // OnChangedRender is not guaranteed to run for the initial value,
            // so apply the current role when this instance is spawned as well.
            OnRoleChanged();

            var playerCamera = GetComponentInChildren<Camera>(true);
            var audioListener = GetComponentInChildren<AudioListener>(true);

            var isLocalPlayer = HasInputAuthority;

            if (playerCamera != null)
                playerCamera.gameObject.SetActive(isLocalPlayer);

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
            if (!GetInput<GameplayInput>(out var input)) return;

            if (HasStateAuthority && input.SelectedAbility != _previousSelectedAbility)
            {
                if (input.SelectedAbility is >= 1 and <= 3)
                {
                    _abilityRoundState ??= FindFirstObjectByType<AbilityRoundState>();
                    _abilityRoundState?.TrySelectAbility(this, input.SelectedAbility - 1);
                }

                _previousSelectedAbility = input.SelectedAbility;
            }

            // Set default world space velocity and jump impulse.
            var moveVelocity = _simpleKcc.TransformRotation * new Vector3(input.MoveDirection.x, 0.0f, input.MoveDirection.y) * 10.0f;
            float jumpImpulse  = 0;

            if (input.Jump && _simpleKcc.IsGrounded)
            {
                // Set upward jump impulse.
                jumpImpulse = 10.0f;
            }

            _simpleKcc.Move(moveVelocity, jumpImpulse);
        }
    }
}
