
using Fusion;
using _Experimenation.Fraser.Scripts;
using _Experimenation.K.Multiplayer.Scripts;
using UnityEngine;

public class BananaPeelObj : NetworkBehaviour
{
    [SerializeField, Range(0.1f, 1f)] private float speedMultiplier = 0.5f;
    [SerializeField] private float hindranceDuration = 3f;

    private void OnCollisionEnter(Collision collision)
    {
        if (!HasStateAuthority)
            return;

        var player = collision.collider.GetComponentInParent<Player>();
        if (player == null)
            return;

        var playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement == null)
            return;

        playerMovement.ApplySpeedHindrance(speedMultiplier, hindranceDuration);
    }
}
