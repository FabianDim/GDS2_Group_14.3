using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Game_Manager.Scripts;
using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using UnityEngine;

namespace _Experimenation.K.Exfiltration_Pod.Scripts
{
    /// <summary>
    /// Authoritative exfiltration objective. The host controls when the pod unlocks,
    /// where it appears, its availability windows, and the round result. The
    /// replicated properties keep the client visuals and trigger state in sync.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class ExfiltrationPod : NetworkBehaviour
    {
        [Header("Unlock Condition")]
        [SerializeField, Min(0f)] private float distanceFromRunner = 25f;
        [SerializeField, Min(1)] private int pointTarget = 100;

        [Header("Availability")]
        [SerializeField, Min(0f)] private float podAvailableTime = 30f;
        [SerializeField, Min(0f)] private float podUnavailableTime = 15f;

        [Header("References")]
        [SerializeField] private GameObject podVisual;
        [SerializeField] private Collider podTrigger;

        [Networked] private NetworkBool IsUnlocked { get; set; }
        [Networked] private NetworkBool IsAvailable { get; set; }
        [Networked] private NetworkBool RoundFinished { get; set; }
        [Networked] private Vector3 NetworkPosition { get; set; }
        [Networked] private TickTimer AvailabilityTimer { get; set; }
        [Networked] private TickTimer CooldownTimer { get; set; }

        private Player _runnerPlayer;
        private int _remainingPoints;
        private bool _subscribed;
        private bool _visualState;
        private bool _hasAppliedPosition;
        private bool _fusionSpawned;

        public override void Spawned()
        {
            _fusionSpawned = true;
            ResolveReferences();
            ApplyVisualState(false);

            if (!HasStateAuthority)
                return;

            _remainingPoints = pointTarget;
            NetworkPosition = transform.position;

            EventBus.Subscribe<AllPlayersSpawnedEvent>(OnAllPlayersSpawned);
            // The pod unlocks through either of two authoritative conditions:
            // the Runner earns the token target, or the Run Phase timer expires.
            EventBus.Subscribe<TimeRunsOutEvent>(OnTimeRunsOut);
            EventBus.Subscribe<TokenCollectedEvent>(OnTokenCollected);
            _subscribed = true;

            TryResolveRunner();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _fusionSpawned = false;

            if (!_subscribed)
                return;

            EventBus.Unsubscribe<AllPlayersSpawnedEvent>(OnAllPlayersSpawned);
            EventBus.Unsubscribe<TimeRunsOutEvent>(OnTimeRunsOut);
            EventBus.Unsubscribe<TokenCollectedEvent>(OnTokenCollected);
            _subscribed = false;
        }

        private void OnDestroy()
        {
            _fusionSpawned = false;
            UnsubscribeFromEvents();
        }

        private void UnsubscribeFromEvents()
        {
            if (!_subscribed)
                return;

            EventBus.Unsubscribe<AllPlayersSpawnedEvent>(OnAllPlayersSpawned);
            EventBus.Unsubscribe<TimeRunsOutEvent>(OnTimeRunsOut);
            EventBus.Unsubscribe<TokenCollectedEvent>(OnTokenCollected);
            _subscribed = false;
        }
        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || RoundFinished)
                return;

            if (_runnerPlayer == null)
                TryResolveRunner();

            if (!IsUnlocked)
                return;

            if (IsAvailable)
            {
                TryFinishRunnerExtraction();

                if (TimerExpired(AvailabilityTimer))
                    StartCooldown();

                return;
            }

            if (TimerExpired(CooldownTimer))
            {
                CooldownTimer = default;
                ActivatePod();
                return;
            }

