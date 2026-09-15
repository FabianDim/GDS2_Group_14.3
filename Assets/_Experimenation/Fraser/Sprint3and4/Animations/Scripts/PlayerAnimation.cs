using Fusion;
using UnityEngine;

namespace _Experimenation.Fraser.Scripts
{
    public class PlayerAnimation : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private Animator thirdPersonAnimator;
        [SerializeField] private GameObject thirdPersonVisual;

        [Header("Movement Speeds")]
        [SerializeField] private float walkingSpeed = 10f;
        [SerializeField] private float sprintingSpeed = 15f;
        [SerializeField] private float animatorDampTime = 0.1f;

        [Networked]
        private float NetworkedAnimationSpeed { get; set; }

        private static readonly int SpeedHash =
            Animator.StringToHash("Speed");

        public override void Spawned()
        {
            if (playerMovement == null)
                playerMovement = GetComponent<PlayerMovement>();

            if (thirdPersonVisual != null)
                thirdPersonVisual.SetActive(!HasInputAuthority);
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || playerMovement == null)
                return;

            NetworkedAnimationSpeed =
                ConvertSpeedToAnimatorValue(
                    playerMovement.HorizontalSpeed
                );
        }

        public override void Render()
        {
            if (thirdPersonAnimator == null)
                return;

            float animationSpeed = NetworkedAnimationSpeed;

            if (HasInputAuthority &&
                playerMovement != null)
            {
                animationSpeed =
                    ConvertSpeedToAnimatorValue(
                        playerMovement.HorizontalSpeed
                    );
            }

            thirdPersonAnimator.SetFloat(
                SpeedHash,
                animationSpeed,
                animatorDampTime,
                Time.deltaTime
            );
        }

        private float ConvertSpeedToAnimatorValue(
            float horizontalSpeed)
        {
            if (horizontalSpeed <= walkingSpeed)
            {
                return Mathf.InverseLerp(
                    0f,
                    walkingSpeed,
                    horizontalSpeed
                ) * 0.5f;
            }

            return Mathf.Lerp(
                0.5f,
                1f,
                Mathf.InverseLerp(
                    walkingSpeed,
                    sprintingSpeed,
                    horizontalSpeed
                )
            );
        }
    }
}