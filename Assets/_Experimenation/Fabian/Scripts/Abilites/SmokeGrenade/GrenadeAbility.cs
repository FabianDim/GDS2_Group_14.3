using System;
using _Experimenation.K.Multiplayer.Scripts;
using _Project.Abilities.Scripts;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

namespace _Experimenation.Fabian.Scripts.Abilites.SmokeGrenade
{
    [Serializable]
    public sealed class GrenadeAbility : AbilityEffect
    {
        [SerializeField] private GameObject grenadePrefab;

        public override void ApplyEffect(Player target)
        {
            if (grenadePrefab == null)
                throw new InvalidOperationException("Smoke grenade prefab is not assigned.");

            // Use this player's replicated aim, including when the host handles
            // the client's input. Camera.main would aim with the host's camera.
            var kcc = target.GetComponent<SimpleKCC>();
            var look = kcc.GetLookRotation(true, true);
            var direction = Quaternion.Euler(look.x, look.y, 0) * Vector3.forward;
            var grenade = target.Runner.Spawn(grenadePrefab,
                target.transform.position + Vector3.up * 1.4f + direction,
                Quaternion.identity, target.Object.InputAuthority);
            var body = grenade.GetComponent<Rigidbody>();
            body.isKinematic = false;
            body.AddForce((direction + Vector3.up * 0.25f).normalized * 10f, ForceMode.VelocityChange);
            grenade.GetComponent<SmokeExplosion>().ArmGrenade();
        }
    }
}
