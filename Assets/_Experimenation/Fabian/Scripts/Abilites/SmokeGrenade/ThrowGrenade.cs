using Fusion;
using UnityEngine;

public class ThrowGrenade : NetworkBehaviour
{
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private Transform throwPosition;
    [SerializeField] private Vector3 throwDirection = new Vector3(0, 1, 0);
    [SerializeField] private float maxForce = 10f;

    private readonly KeyCode throwKey = KeyCode.Mouse0;
    private Camera mainCamera;
    private GameObject grenadeObject;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    public override void FixedUpdateNetwork()
    {
        if (grenadeObject != null)
        {
            if (throwPosition != null)
            {
                grenadeObject.transform.position = throwPosition.position;
                grenadeObject.transform.rotation = throwPosition.rotation;
            }

            if (Input.GetKeyUp(throwKey))
            {
                ThrowGrenadeFunc(maxForce, grenadeObject);
            }
        }
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
        if (grenadeObject != null) return;

        Transform spawnPoint = throwPosition != null ? throwPosition : transform;
        grenadeObject = Instantiate(grenadePrefab, spawnPoint.position, spawnPoint.rotation);

        if (grenadeObject.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
        }
    }

    private void ThrowGrenadeFunc(float force, GameObject grenade)
    {
        if (mainCamera == null) mainCamera = Camera.main;

        if (grenade.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;

            Vector3 finalThrowDirection = (mainCamera.transform.forward + throwDirection).normalized;
            rb.AddForce(finalThrowDirection * force, ForceMode.VelocityChange);
        }

        if (grenade.TryGetComponent<SmokeExplosion>(out var smokeEffect))
        {
            smokeEffect.ArmGrenade();
        }
        grenadeObject = null;
    }
}