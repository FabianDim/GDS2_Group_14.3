using NUnit.Framework;
using UnityEngine;

public class SmokeExplosion : MonoBehaviour
{
    [Header("SmokeExplosionPrefab")]
    [SerializeField] private GameObject smokeExplosionEffectPrefab;
    [SerializeField] private Vector3 particleOffset = new Vector3(0, 1, 0);
    [Header("Smoke Settings")]
    [SerializeField] private float smokeExplosionDelay = 3f;
    [SerializeField] private float smokeExplosionForce = 700f;
    [SerializeField] private float explosionRadius = 5f;

    private float countDown;
    private bool hasExploded = false;
    private bool isArmed = false;

    private void Start()
    {
        countDown = smokeExplosionDelay;
    }

    private void Update()
    {
        if (!hasExploded && isArmed)
        {
            countDown -= Time.deltaTime;
            if (countDown <= 0f)
            {
                CreateSmokeExplosion();
                hasExploded = true;
            }
        }
    }
    public void ArmGrenade()
    {
        isArmed = true;
    }
    private void CreateSmokeExplosion()
    {
        GameObject smoke = GameObject.Instantiate(smokeExplosionEffectPrefab, transform.position + particleOffset, Quaternion.identity);

        Destroy(smoke, 10f);

        Destroy(gameObject);
    }
}
