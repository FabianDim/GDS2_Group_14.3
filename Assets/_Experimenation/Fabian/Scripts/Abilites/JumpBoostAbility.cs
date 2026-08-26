using UnityEngine;
using System.Collections.Generic;
namespace _Experimenation.K.Abilities.Scripts
{
    [System.Serializable]

    public class JumpBoostAbility : AbilityEffect
    {
        public float abilityDuration = 20f;

        public override void ApplyEffect(MonoBehaviour runner)
        {
            if (!runner.TryGetComponent<Jump>(out var jumper))
            {
                jumper = UnityEngine.Object.FindAnyObjectByType<Jump>();
            }

            if (jumper)
            {
                Debug.Log("JumpBoost: found the jump class. Running the ability logic.");
                float baseStrength = jumper.GetJumpStrength();
                jumper.SetJumpStrength(baseStrength * 2f);
                jumper.StartCoroutine(ExecuteAfterDelay(() => { jumper.ResetJumpStrength(); }, abilityDuration));
            }
        }
    }
}
