
using Fusion;
using UnityEngine;

public class BananaPeelNetworkSpawner : NetworkBehaviour
{

    [SerializeField] private GameObject bananaPeelPrefab;
    [SerializeField] private Vector3 Offset = new Vector3(-1, 2, 0);

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
