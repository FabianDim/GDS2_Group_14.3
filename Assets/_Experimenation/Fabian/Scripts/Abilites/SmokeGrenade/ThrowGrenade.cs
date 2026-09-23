using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using UnityEngine;

namespace _Experimenation.Fabian.Scripts.Abilites.SmokeGrenade
{
    public class ThrowGrenade : NetworkBehaviour, IGameplayInputConsumer
    {
        [SerializeField] private GameObject grenadePrefab;
        [SerializeField] private Transform throwPosition;
        [SerializeField] private Vector3 throwDirection = new Vector3(0, 1, 0);
        [SerializeField] private float maxForce = 10f;

        private Camera _mainCamera;
        private GameObject _grenadeObject;

        public override void Spawned()
        {
            _mainCamera = Camera.main;
        }

        public void ProcessInput(GameplayInput input, NetworkButtons previousButtons)
        {
            if (_grenadeObject == null)
                return;

            if (throwPosition != null)
            {
                _grenadeObject.transform.position = throwPosition.position;
                _grenadeObject.transform.rotation = throwPosition.rotation;
            }

            if (input.Buttons.WasPressed(previousButtons, InputButton.Fire))
                ThrowGrenadeFunc(maxForce, _grenadeObject);
        }

        public void SpawnGrenade()
        {
            if (!HasStateAuthority)
                return;

            if (grenadePrefab == null)
            {
                UnityEngine.Debug.LogError("ThrowGrenade: 'grenadePrefab' is missing in Inspector!");
                return;
            }
            if (_grenadeObject != null) return;

            Transform spawnPoint = throwPosition != null ? throwPosition : transform;
            _grenadeObject = Instantiate(grenadePrefab, spawnPoint.position, spawnPoint.rotation);

            if (_grenadeObject.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = true;
            }
        }

        private void ThrowGrenadeFunc(float force, GameObject grenade)
        {
            if (_mainCamera == null) _mainCamera = Camera.main;

            if (grenade.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = false;

                Vector3 finalThrowDirection = (_mainCamera.transform.forward + throwDirection).normalized;
                rb.AddForce(finalThrowDirection * force, ForceMode.VelocityChange);
            }

            if (grenade.TryGetComponent<SmokeExplosion>(out var smokeEffect))
            {
                smokeEffect.ArmGrenade();
            }
            _grenadeObject = null;
        }
    }
}