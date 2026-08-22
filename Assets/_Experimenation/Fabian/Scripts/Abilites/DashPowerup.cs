using System;
using System.Collections;
using System.Threading.Tasks;
using _Experimenation.K.Abilities.Scripts;
using UnityEngine;

namespace _Experimenation.K.Abilities.Scripts
{
    [System.Serializable]
    public class DashPowerup : AbilityEffect
    {
        [Header("Settings")]
        public float boostSpeed = 100f;
        public float boostDuration = 3f;

        private Func<float> SpeedOverride;
        public override void ApplyEffect(MonoBehaviour runner)
        {


            if (!runner.TryGetComponent<FirstPersonMovement>(out var movement))
            {
                movement = UnityEngine.Object.FindAnyObjectByType<FirstPersonMovement>();
            }

            if (movement != null)
            {
                Debug.Log($"DashPowerup: Adding the boost of {{ {boostSpeed} }}");
                SpeedOverride = () => boostSpeed;
                movement.GetSpeedOverrideList().Add(SpeedOverride);
                Debug.Log("DashPowerup: First person movement object found in the scene. Starting coroutine.");
                movement.StartCoroutine(RemoveEffectAfterTime(movement));
            }
            else
            {
                Debug.LogError("DashPowerup: No FirstPersonMovement found in scene.");
            }
        }

        private IEnumerator RemoveEffectAfterTime(FirstPersonMovement movement)
        {
            Debug.Log("DashPowerup: Coroutine running.");
            yield return new WaitForSeconds(boostDuration);
            Debug.Log("DashPowerup: Coroutine wait for seconds ran.");
            movement.GetSpeedOverrideList().Remove(SpeedOverride);

        }


    }
}