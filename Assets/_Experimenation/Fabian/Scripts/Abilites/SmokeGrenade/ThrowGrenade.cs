using UnityEngine;

public class ThrowGrenade : MonoBehaviour
{
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private Transform throwPosition;
    [SerializeField] private Vector3 throwDirection = new Vector3(0, 1, 0);
    [SerializeField] private float maxForce = 10f;

    private KeyCode throwKey = KeyCode.Mouse0;
    private Camera mainCamera;
    private GameObject grenadeObject;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (grenadeObject != null)
        {
            // Keep the grenade locked to the throw position while holding
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
        if (grenadePrefab == null)
        {
            Debug.LogError("ThrowGrenade: 'grenadePrefab' is missing in Inspector!");
            return;
        }

        // Prevent spawning multiple active grenades at once
        if (grenadeObject != null) return;

        Transform spawnPoint = throwPosition != null ? throwPosition : transform;

        // Instantiate at exact throw position
        grenadeObject = Instantiate(grenadePrefab, spawnPoint.position, spawnPoint.rotation);

        // Disable physics temporarily so it doesn't drop to the ground
        if (grenadeObject.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
        }
    }

    private void ThrowGrenadeFunc(float force, GameObject grenade)
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // Re-enable physics before applying force
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