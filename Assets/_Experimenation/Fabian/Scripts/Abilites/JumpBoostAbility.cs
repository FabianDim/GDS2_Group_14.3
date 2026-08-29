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
        private PlayerMovement _playerMovement;
        private TickTimer _timer;

        public override void ApplyEffect(Player target)
        {
            _playerMovement = target != null ? target.GetComponent<PlayerMovement>() : null;
            if (_playerMovement == null)
                return;

            if ((_playerMovement.jumpForce * boostMultiplayer) <= _playerMovement.maxJumpForce)
            {
                _playerMovement.jumpForce *= boostMultiplayer;
            }
            else
            {
                _playerMovement.jumpForce = _playerMovement.maxJumpForce;
            }
        }
    }
}
