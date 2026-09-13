using _Experimenation.K.Game_Manager.Scripts;
using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using UnityEngine;

namespace _Experimenation.K.Cube_Tokens.Scripts
{
    public class Token : NetworkBehaviour
    {
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private int tokenValue = 10;

        [Space, Header("Lifetime")]
        [SerializeField, Min(1f)] private float lifetime = 15f;

        [SerializeField, Min(0.1f)] private float collectionRadius = 1.25f;

        // Host-side live token count, used by TokenSpawner as a population cap.
        public static int LiveTokens { get; private set; }

        /// <summary>Called on runner shutdown so the static count never leaks into a new match.</summary>
        public static void ResetLiveCount() => LiveTokens = 0;

        [Networked] private TickTimer LifeTimer { get; set; }

        public override void Spawned()
        {
            if (!HasStateAuthority)
                return;

            LiveTokens++;

            // Set unconditionally: pooled instances keep their previous
            // (expired) timer, so an IsRunning guard would never restart it.
            LifeTimer = TickTimer.CreateFromSeconds(Runner, lifetime);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (hasState)
                LiveTokens = Mathf.Max(0, LiveTokens - 1);
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || !LifeTimer.IsRunning || !LifeTimer.Expired(Runner))
            {
                if (HasStateAuthority && !IsCollected)
                    TryCollectNearbyPlayer();
                return;
            }

            Runner.Despawn(Object);
        }

        [Networked] private NetworkBool IsCollected { get; set; }

        private void TryCollectNearbyPlayer()
        {
            if (IsCollected || RPCHandler.Instance == null)
                return;

            foreach (var pair in GameManager.SpawnedPlayers)
            {
                var playerObject = pair.Value;
                if (playerObject == null ||
                    !playerObject.TryGetComponent<Player>(out var player))
                    continue;

                if (Vector3.Distance(transform.position, player.transform.position) > collectionRadius)
                    continue;

                Collect(player);
                return;
            }
        }

        private void Collect(Player player)
        {
            if (IsCollected || player == null || player.Object == null)
                return;

            IsCollected = true;
            RPCHandler.Instance.GetCollected(player, tokenValue, Object);
        }

        private void Update() =>
            transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime));

        private void OnTriggerEnter(Collider other)
        {
            if (!HasStateAuthority || IsCollected)
                return;

            var player = other.GetComponentInParent<Player>();
            if (player != null)
                Collect(player);
        }
    }
}
