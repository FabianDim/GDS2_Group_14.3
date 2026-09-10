using Fusion;
using UnityEngine;

public class SmokeExplosion : NetworkBehaviour
{
    [Header("Smoke Explosion")]
    [SerializeField] private GameObject smokeExplosionEffectPrefab;
    [SerializeField] private Vector3 particleOffset = new Vector3(0, 1, 0);

    [Header("Smoke Settings")]
    [SerializeField] private float smokeExplosionDelay = 3f;

    private float countDown;
    private bool hasExploded;
    private bool isArmed;

    public override void Spawned()
    {
        countDown = smokeExplosionDelay;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (!isArmed || hasExploded)
            return;

        countDown -= Runner.DeltaTime;

        if (countDown <= 0f)
        {
            Explode();
        }
    }

    public void ArmGrenade()
    {
        if (!HasStateAuthority)
            return;

        isArmed = true;
    }

    private void Explode()
    {
        if (hasExploded)
            return;

        hasExploded = true;

        PlaySmokeExplosionRpc();

        Runner.Despawn(Object);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void PlaySmokeExplosionRpc()
    {
        GameObject smoke = Instantiate(
            smokeExplosionEffectPrefab,
            transform.position + particleOffset,
            Quaternion.identity
        );

        Destroy(smoke, 10f);
    }
}