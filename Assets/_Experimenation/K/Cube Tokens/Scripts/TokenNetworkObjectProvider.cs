using System.Collections.Generic;
using Fusion;

namespace _Experimenation.K.Cube_Tokens.Scripts
{
    /// <summary>
    /// Reuses despawned Token instances without passing an already initialized
    /// NetworkObject directly to Runner.Spawn().
    /// </summary>
    public class TokenNetworkObjectProvider : NetworkObjectProviderDefault
    {
        private readonly Queue<NetworkObject> _tokenPool = new();

        protected override NetworkObject InstantiatePrefab(
            NetworkRunner runner,
            NetworkObject prefab)
        {
            if (prefab.GetComponent<Token>() == null || _tokenPool.Count <= 0)
                return Instantiate(prefab);
            var instance = _tokenPool.Dequeue();
            instance.gameObject.SetActive(true);
            return instance;
        }

        public override void ReleaseInstance(
            NetworkRunner runner,
            in NetworkObjectReleaseContext context)
        {
            var instance = context.Object;
            var isToken = instance != null && instance.GetComponent<Token>() != null;

            if (!isToken || !context.TypeId.IsPrefab)
            {
                base.ReleaseInstance(runner, context);
                return;
            }

            if (context.IsBeingDestroyed)
            {
                Destroy(instance.gameObject);
            }
            else
            {
                instance.gameObject.SetActive(false);
                _tokenPool.Enqueue(instance);
            }

            // Fusion tracks provider-owned prefab instances and expects this count
            // to be decremented even when the object is pooled instead of destroyed.
            runner.Prefabs.RemoveInstance(context.TypeId.AsPrefabId);
        }
    }
}
