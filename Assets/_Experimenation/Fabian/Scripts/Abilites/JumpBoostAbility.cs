using _Experimenation.Fraser.Scripts;
using _Experimenation.K.Multiplayer.Scripts;
using _Project.Abilities.Scripts;
using Fusion;
using UnityEngine;

namespace _Experimenation.Fabian.Scripts.Abilites
{
    [System.Serializable]

    public class JumpBoostAbility : AbilityEffect
    {
        [SerializeField] private float abilityDuration = 20f;
        [SerializeField] private float boostMultiplayer = 2f;
        [SerializeField] private float maxJumpForce;
        private PlayerMovement _playerMovement;
        private TickTimer _timer;

        public override void ApplyEffect(Player target)
        {
            _playerMovement = target != null ? target.GetComponent<PlayerMovement>() : null;
            if (_playerMovement == null)
                return;

            var defaultJumpForce = _playerMovement.jumpForce;
            _playerMovement.jumpForce = Mathf.Min(maxJumpForce, defaultJumpForce * boostMultiplayer);
                
            target.StartCoroutine(EndEffect(() => { _playerMovement.jumpForce = defaultJumpForce; }));
        }
    }
}
