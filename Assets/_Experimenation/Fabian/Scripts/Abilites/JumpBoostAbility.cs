using UnityEngine;
namespace _Experimenation.K.Abilities.Scripts
{
    [System.Serializable]

    public class JumpBoostAbility : AbilityEffect
    {
        public float jumpStrength = 100f;
        public float abilityDuration = 3f;
        public override void ApplyEffect(MonoBehaviour runner)
        {
            if (!runner.TryGetComponent<Jump>(out var jumper))
            {
                jumper = UnityEngine.Object.FindAnyObjectByType<Jump>();
            }

            if (jumper)
            {
                jumper.
            }
        }
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
