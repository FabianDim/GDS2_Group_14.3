using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using Fusion;
using TargetIndicators.Samples;
using UnityEngine;

namespace _Experimenation.K.Multiplayer.Scripts
{
    public enum PlayerRole { Runner = 1, Chaser = 2 }

    public class Player : NetworkBehaviour, IGameplayInputConsumer
    {
        [OnChangedRender(nameof(OnRoleChanged))]
        [Networked] public PlayerRole Role { get; set; }

        // Local references to the two replicated players; populated as they spawn.
        public Player OtherPlayer { get; private set; }

        private CompassTapeTargetIndicatorsSetupExample _compassSetup;

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

                    var compass = FindAnyObjectByType<TargetIndicators.TargetIndicatorManager>(FindObjectsInactive.Include);

                    if (compass != null)
                    {
                        compass.Camera = playerCamera;
                        compass.CompassForwardReferenceOverride = null;
                    }
                }
            }

            if (audioListener != null)
                audioListener.enabled = isLocalPlayer;

            if (isLocalPlayer)
                _compassSetup = FindAnyObjectByType<CompassTapeTargetIndicatorsSetupExample>(FindObjectsInactive.Include);

            // The second player to spawn links both sides, regardless of spawn order.
            foreach (var player in FindObjectsByType<Player>())
            {
                if (player == this || player.Object == null || !player.Object.IsValid)
                    continue;

                Debug.Log($"Found the second player: [{player.name}]");

                OtherPlayer = player;
                player.OtherPlayer = this;
                player.UpdateCompassTarget();
                break;
            }

            UpdateCompassTarget();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (_compassSetup != null)
                _compassSetup.SetTarget(null);

            if (OtherPlayer != null && OtherPlayer.OtherPlayer == this)
            {
                OtherPlayer.OtherPlayer = null;
                OtherPlayer.UpdateCompassTarget();
            }

            OtherPlayer = null;
            _compassSetup = null;
        }

        private void UpdateCompassTarget()
        {
            if (OtherPlayer == null)
            {
                Debug.Log($"The other player is NULL");
            }
            // A null target also clears the marker when the other player despawns.
            if (_compassSetup != null)
                _compassSetup.SetTarget(OtherPlayer != null ? OtherPlayer.transform : null);
        }

        private void OnRoleChanged()
        {
            var meshRenderer = GetComponentInChildren<MeshRenderer>();
            if (meshRenderer == null)
                return;

            meshRenderer.material.color =
                Role == PlayerRole.Runner ? Color.cyan : Color.red;
        }

        public void ProcessInput(GameplayInput input, NetworkButtons previousButtons)
        {
            var selectedAbility = 0;
            if (input.Buttons.WasPressed(previousButtons, InputButton.Ability1))
                selectedAbility = 1;
            else if (input.Buttons.WasPressed(previousButtons, InputButton.Ability2))
                selectedAbility = 2;
            else if (input.Buttons.WasPressed(previousButtons, InputButton.Ability3))
                selectedAbility = 3;
            else if (input.Buttons.WasPressed(previousButtons, InputButton.Ability4))
                selectedAbility = 4;

            if (selectedAbility != 0)
            {
                EventBus.Raise(new AbilitySelectedEvent(selectedAbility, this));
            }
        }

    }
}
