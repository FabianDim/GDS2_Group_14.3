using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using UnityEngine;

namespace _Experimenation.Fraser.Scripts
{
    public class Climb : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform orientation;

        [Header("Detection")]
        [SerializeField] private LayerMask climbMask;
        [SerializeField] private float climbDistance = 0.8f;
        [SerializeField] private float wallCheckHeight = 1f;
        [SerializeField] private float lowerCheckHeight = 0.35f;
        [SerializeField] private float wallCheckRadius = 0.15f;

        [Header("Climbing")]
        [SerializeField] private float climbSpeed = 5f;
        [SerializeField] private float climbSideSpeed = 2.5f;
        [SerializeField] private float wallPullSpeed = 1f;
        [SerializeField] private float climbHeightMultiplier = 5f;

        [Header("Top Out")]
        [SerializeField] private float topOutForwardSpeed = 3f;

        [Header("Debug")]
        [SerializeField] private bool wallInFront;
        [SerializeField] private bool lowerWallInFront;
        [SerializeField] private bool climbLocked;
        [SerializeField] private float climbDistanceUsed;

        private bool IsClimbing { get; set; }

        private float _maximumClimbHeight;
        private float _previousClimbHeight;

        private RaycastHit _wallHit;
        private RaycastHit _lowerWallHit;

        private PlayerMovement _playerMovement;
        private WallRun _wallRun;

        public override void Spawned()
        {
            _playerMovement = GetComponent<PlayerMovement>();
            _wallRun = GetComponent<WallRun>();

            _maximumClimbHeight =
                2f * climbHeightMultiplier;

            _previousClimbHeight =
                transform.position.y;
        }

        public void UpdateClimbState(GameplayInput playerInput)
        {
            if (!_playerMovement) return;

            if (_playerMovement.IsGrounded)
            {
                climbLocked = false;
                climbDistanceUsed = 0f;
            }

            CheckWall();

            if (IsClimbing)
            {
                TrackClimbDistance();

                if (climbDistanceUsed >= _maximumClimbHeight)
                {
                    ExhaustClimb();
                    return;
                }

                var movingForward =
                    playerInput.MoveInput.y > 0.1f;

                if (movingForward &&
                    (wallInFront || lowerWallInFront))
                {
                    return;
                }

                StopClimb();
                return;
            }

            var canStartClimb =
                !climbLocked &&
                !_playerMovement.IsCrouching &&
                !_playerMovement.IsSliding &&
                playerInput.MoveInput.y > 0.1f &&
                wallInFront;

            if (canStartClimb)
            {
                StartClimb();
            }
        }

        private void CheckWall()
        {
            var physicsScene =
                Runner.GetPhysicsScene();

            var centrePosition =
                transform.position +
                Vector3.up * wallCheckHeight;

            var lowerPosition =
                transform.position +
                Vector3.up * lowerCheckHeight;

            wallInFront = physicsScene.SphereCast(
                centrePosition,
                wallCheckRadius,
                orientation.forward,
                out _wallHit,
                climbDistance,
                climbMask,
                QueryTriggerInteraction.Ignore
            );

            lowerWallInFront = physicsScene.SphereCast(
                lowerPosition,
                wallCheckRadius,
                orientation.forward,
                out _lowerWallHit,
                climbDistance,
                climbMask,
                QueryTriggerInteraction.Ignore
            );

            Debug.DrawRay(
                centrePosition,
                orientation.forward * climbDistance,
                wallInFront ? Color.yellow : Color.blue
            );

            Debug.DrawRay(
                lowerPosition,
                orientation.forward * climbDistance,
                lowerWallInFront ? Color.yellow : Color.cyan
            );
        }

        private void StartClimb()
        {
            if (IsClimbing)
            {
                return;
            }

            IsClimbing = true;
            _playerMovement.IsClimbing = true;

            _previousClimbHeight =
                transform.position.y;

            _playerMovement.ClearMovementVelocity();
            _playerMovement.SetGravity(0f);

            if (_wallRun != null)
            {
                _wallRun.StopWallRunFromClimb();
            }
        }

        private void StopClimb()
        {
            if (!IsClimbing)
            {
                return;
            }

            IsClimbing = false;
            _playerMovement.IsClimbing = false;

            _playerMovement.SetGravity(
                _playerMovement.NormalGravity
            );
        }

        private void ExhaustClimb()
        {
            climbLocked = true;

            IsClimbing = false;
            _playerMovement.IsClimbing = false;

            _playerMovement.ClearMovementVelocity();

            _playerMovement.SetGravity(
                _playerMovement.NormalGravity
            );
        }

        private void TrackClimbDistance()
        {
            var heightDifference =
                transform.position.y -
                _previousClimbHeight;

            if (heightDifference > 0f)
            {
                climbDistanceUsed +=
                    heightDifference;
            }

            _previousClimbHeight =
                transform.position.y;
        }

        public Vector3 GetClimbVelocity(GameplayInput playerInput)
        {
            var currentWallHit = wallInFront ? _wallHit : _lowerWallHit;

            var wallSideDirection =
                Vector3.Cross(
                    Vector3.up,
                    currentWallHit.normal
                ).normalized;

            if (Vector3.Dot(
                    wallSideDirection,
                    orientation.right
                ) < 0f)
            {
                wallSideDirection =
                    -wallSideDirection;
            }

            var sideVelocity =
                wallSideDirection *
                playerInput.MoveInput.x *
                climbSideSpeed;

            var climbVelocity =
                Vector3.up * climbSpeed;

            var wallVelocity =
                -currentWallHit.normal *
                wallPullSpeed;

            if (!wallInFront &&
                lowerWallInFront)
            {
                wallVelocity =
                    orientation.forward *
                    topOutForwardSpeed;
            }

            return
                climbVelocity +
                sideVelocity +
                wallVelocity;
        }
    }
}