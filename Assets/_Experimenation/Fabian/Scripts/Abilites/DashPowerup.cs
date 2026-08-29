using System;
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
        public float boostSpeed = 100f;
        public float boostDuration = 3f;

        private Func<float> _speedOverride;
        public override void ApplyEffect(Player target)
        {
            // if (!Runner.TryGetComponent<PlayerMovement>(out var movement))
            // {
            //     movement = FindAnyObjectByType<PlayerMovement>();
            // }
            //
            // if (movement != null)
            // {
            //     Debug.Log($"DashPowerup: Adding the boost of {{ {boostSpeed} }}");
            //     _speedOverride = () => boostSpeed;
            //     movement.GetSpeedOverrideList().Add(_speedOverride);
            //     Debug.Log("DashPowerup: First person movement object found in the scene. Starting coroutine.");
            //     movement.StartCoroutine(ExecuteAfterDelay(() => { movement.GetSpeedOverrideList().Remove(_speedOverride); }, boostDuration));
            // }
            // else
            // {
            //     Debug.LogError("DashPowerup: No FirstPersonMovement found in scene.");
            // }
        }
    }
}