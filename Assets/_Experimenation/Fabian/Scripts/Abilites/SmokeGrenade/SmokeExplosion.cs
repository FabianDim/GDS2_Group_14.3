using Fusion;
using UnityEngine;

public class SmokeExplosion : NetworkBehaviour
{
    [Header("Smoke Explosion")]
    [SerializeField] private GameObject smokeExplosionEffectPrefab;
    [SerializeField] private Vector3 particleOffset = new Vector3(0, 1, 0);

    [Header("Smoke Settings")]
    [SerializeField] private float smokeExplosionDelay = 3f;

    private const float SmokeDuration = 10f;

    [Networked] private TickTimer FuseTimer { get; set; }
    [Networked] private TickTimer SmokeTimer { get; set; }
    [Networked] private NetworkBool HasExploded { get; set; }
    [Networked] private Vector3 ExplosionPosition { get; set; }

    private bool explosionVisualsPlayed;
    private GameObject smoke;

    public override void Spawned()
    {
        explosionVisualsPlayed = false;
        // Only the host simulates the projectile; NetworkTransform replicates it.
        if (TryGetComponent<Rigidbody>(out var body))
            body.isKinematic = !HasStateAuthority;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (HasExploded)
        {
            if (SmokeTimer.Expired(Runner))
                Runner.Despawn(Object);
            return;
        }

        if (FuseTimer.Expired(Runner))
            Explode();
    }

    public void ArmGrenade()
    {
        if (!HasStateAuthority || HasExploded || FuseTimer.IsRunning)
            return;

        FuseTimer = TickTimer.CreateFromSeconds(Runner, smokeExplosionDelay);
    }

    private void Explode()
    {
        ExplosionPosition = transform.position + particleOffset;
        HasExploded = true;
        SmokeTimer = TickTimer.CreateFromSeconds(Runner, SmokeDuration);
        HideGrenade();
    }

    public override void Render()
    {
        // Keep the explosion in replicated state for the smoke's lifetime.
        // An RPC on a grenade despawned in the same tick can be lost by clients.
        if (!HasExploded || explosionVisualsPlayed)
            return;

        explosionVisualsPlayed = true;
        HideGrenade();

        if (smokeExplosionEffectPrefab == null)
        {
            Debug.LogError("SmokeExplosion: assign Smoke Explosion Effect Prefab on the grenade prefab.", this);
            return;
        }

        float remaining = SmokeTimer.RemainingTime(Runner) ?? 0f;
        if (remaining <= 0f)
            return;

        smoke = Instantiate(
            smokeExplosionEffectPrefab,
            ExplosionPosition,
            smokeExplosionEffectPrefab.transform.rotation
        );
        Runner.MoveToRunnerScene(smoke);
        smoke.SetActive(true);

        foreach (var particles in smoke.GetComponentsInChildren<ParticleSystem>())
            particles.Play();

        Destroy(smoke, remaining);
    }

    private void HideGrenade()
    {
        foreach (var mesh in GetComponentsInChildren<MeshRenderer>())
            mesh.enabled = false;

        foreach (var collider in GetComponentsInChildren<Collider>())
            collider.enabled = false;

        if (TryGetComponent<Rigidbody>(out var body))
            body.isKinematic = true;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (smoke != null)
            Destroy(smoke);
    }
}
