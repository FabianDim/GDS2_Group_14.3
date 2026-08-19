using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using UnityEngine;
using UnityEngine.Pool;

namespace _Experimenation.K.Cube_Tokens.Scripts
{
    public class Token : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 0.0001f;
        [SerializeField] private int tokenValue = 10;

        private IObjectPool<Token> _pool;
        private bool _collected;

        public void SetPool(IObjectPool<Token> pool) => _pool = pool;

        public void Spawn(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            _collected = false;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            transform.Rotate(Vector3.up * rotationSpeed);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Runner") && !other.CompareTag("Chaser")) return;

            // A trigger can fire again before the object deactivates. Releasing the same instance
            // twice would leave it in the pool twice, so two Gets would hand out the same token.
            if (_collected) return;
            _collected = true;

            EventBus.Raise(new PointChangeEvent(tokenValue));

            if (_pool != null) _pool.Release(this);
            else Destroy(gameObject);
        }
    }
}
