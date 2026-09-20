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
        [SerializeField] private float crouchWalkingSpeed = 5f;
        [SerializeField] private float animatorDampTime = 0.1f;

        [Networked] private float NetworkedAnimationSpeed { get; set; }
        [Networked] private float NetworkedCrouchSpeed { get; set; }
        [Networked] private NetworkBool NetworkedIsSliding { get; set; }
        [Networked] private NetworkBool NetworkedIsCrouching { get; set; }
        [Networked] private NetworkBool NetworkedIsAirborne { get; set; }
        [Networked] private NetworkBool NetworkedJumpAnimationActive { get; set; }
        [Networked] private NetworkBool NetworkedIsClimbing { get; set; }

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int CrouchSpeedHash = Animator.StringToHash("CrouchSpeed");
        private static readonly int IsSlidingHash = Animator.StringToHash("IsSliding");
        private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
        private static readonly int IsAirborneHash = Animator.StringToHash("IsAirborne");
        private static readonly int JumpAnimationActiveHash =
            Animator.StringToHash("JumpAnimationActive");
        private static readonly int IsClimbingHash = Animator.StringToHash("IsClimbing");
        private static readonly int JumpHash = Animator.StringToHash("Jump");

        private int _lastJumpSequence;

        public override void Spawned()
        {
            if (playerMovement == null)
                playerMovement = GetComponent<PlayerMovement>();

            if (playerMovement != null)
                _lastJumpSequence = playerMovement.JumpSequence;

            if (thirdPersonVisual != null)
                thirdPersonVisual.SetActive(!HasInputAuthority);
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || playerMovement == null)
                return;

            float horizontalSpeed = playerMovement.HorizontalSpeed;

            NetworkedAnimationSpeed =
                ConvertSpeedToAnimatorValue(horizontalSpeed);

            NetworkedCrouchSpeed =
                ConvertCrouchSpeedToAnimatorValue(horizontalSpeed);

            NetworkedIsSliding = playerMovement.IsSliding;
            NetworkedIsCrouching = playerMovement.IsCrouching;
            NetworkedIsAirborne = !playerMovement.IsGrounded;
            NetworkedJumpAnimationActive = playerMovement.JumpAnimationActive;
            NetworkedIsClimbing = playerMovement.IsClimbing;
        }

        public override void Render()
        {
            if (thirdPersonAnimator == null)
                return;

            float animationSpeed = NetworkedAnimationSpeed;
            float crouchSpeed = NetworkedCrouchSpeed;
            bool isSliding = NetworkedIsSliding;
            bool isCrouching = NetworkedIsCrouching;
            bool isAirborne = NetworkedIsAirborne;
            bool jumpAnimationActive = NetworkedJumpAnimationActive;
            bool isClimbing = NetworkedIsClimbing;

            if (HasInputAuthority && playerMovement != null)
            {
                float horizontalSpeed = playerMovement.HorizontalSpeed;

                animationSpeed =
                    ConvertSpeedToAnimatorValue(horizontalSpeed);

                crouchSpeed =
                    ConvertCrouchSpeedToAnimatorValue(horizontalSpeed);

                isSliding = playerMovement.IsSliding;
                isCrouching = playerMovement.IsCrouching;
                isAirborne = !playerMovement.IsGrounded;
                jumpAnimationActive = playerMovement.JumpAnimationActive;
                isClimbing = playerMovement.IsClimbing;
            }

            thirdPersonAnimator.SetFloat(
                SpeedHash,
                animationSpeed,
                animatorDampTime,
                Time.deltaTime
            );

            thirdPersonAnimator.SetFloat(
                CrouchSpeedHash,
                crouchSpeed,
                animatorDampTime,
                Time.deltaTime
            );

            thirdPersonAnimator.SetBool(IsSlidingHash, isSliding);
            thirdPersonAnimator.SetBool(IsCrouchingHash, isCrouching);
            thirdPersonAnimator.SetBool(IsAirborneHash, isAirborne);
            thirdPersonAnimator.SetBool(
                JumpAnimationActiveHash,
                jumpAnimationActive
            );
            thirdPersonAnimator.SetBool(IsClimbingHash, isClimbing);

            if (playerMovement != null &&
                playerMovement.JumpSequence != _lastJumpSequence)
            {
                _lastJumpSequence = playerMovement.JumpSequence;
                thirdPersonAnimator.SetTrigger(JumpHash);
            }
        }

        private float ConvertSpeedToAnimatorValue(float horizontalSpeed)
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

        private float ConvertCrouchSpeedToAnimatorValue(float horizontalSpeed)
        {
            return Mathf.InverseLerp(
                0f,
                crouchWalkingSpeed,
                horizontalSpeed
            );
        }
    }
}