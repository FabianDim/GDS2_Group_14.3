using _Experimenation.Fraser.Scripts;
using _Experimenation.K.Multiplayer.Scripts;
using _Project.Abilities.Scripts;
using UnityEngine;

namespace _Experimenation.Fabian.Scripts.Abilites
{
    [System.Serializable]
    public class JumpBoostAbility : AbilityEffect
    {
        [SerializeField] private float boostMultiplayer = 0.25f;
        [SerializeField] private float maxJumpForce;
        private PlayerMovement _playerMovement;

        public override void ApplyEffect(Player target)
        {
            _playerMovement = target != null ? target.GetComponent<PlayerMovement>() : null;

            if (_playerMovement == null)
                return;

            _playerMovement.ApplyJumpBoost(
                boostMultiplayer,
                maxJumpForce
            );
        }
    }
}