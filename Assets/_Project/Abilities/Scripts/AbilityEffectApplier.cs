using System;
using _Experimenation.K.Multiplayer.Scripts;
using UnityEngine;

namespace _Project.Abilities.Scripts
{
    /// <summary>
    /// Applies an ability's serialized effects on the State Authority.
    /// </summary>
    public static class AbilityEffectApplier
    {
        public static void ApplyAll(Ability ability, Player player)
        {
            if (ability == null || player == null || player.Object == null ||
                !player.HasStateAuthority || ability.effects == null)
                return;

            foreach (var effect in ability.effects)
            {
                if (effect == null)
                    continue;

                try
                {
                    effect.ApplyEffect(player);
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        $"Failed to apply an effect from '{ability.abilityName}' " +
                        $"to '{player.name}': {exception}");
                }
            }
        }
    }
}
