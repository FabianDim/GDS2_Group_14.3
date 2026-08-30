using _Project.Abilities.Scripts;
using Fusion;
using Fusion.Addons.SimpleKCC;
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

            if (isLocalPlayer)
                Debug.Log($"Player camera enabled for local player {Object.InputAuthority}.");
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
            if (!GetInput(out GameplayInput input)) return;
            HandleAbilitySelection(input);
        }

        private void HandleAbilitySelection(GameplayInput input)
        {
            if (input.SelectedAbility is < 1 or > 3)
                return;

            _abilityRoundState ??= FindAnyObjectByType<AbilityRoundState>();
            if (_abilityRoundState == null)
                return;

            _abilityRoundState.TrySelectAbility(this, input.SelectedAbility - 1);
        }
    }
}
