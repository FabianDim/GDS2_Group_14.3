using _Experimenation.K.Multiplayer.Scripts;
using _Project.Abilities.Scripts;
using UnityEngine;
namespace _Experimenation.Fabian.Scripts.Abilites
{
    public class GrenadeAbility : AbilityEffect
    {
        public override void ApplyEffect(Player runner)
        {
            if (!runner.TryGetComponent<ThrowGrenade>(out var grenade))
            {
                grenade = UnityEngine.Object.FindAnyObjectByType<ThrowGrenade>();

                grenade.SpawnGrenade();
            }
        }
    }
}