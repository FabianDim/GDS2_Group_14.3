using System.Linq;
using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using UnityEngine;

namespace _Experimenation.K.Cube_Tokens.Scripts
{
    public class TokenSpawner : NetworkBehaviour
    {
        [SerializeField] private float spawnInterval = 5f;
        [SerializeField] private Token tokenPrefab;
        [SerializeField] private int tokensPerSpawn = 5;

        [Space, Header("Spawn Placement")]
        [SerializeField] private float spawnDistance = 10f;
        [SerializeField] private float surfaceSearchRadius = 4f;
        [SerializeField] private float surfaceOffset = 0.5f;
        [SerializeField] private LayerMask surfaceMask = ~0;
        [SerializeField] private int maxSpawnAttempts = 20;

        private Transform _runner;
        private TickTimer _spawnTimer;

        private const float SkinWidth = 0.1f;
        private const float BuriedThreshold = 0.01f;

        private readonly Collider[] _surfaceBuffer = new Collider[32];

        public override void Spawned()
        {
            if (!HasStateAuthority)
                return;

            _runner = FindObjectsByType<Player>()
                .FirstOrDefault(player => player.Role == PlayerRole.Runner)
                ?.transform;

            _spawnTimer = TickTimer.CreateFromSeconds(
                Runner,
                spawnInterval
            );
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || !_spawnTimer.Expired(Runner))
                return;
            
            _spawnTimer = TickTimer.CreateFromSeconds(
                Runner,
                spawnInterval
            );
            SpawnTokens();
        }

        private void SpawnTokens()
        {
            for (var i = 0; i < tokensPerSpawn; i++)
            {
                if (!TryGetSpawnPoint(out var spawnPosition, out var spawnRotation))
                    continue;

                Runner.Spawn(
                    tokenPrefab,
                    spawnPosition,
                    spawnRotation
                );
            }
        }

        #region Spawning Utilities
        private bool TryGetSpawnPoint(
            out Vector3 position,
            out Quaternion rotation)
        {
            for (var attempt = 0; attempt < maxSpawnAttempts; attempt++)
            {
                if (TrySnapToNearestSurface(
                        GetRandomCandidate(),
                        out position,
                        out rotation))
                {
                    return true;
                }
            }

            position = default;
            rotation = Quaternion.identity;
            return false;
        }

        private Vector3 GetRandomCandidate()
        {
            return _runner.position +
                   Random.onUnitSphere * spawnDistance;
        }

        private bool TrySnapToNearestSurface(
            Vector3 candidate,
            out Vector3 position,
            out Quaternion rotation)
        {
            position = default;
            rotation = Quaternion.identity;

            var count = Physics.OverlapSphereNonAlloc(
                candidate,
                surfaceSearchRadius,
                _surfaceBuffer,
                surfaceMask,
                QueryTriggerInteraction.Ignore
            );

            if (count == 0)
                return false;

            var bestDistance = float.PositiveInfinity;
            var bestPoint = Vector3.zero;

            for (var i = 0; i < count; i++)
            {
                var closestPoint =
                    _surfaceBuffer[i].ClosestPoint(candidate);

                var distance =
                    Vector3.Distance(candidate, closestPoint);

                // ClosestPoint returns the input point when the
                // candidate is inside the collider.
                if (distance < BuriedThreshold)
                    return false;

                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                bestPoint = closestPoint;
            }

            var direction =
                (bestPoint - candidate).normalized;

            var normal = -direction;
            var surfacePoint = bestPoint;

            if (Physics.Raycast(
                    candidate,
                    direction,
                    out var hit,
                    bestDistance + SkinWidth,
                    surfaceMask,
                    QueryTriggerInteraction.Ignore))
            {
                normal = hit.normal;
                surfacePoint = hit.point;
            }

            position =
                surfacePoint + normal * surfaceOffset;

            rotation =
                Quaternion.FromToRotation(
                    Vector3.up,
                    normal
                );

            return true;
        }
        #endregion
    }
}