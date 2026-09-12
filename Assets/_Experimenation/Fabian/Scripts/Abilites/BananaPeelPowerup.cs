using _Experimenation.Fraser.Scripts;
using _Experimenation.K.Multiplayer.Scripts;
using _Project.Abilities.Scripts;

namespace _Experimenation.Fabian.Scripts.Abilites
{
    [System.Serializable]
    public class BananaPeelPowerup : AbilityEffect
    {
        private PlayerMovement _playerMovement;

        public override void ApplyEffect(Player target)
        {
            _playerMovement = target != null ? target.GetComponent<PlayerMovement>() : null;
        }
    }
}
