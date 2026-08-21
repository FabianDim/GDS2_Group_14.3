using System;
using System.Collections;
using _Experimenation.K.Abilities.Scripts;
using UnityEngine;

namespace _Experimenation.K.Abilities.Scripts
{
    [System.Serializable]
    public class DashPowerup : AbilityEffect
    {
        [Header("Settings")]
        public float boostSpeed = 1999999f;
        public float boostDuration = 3f;

        float SpeedOverride() => boostSpeed;
        public override void ApplyEffect(MonoBehaviour runner)
        {


            if (!runner.TryGetComponent<FirstPersonMovement>(out var movement))
            {
                movement = UnityEngine.Object.FindAnyObjectByType<FirstPersonMovement>();
            }

            if (movement != null)
            {
                Debug.Log($"DashPowerup: Adding the boost of {{ {boostSpeed} }}");
                movement.GetSpeedOverrideList().Add(SpeedOverride);
                Debug.Log("DashPowerup: First person movement object found in the scene. Starting coroutine.");
                runner.StartCoroutine(RemoveEffectAfterTime(movement));
            }
            else
            {
                Debug.LogError("DashPowerup: No FirstPersonMovement found in scene.");
            }
        }

        private IEnumerator RemoveEffectAfterTime(FirstPersonMovement movement)
        {
            yield return new WaitForSeconds(boostDuration);
            movement.GetSpeedOverrideList().Remove(SpeedOverride);
        }


    }
}