            // This also handles the first activation if the player registry was
            // not ready when the unlock event arrived.
            if (!CooldownTimer.IsRunning)
                ActivatePod();
        }

        private void Update()
        {
            // Unity can call Update on a scene object before Fusion has completed
            // Spawned(). Networked properties are illegal to read before then.
            if (!_fusionSpawned)
                return;

            ResolveReferences();
            ApplyVisualState(IsAvailable);

            if (IsUnlocked && !_hasAppliedPosition)
            {
                transform.position = NetworkPosition;
                _hasAppliedPosition = true;
            }
            else if (IsUnlocked && transform.position != NetworkPosition)
            {
                transform.position = NetworkPosition;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!HasStateAuthority || !IsAvailable || RoundFinished)
                return;

            var player = other.GetComponentInParent<Player>();
            if (player != null)
                TryFinishRunnerExtraction(player);
        }

        private void TryFinishRunnerExtraction()
        {
            if (_runnerPlayer == null || podTrigger == null)
                return;

            // SimpleKCC movement does not require a Rigidbody trigger callback on
            // every peer. The authoritative bounds check makes extraction reliable.
            if (podTrigger.bounds.Contains(_runnerPlayer.transform.position))
                TryFinishRunnerExtraction(_runnerPlayer);
        }

        private void TryFinishRunnerExtraction(Player player)
        {
            if (!HasStateAuthority || !IsAvailable || RoundFinished ||
                player == null || player.Role != PlayerRole.Runner)
                return;

            RoundFinished = true;
            IsAvailable = false;
            AvailabilityTimer = default;
            CooldownTimer = default;
            EventBus.Raise(new RoundOverEvent(true));
        }

        private void OnAllPlayersSpawned(AllPlayersSpawnedEvent ev)
        {
            TryResolveRunner();
        }

        private void OnTimeRunsOut(TimeRunsOutEvent ev)
        {
            // Timeout is the second unlock condition. It does not represent a
            // round win; the pod becomes available so the Runner can extract.
            UnlockPod();
        }

        private void OnTokenCollected(TokenCollectedEvent ev)
        {
            if (ev == null || IsUnlocked || ev.CollectedBy == null ||
                ev.CollectedBy.Role != PlayerRole.Runner)
            {
                return;
            }

            // This threshold tracks only points earned by the Runner during this
            // round; the Runner's starting GameData points are unrelated.
            _remainingPoints -= Mathf.Max(0, ev.Points);
            if (_remainingPoints <= 0)
                UnlockPod();
        }

        private void UnlockPod()
        {
            if (!HasStateAuthority || IsUnlocked || RoundFinished)
                return;

            IsUnlocked = true;
            ActivatePod();
        }

        private void ActivatePod()
        {
            if (!HasStateAuthority || RoundFinished || !IsUnlocked)
                return;

            if (_runnerPlayer == null)
                TryResolveRunner();

            if (_runnerPlayer == null)
                return;

            NetworkPosition = _runnerPlayer.transform.position +
                              _runnerPlayer.transform.forward * distanceFromRunner;
            IsAvailable = true;
            AvailabilityTimer = TickTimer.CreateFromSeconds(
                Runner,
                Mathf.Max(0f, podAvailableTime));
            CooldownTimer = default;
        }

        private void StartCooldown()
        {
            IsAvailable = false;
            AvailabilityTimer = default;
            CooldownTimer = TickTimer.CreateFromSeconds(
                Runner,
                Mathf.Max(0f, podUnavailableTime));
        }

        private bool TimerExpired(TickTimer timer)
        {
            return timer.IsRunning && timer.Expired(Runner);
        }

        private void TryResolveRunner()
        {
            foreach (var pair in GameManager.SpawnedPlayers)
            {
                if (pair.Value == null ||
                    !pair.Value.TryGetComponent<Player>(out var player))
                    continue;

                if (player.Role != PlayerRole.Runner)
                    continue;

                _runnerPlayer = player;
                return;
            }
        }

        private void ResolveReferences()
        {
            if (podVisual == null && transform.childCount > 0)
                podVisual = transform.GetChild(0).gameObject;

            if (podTrigger == null)
                podTrigger = GetComponent<Collider>();
        }

        private void ApplyVisualState(bool available)
        {
            if (_visualState == available &&
                (podVisual == null || podVisual.activeSelf == available) &&
                (podTrigger == null || podTrigger.enabled == available))
                return;

            _visualState = available;
            if (podVisual != null)
                podVisual.SetActive(available);
            if (podTrigger != null)
                podTrigger.enabled = available;
        }
    }
}
