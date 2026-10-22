using System;
using _Experimenation.K.Multiplayer.Scripts;
using _Project.Abilities.Scripts;
using UnityEngine;

namespace _Experimenation.Fabian.Scripts.Abilites
{
    [Serializable]
    public class BananaPeelPowerup : AbilityEffect
    {
        [SerializeField] private GameObject bananaPeelPrefab;

        public override void ApplyEffect(Player target)
        {
            if (bananaPeelPrefab == null)
                throw new InvalidOperationException("Banana peel prefab is not assigned.");

            var position = target.transform.position - target.transform.forward * 1.5f;
            if (Physics.Raycast(position + Vector3.up, Vector3.down, out var hit, 10f))
                position = hit.point;
            target.Runner.Spawn(bananaPeelPrefab, position + Vector3.up * 0.1f,
                Quaternion.identity, target.Object.InputAuthority);
        }
    }
}
