using _Experimenation.Fraser.Scripts;
using _Experimenation.K.Multiplayer.Scripts;
using _Project.Abilities.Scripts;
using UnityEngine;

namespace _Experimenation.Fabian.Scripts.Abilites
{
    [System.Serializable]
    public class DashPowerup : AbilityEffect
    {
        [Header("Settings")]
        public float boostMultiplier = 0.25f;
        private PlayerMovement _playerMovement;

        public override void ApplyEffect(Player target)
        {
            _playerMovement = target != null ? target.GetComponent<PlayerMovement>() : null;

            if (_playerMovement == null)
            {
                Debug.LogError("DashPowerup: No FirstPersonMovement found in scene.");
                return;
            }

            _playerMovement.ApplyDashBoost(boostMultiplier);
        }
    }
}