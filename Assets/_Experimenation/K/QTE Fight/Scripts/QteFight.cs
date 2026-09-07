using _Experimenation.Fraser.Scripts;
using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Game_Manager.Scripts;
using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Experimenation.K.QTE_Fight.Scripts
{
    /// <summary>
    /// Two-phase catch QTE:
    /// Phase A - while the players are close, Chaser and Runner mash the QTE Fight
    /// button and the meter is a tug-of-war. When the mash timer expires the Chaser
    /// wins if the meter is still above the threshold.
    /// Phase B - a random catch key is shown and the Chaser must press it within the
    /// catch window to capture the Runner. Only the Chaser can end the round.
    /// A lost mash or a failed key phase never ends the round - the Runner escapes
    /// and receives a temporary Speed Boost. Direct physical contact still counts
    /// as an instant catch.
    /// </summary>
    public class QteFight : NetworkBehaviour
    {
        private enum QtePhase : byte
        {
            Idle,
            Mash,
            Catch,
        }

        [Header("Distances")]
        [SerializeField] private float triggerDistance = 10f;
        [SerializeField] private float catchingDistance = 0.1f;

        [Space, Header("Mash Phase")]
        [SerializeField] private float qteStrength = 0.025f;
        [SerializeField] private float qteDuration = 5f;
        [SerializeField, Range(0f, 1f)] private float mashStartValue = 0.75f;
        [SerializeField, Range(0f, 1f)] private float chaserWinThreshold = 0.5f;

        [Space, Header("Key Phase")]
        [SerializeField] private float catchDuration = 5f;
        [SerializeField] private string[] keyboardKeyLabels = { "I", "K", "J", "L" };
        [SerializeField] private string[] gamepadKeyLabels = { "Y", "A", "X", "B" };

        [Space, Header("Runner Boost")]
        [SerializeField] private float runnerBoostDuration = 3f;
        [SerializeField] private float runnerBoostMultiplier = 1.5f;

        [Space, Header("Prompts")]
        [SerializeField] private string mashKeyboardLabel = "J";
        [SerializeField] private string mashGamepadLabel = "Y";

        [Space, Header("Cooldown")]
        [SerializeField, Min(0f)] private float qteCooldown = 5f;

        private static readonly InputButton[] CatchButtons =
        {
            InputButton.CatchUp,
            InputButton.CatchDown,
            InputButton.CatchLeft,
            InputButton.CatchRight,
        };

        private Slider _meter;
        private TextMeshProUGUI _keyPrompt;
        private bool _meterVisible;
        private bool _roundEnded;
        private bool _distanceCooldownApplied;
        private bool _subscribedToEvents;

        private TickTimer _qteCooldownTimer;
        private TickTimer _mashTimer;
        private TickTimer _catchTimer;
        private TickTimer _runnerBoostTimer;

        private PlayerRef _chaserPlayer;
        private PlayerRef _runnerPlayer;
        private Transform _chaserTransform;
        private Transform _runnerTransform;
        private PlayerMovement _runnerMovement;
        private CharacterController _chaserController;
        private CharacterController _runnerController;

        // Pivot distance at which the two player capsules physically touch.
        // CharacterControllers depenetrate each other, so the pivots can never
        // get within (r1 + r2) of each other - a raw 0.1m check is unreachable.
        private float _contactDistance;

        private NetworkButtons _chaserPreviousButtons;
        private NetworkButtons _runnerPreviousButtons;

        [Networked] private float MeterValue { get; set; }
        [Networked] private int CurrentKeyIndex { get; set; }
        [Networked] private QtePhase Phase { get; set; }

        public override void Spawned()
        {
            _meter = GetComponentInChildren<Slider>(true);
            _keyPrompt = GetComponentInChildren<TextMeshProUGUI>(true);

            if (_meter != null)
                _meter.gameObject.SetActive(false);
            if (_keyPrompt != null)
                _keyPrompt.gameObject.SetActive(false);

            if (!HasStateAuthority)
                return;

            EventBus.Subscribe<AllPlayersSpawnedEvent>(OnAllPlayersSpawned);
            _subscribedToEvents = true;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (!_subscribedToEvents)
                return;

            EventBus.Unsubscribe<AllPlayersSpawnedEvent>(OnAllPlayersSpawned);
            _subscribedToEvents = false;
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || _roundEnded)
                return;

            var qteOnCooldown = UpdateCooldown();
            UpdateRunnerBoost();

            if (!TryGetPlayerDistance(out var distance))
                return;

            if (HandleOutOfRange(distance))
                return;

            // The players are back in range, so a future separation can start a
            // new distance-based cooldown. An already-running timer continues.
            _distanceCooldownApplied = false;

            // Direct contact always wins, even while a QTE cooldown is active.
            if (distance <= _contactDistance)
            {
                EndRound(false);
                return;
            }

            if (qteOnCooldown)
                return;

            SetMeterVisible(true);
            UpdateQtePhase();
        }

        private void Update()
        {
            if (_meter != null)
                _meter.value = MeterValue;

            UpdatePrompt();
        }

        private void UpdatePrompt()
        {
            if (_keyPrompt == null)
                return;

            switch (Phase)
            {
                case QtePhase.Mash:
                    ShowPrompt($"Mash {GetMashLabel()}!");
                    break;

                case QtePhase.Catch:
                    ShowPrompt($"Press {GetKeyLabel(CurrentKeyIndex)}!");
                    break;

                default:
                    _keyPrompt.gameObject.SetActive(false);
                    break;
            }
        }

        private void ShowPrompt(string text)
        {
            _keyPrompt.gameObject.SetActive(true);
            _keyPrompt.SetText(text);
        }

        /// <summary>Distributed round end. Only State Authority can finish the round.</summary>
        private void EndRound(bool runnerWins)
        {
            if (!HasStateAuthority || _roundEnded)
                return;

            _roundEnded = true;
            ResetQteState();
            _qteCooldownTimer = default;
            RPC_EndRound(runnerWins);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_EndRound(bool runnerWins)
        {
            EventBus.Raise(new RoundOverEvent(runnerWins));
        }

        private bool TryGetPlayerDistance(out float distance)
        {
            distance = 0f;
            if (_chaserTransform == null || _runnerTransform == null)
                return false;

            distance = Vector3.Distance(_chaserTransform.position, _runnerTransform.position);
            return true;
        }

        private bool HandleOutOfRange(float distance)
        {
            if (distance <= triggerDistance)
                return false;

            CancelQte();

            if (!_distanceCooldownApplied)
            {
                _distanceCooldownApplied = true;
                StartCooldown();
            }

            return true;
        }

        private void UpdateQtePhase()
        {
            switch (Phase)
            {
                case QtePhase.Idle:
                    StartMashPhase();
                    break;

                case QtePhase.Mash:
                    UpdateMashPhase();
                    break;

                case QtePhase.Catch:
                    UpdateCatchPhase();
                    break;

                default:
                    ResetQteState();
                    return;
            }

            // Resolve the phase first. This preserves the existing behavior where
            // a Chaser can press the catch key on the same tick the mash phase ends.
            switch (Phase)
            {
                case QtePhase.Mash:
                    HandleMashInput();
                    break;

                case QtePhase.Catch:
                    HandleCatchKeyInput();
                    break;
            }
        }

        private void StartMashPhase()
        {
            MeterValue = mashStartValue;
            _mashTimer = TickTimer.CreateFromSeconds(Runner, qteDuration);
            Phase = QtePhase.Mash;
        }

        private void UpdateMashPhase()
        {
            if (!_mashTimer.IsRunning)
            {
                StartMashPhase();
                return;
            }

            if (!TimerExpired(_mashTimer))
                return;

            if (MeterValue >= chaserWinThreshold)
                StartCatchPhase();
            else
                RunnerEscapes();
        }

        private void StartCatchPhase()
        {
            CurrentKeyIndex = Random.Range(0, CatchButtons.Length);
            _catchTimer = TickTimer.CreateFromSeconds(Runner, catchDuration);
            Phase = QtePhase.Catch;
        }

        private void UpdateCatchPhase()
        {
            if (TimerExpired(_catchTimer))
                RunnerEscapes();
        }

        /// <summary>Ends the current QTE and gives the Runner a temporary escape boost.</summary>
        private void RunnerEscapes()
        {
            EndQte();
            GiveRunnerBoost();
        }

        /// <summary>Ends the QTE without ending the round - play continues.</summary>
        private void EndQte()
        {
            ResetQteState();
            StartCooldown();
        }

        private void ResetQteState()
        {
            Phase = QtePhase.Idle;
            _mashTimer = default;
            _catchTimer = default;
            MeterValue = mashStartValue;
            SetMeterVisible(false);
        }

        private void CancelQte()
        {
            if (Phase != QtePhase.Idle || _mashTimer.IsRunning || _catchTimer.IsRunning || _meterVisible)
                ResetQteState();
            else
                MeterValue = mashStartValue;
        }

        private void StartCooldown()
        {
            if (qteCooldown <= 0f)
            {
                _qteCooldownTimer = default;
                return;
            }

            if (!_qteCooldownTimer.IsRunning)
                _qteCooldownTimer = TickTimer.CreateFromSeconds(Runner, qteCooldown);
        }

        private bool UpdateCooldown()
        {
            if (TimerExpired(_qteCooldownTimer))
                _qteCooldownTimer = default;

            return _qteCooldownTimer.IsRunning;
        }

        private void UpdateRunnerBoost()
        {
            if (!_runnerBoostTimer.IsRunning || !_runnerBoostTimer.Expired(Runner))
                return;

            _runnerBoostTimer = default;
            if (_runnerMovement != null)
                _runnerMovement.SpeedBoostMultiplier = 1f;
        }

        private void GiveRunnerBoost()
        {
            if (_runnerMovement == null)
                return;

            _runnerMovement.SpeedBoostMultiplier = runnerBoostMultiplier;

            // Extend the active boost rather than restarting it if one is still running.
            // RemainingTime is nullable when a timer is not running, so coalesce to 0.
            var remaining = _runnerBoostTimer.IsRunning
                ? (float)(_runnerBoostTimer.RemainingTime(Runner) ?? 0d)
                : 0f;
            _runnerBoostTimer = TickTimer.CreateFromSeconds(Runner, remaining + runnerBoostDuration);
        }

        private void HandleMashInput()
        {
            if (PlayerPressed(_chaserPlayer, ref _chaserPreviousButtons, InputButton.QteFight))
                MeterValue = Mathf.Clamp01(MeterValue - qteStrength);

            if (PlayerPressed(_runnerPlayer, ref _runnerPreviousButtons, InputButton.QteFight))
                MeterValue = Mathf.Clamp01(MeterValue + qteStrength);
        }

        private void HandleCatchKeyInput()
        {
            if (CurrentKeyIndex < 0 || CurrentKeyIndex >= CatchButtons.Length)
                return;

            var requiredButton = CatchButtons[CurrentKeyIndex];
            if (PlayerPressed(_chaserPlayer, ref _chaserPreviousButtons, requiredButton))
                EndRound(false);
        }

        private bool PlayerPressed(PlayerRef player, ref NetworkButtons previousButtons, InputButton button)
        {
            if (!Runner.TryGetInputForPlayer(player, out GameplayInput input))
                return false;

            var wasPressed = input.Buttons.WasPressed(previousButtons, button);
            previousButtons = input.Buttons;
            return wasPressed;
        }

        private bool TimerExpired(TickTimer timer)
        {
            return timer.IsRunning && timer.Expired(Runner);
        }

        private string GetKeyLabel(int index)
        {
            var labels = Gamepad.current != null ? gamepadKeyLabels : keyboardKeyLabels;
            if (labels == null || index < 0 || index >= labels.Length)
                return "?";

            return labels[index];
        }

        private string GetMashLabel()
        {
            return Gamepad.current != null ? mashGamepadLabel : mashKeyboardLabel;
        }

        private void SetMeterVisible(bool visible)
        {
            if (_meterVisible == visible)
                return;

            _meterVisible = visible;
            RPC_ShowMeter(visible);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_ShowMeter(bool show)
        {
            if (_meter != null)
                _meter.gameObject.SetActive(show);
        }

        private void OnAllPlayersSpawned(AllPlayersSpawnedEvent ev)
        {
            _chaserTransform = null;
            _runnerTransform = null;
            _runnerMovement = null;
            _chaserController = null;
            _runnerController = null;

            foreach (var pair in GameManager.SpawnedPlayers)
            {
                if (pair.Value == null)
                    continue;

                var player = pair.Value.GetComponent<Player>();
                if (player == null)
                    continue;

                if (player.Role == PlayerRole.Chaser)
                {
                    _chaserPlayer = pair.Key;
                    _chaserTransform = pair.Value.transform;
                    _chaserController = pair.Value.GetComponent<CharacterController>();
                }
                else if (player.Role == PlayerRole.Runner)
                {
                    _runnerPlayer = pair.Key;
                    _runnerTransform = pair.Value.transform;
                    _runnerMovement = pair.Value.GetComponent<PlayerMovement>();
                    _runnerController = pair.Value.GetComponent<CharacterController>();
                }
            }

            // Two CharacterControllers can never get closer than the sum of their
            // radii, so catch detection uses that physical contact distance instead
            // of the raw serialized value (kept as a floor).
            var chaserRadius = _chaserController != null ? _chaserController.radius : 0.4f;
            var runnerRadius = _runnerController != null ? _runnerController.radius : 0.4f;
            _contactDistance = Mathf.Max(catchingDistance, chaserRadius + runnerRadius + 0.2f);
        }
    }
}
