
using Fusion;
using UnityEngine;

public class BananaPeelNetworkSpawner : NetworkBehaviour
{
    [SerializeField] public bool testSpawnBananaPeel;
    [SerializeField] private GameObject bananaPeelPrefab;
    [SerializeField] private Vector3 Offset = new Vector3(-1, 2, 0);
    private bool hasTestSpawned;

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (!testSpawnBananaPeel)
        {
            hasTestSpawned = false;
            return;
        }

        if (hasTestSpawned)
            return;

        hasTestSpawned = true;
        SpawnTheBanana(transform.position);
    }

    public void SpawnTheBanana(Vector3 groundCoords)
    {
        if (!HasStateAuthority)
        {
            return;
        }
        if (!bananaPeelPrefab)
        {
            Debug.LogError("Banana peel must be asigned.");
            return;
        }

        Runner.Spawn(bananaPeelPrefab, groundCoords + Offset, Quaternion.identity);
    }
}
