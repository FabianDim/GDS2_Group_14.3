using _Experimenation.K.Multiplayer.Scripts;
using _Project.Abilities.Scripts;

namespace _Experimenation.Fabian.Scripts.Abilites.SmokeGrenade
{
    public class GrenadeAbility : AbilityEffect
    {
        public override void ApplyEffect(Player target)
        {
            if (target.TryGetComponent<ThrowGrenade>(out var grenade)) return;
            grenade = UnityEngine.Object.FindAnyObjectByType<ThrowGrenade>();
            grenade.SpawnGrenade();
        }
    }
}