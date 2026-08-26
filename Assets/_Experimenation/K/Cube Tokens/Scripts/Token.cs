using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using Fusion;
using UnityEngine;

namespace _Experimenation.K.Cube_Tokens.Scripts
{
    public class Token : NetworkBehaviour
    {
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private int tokenValue = 10;

        private bool _collected;

        private void Update()
        {
            transform.Rotate(
                Vector3.up * (rotationSpeed * Time.deltaTime)
            );
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!HasStateAuthority)
                return;

            if (!other.CompareTag("Runner") &&
                !other.CompareTag("Chaser"))
            {
                return;
            }

            if (_collected)
                return;

            _collected = true;

            EventBus.Raise(
                new TokenCollectedEvent(
                    tokenValue,
                    other.tag
                )
            );

            Runner.Despawn(Object);
        }
    }
}