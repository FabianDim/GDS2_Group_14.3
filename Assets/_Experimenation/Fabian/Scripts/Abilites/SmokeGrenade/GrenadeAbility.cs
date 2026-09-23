using _Experimenation.K.Multiplayer.Scripts;
using _Project.Abilities.Scripts;
using UnityEngine;

namespace _Experimenation.Fabian.Scripts.Abilites.SmokeGrenade
{
    public sealed class GrenadeAbility : AbilityEffect
    {
        public override void ApplyEffect(Player target)
        {
            if (target == null)
                return;

            // The effect belongs to the purchased player's Player object. Never
            // search globally, otherwise the host could activate another player's
            // grenade component.
            if (!target.TryGetComponent<ThrowGrenade>(out var grenade))
            {
                UnityEngine.Debug.LogWarning($"{target.name} has no ThrowGrenade component.", target);
                return;
            }

            grenade.SpawnGrenade();
        }
    }
}