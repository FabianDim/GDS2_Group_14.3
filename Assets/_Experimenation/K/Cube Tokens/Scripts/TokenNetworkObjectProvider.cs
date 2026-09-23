using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace _Experimenation.K.Cube_Tokens.Scripts
{
    /// <summary>
    /// Pools token instances while safely discarding instances Unity destroyed during
    /// a scene transition or runner shutdown.
    /// </summary>
    public class TokenNetworkObjectProvider : NetworkObjectProviderDefault
    {
        private readonly Queue<NetworkObject> _tokenPool = new();
        private readonly HashSet<NetworkObject> _pooledTokens = new();

        protected override NetworkObject InstantiatePrefab(
            NetworkRunner runner,
            NetworkObject prefab)
        {
            if (prefab == null || prefab.GetComponent<Token>() == null)
                return prefab == null ? null : Instantiate(prefab);

            while (_tokenPool.Count > 0)
            {
                var instance = _tokenPool.Dequeue();
                _pooledTokens.Remove(instance);

                // Unity overrides == for destroyed objects, so this also rejects
                // objects that look non-null as plain C# references.
                if (instance == null || instance.gameObject == null)
                    continue;

                if (instance.GetComponent<Token>() == null)
                {
                    Destroy(instance.gameObject);
                    continue;
                }

                instance.gameObject.SetActive(true);
                return instance;
            }

            return Instantiate(prefab);
        }

        public override void ReleaseInstance(
            NetworkRunner runner,
            in NetworkObjectReleaseContext context)
        {
            var instance = context.Object;
            if (instance == null || !context.TypeId.IsPrefab ||
                instance.GetComponent<Token>() == null)
            {
                base.ReleaseInstance(runner, context);
                return;
            }

            if (context.IsBeingDestroyed)
            {
                _pooledTokens.Remove(instance);
                Destroy(instance.gameObject);
            }
            else if (!_pooledTokens.Contains(instance))
            {
                instance.gameObject.SetActive(false);
                _tokenPool.Enqueue(instance);
                _pooledTokens.Add(instance);
            }

            // CreatePrefabInstance increments this count. Fusion expects it to be
            // decremented for pooled objects as well as destroyed objects.
            if (runner != null)
                runner.Prefabs.RemoveInstance(context.TypeId.AsPrefabId);
        }

        private void OnDestroy()
        {
            ClearPool();
        }

        private void ClearPool()
        {
            while (_tokenPool.Count > 0)
            {
                var instance = _tokenPool.Dequeue();
                if (instance != null)
                    Destroy(instance.gameObject);
            }

            _pooledTokens.Clear();
        }
    }
}
