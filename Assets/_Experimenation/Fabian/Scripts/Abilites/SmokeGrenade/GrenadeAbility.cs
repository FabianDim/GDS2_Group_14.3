using _Experimenation.K.Abilities.Scripts;
using UnityEngine;
namespace _Experimenation.K.Abilities.Scripts
{
    public class GrenadeAbility : AbilityEffect
    {
        public override void ApplyEffect(MonoBehaviour runner)
        {
            if (!runner.TryGetComponent<ThrowGrenade>(out var grenade))
            {
                grenade = UnityEngine.Object.FindAnyObjectByType<ThrowGrenade>();

                grenade.SpawnGrenade();
            }
        }
    }
}