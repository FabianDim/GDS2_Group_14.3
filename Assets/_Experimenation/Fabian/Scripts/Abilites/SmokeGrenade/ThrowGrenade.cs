using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using UnityEngine;

namespace _Experimenation.Fabian.Scripts.Abilites.SmokeGrenade
{
    public class ThrowGrenade : NetworkBehaviour
    {
        [SerializeField] private GameObject grenadePrefab;
        [SerializeField] private Transform throwPosition;
        [SerializeField] private Vector3 throwDirection = new Vector3(0, 1, 0);
        [SerializeField] private float maxForce = 10f;

        [Networked] private NetworkButtons PreviousButtons { get; set; }
        private Camera _mainCamera;
        private GameObject _grenadeObject;

        private void Start()
        {
            _mainCamera = Camera.main;
        }

        public override void FixedUpdateNetwork()
        {
            if (_grenadeObject == null || !GetInput(out GameplayInput input)) return;
        
            if (throwPosition != null)
            {
                _grenadeObject.transform.position = throwPosition.position;
                _grenadeObject.transform.rotation = throwPosition.rotation;
            }

            if (input.Buttons.WasPressed(PreviousButtons, InputButton.Fire))
            {
                ThrowGrenadeFunc(maxForce, _grenadeObject);
            }
        
            PreviousButtons = input.Buttons;
        }

        public void SpawnGrenade()
        {
            if (!HasStateAuthority)
                return;

            if (grenadePrefab == null)
            {
                Debug.LogError("ThrowGrenade: 'grenadePrefab' is missing in Inspector!");
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