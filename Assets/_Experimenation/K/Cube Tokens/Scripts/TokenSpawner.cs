using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace _Experimenation.K.Cube_Tokens.Scripts
{
    public class TokenSpawner : MonoBehaviour
    {
        [SerializeField] private float spawnInterval;
        private WaitForSeconds _spawnInterval;
        [SerializeField] private Token tokenPrefab;
        [SerializeField] private int tokensPerSpawn = 5;

        [Header("Pool")]
        [SerializeField] private int poolPrewarmCount = 20;
        [SerializeField] private int poolMaxSize = 200;
        private ObjectPool<Token> _tokenPool;

        [Space, Header("Spawn Placement")]
        [SerializeField] private float spawnDistance = 10f;
        [SerializeField] private float surfaceSearchRadius = 4f;
        [SerializeField] private float surfaceOffset = 0.5f;
        [SerializeField] private LayerMask surfaceMask = ~0;
        [SerializeField] private int maxSpawnAttempts = 20;
        private Transform _player;

        private const float SkinWidth = 0.1f;
        private const float BuriedThreshold = 0.01f;
        private readonly Collider[] _surfaceBuffer = new Collider[32];

        private void Awake()
        {
            _player = GameObject.FindGameObjectWithTag("Runner").transform;
            _spawnInterval = new WaitForSeconds(spawnInterval);

            _tokenPool = new ObjectPool<Token>(
                CreateToken,
                actionOnRelease: token => token.gameObject.SetActive(false),
                actionOnDestroy: token => Destroy(token.gameObject),
                defaultCapacity: poolPrewarmCount,
                maxSize: poolMaxSize);

            Prewarm();
        }

        private Token CreateToken()
        {
            var token = Instantiate(tokenPrefab, transform);
            token.SetPool(_tokenPool);
            token.gameObject.SetActive(false);
            return token;
        }

        // Pay the instantiation cost up front so the first spawns do not hitch.
        private void Prewarm()
        {
            var prewarmed = new Token[poolPrewarmCount];
            for (var i = 0; i < poolPrewarmCount; i++) prewarmed[i] = _tokenPool.Get();
            foreach (var token in prewarmed) _tokenPool.Release(token);
        }

        private void OnDestroy() => _tokenPool?.Dispose();

        private IEnumerator Start()
        {
            while (true)
            {
                yield return _spawnInterval;
                for (var i = 0; i < tokensPerSpawn; i++)
                    if (TryGetSpawnPoint(out var spawnPosition, out var spawnRotation))
                        _tokenPool.Get().Spawn(spawnPosition, spawnRotation);
            }
        }

        private bool TryGetSpawnPoint(out Vector3 position, out Quaternion rotation)
        {
            for (var attempt = 0; attempt < maxSpawnAttempts; attempt++)
            {
                if (TrySnapToNearestSurface(GetRandomCandidate(), out position, out rotation))
                    return true;
            }

            position = default;
            rotation = Quaternion.identity;
            return false;
        }

        // Any direction around the player, always spawnDistance away.
        private Vector3 GetRandomCandidate() => _player.position + Random.onUnitSphere * spawnDistance;

        private bool TrySnapToNearestSurface(Vector3 candidate, out Vector3 position, out Quaternion rotation)
        {
            position = default;
            rotation = Quaternion.identity;

            var count = Physics.OverlapSphereNonAlloc(candidate, surfaceSearchRadius, _surfaceBuffer, surfaceMask,
                QueryTriggerInteraction.Ignore);
            if (count == 0) return false;

            var bestDistance = float.PositiveInfinity;
            var bestPoint = Vector3.zero;

            for (var i = 0; i < count; i++)
            {
                var closestPoint = _surfaceBuffer[i].ClosestPoint(candidate);
                var distance = Vector3.Distance(candidate, closestPoint);

                // ClosestPoint hands back the input point when it is inside the collider, so the
                // candidate is buried in geometry - discard it rather than snap to a wrong face.
                if (distance < BuriedThreshold) return false;

                if (distance >= bestDistance) continue;
                bestDistance = distance;
                bestPoint = closestPoint;
            }

            // ClosestPoint gives no orientation, so cast into the winning surface to read its normal.
            var direction = (bestPoint - candidate).normalized;
            var normal = -direction;
            var surfacePoint = bestPoint;

            if (Physics.Raycast(candidate, direction, out var hit, bestDistance + SkinWidth, surfaceMask,
                    QueryTriggerInteraction.Ignore))
            {
                normal = hit.normal;
                surfacePoint = hit.point;
            }

            position = surfacePoint + normal * surfaceOffset;
            rotation = Quaternion.FromToRotation(Vector3.up, normal);
            return true;
        }

        private void OnDrawGizmosSelected()
        {
            if (_player == null) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_player.position, spawnDistance);
        }
    }
}
