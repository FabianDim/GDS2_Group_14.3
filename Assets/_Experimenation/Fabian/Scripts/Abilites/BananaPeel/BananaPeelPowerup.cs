using _Experimenation.Fraser.Scripts;
using _Experimenation.K.Multiplayer.Scripts;
using _Project.Abilities.Scripts;
using UnityEngine;

namespace _Experimenation.Fabian.Scripts.Abilites
{
    [System.Serializable]
    public class BananaPeelPowerup : AbilityEffect
    {
        RaycastHit hit;

        private PlayerMovement _playerMovement;


        public override void ApplyEffect(Player target)
        {
            _playerMovement = target != null ? target.GetComponent<PlayerMovement>() : null;

            var closestGround = RayCastForGround();

            var bananaPeelSpawner = target != null
                ? target.GetComponent<BananaPeelNetworkSpawner>()
                : null;

            if (bananaPeelSpawner == null)
            {
                Debug.LogError("BananaPeelPowerup: Player is missing BananaPeelNetworkSpawner.");
                return;
            }

            bananaPeelSpawner.SpawnTheBanana(closestGround);

            return;
        }

        public Vector3 RayCastForGround()
        {
            if (_playerMovement == null)
                return Vector3.zero;

            Ray ray = new Ray(_playerMovement.transform.position, Vector3.down);
            return Physics.Raycast(ray, out hit) ? hit.point : Vector3.zero;
        }
    }

}
