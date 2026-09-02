using _Experimenation.K.Game_Manager.Scripts;
using Fusion;
using UnityEngine;

namespace _Experimenation.K.Cube_Tokens.Scripts
{
    public class Token : NetworkBehaviour
    {
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private int tokenValue = 10;

        private void Update() => 
            transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime));

        private void OnTriggerEnter(Collider other) =>
            RPCHandler.Instance.GetCollected(other, tokenValue, Object);
    }
}
