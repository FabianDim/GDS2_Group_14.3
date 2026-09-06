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

        [Space, SerializeField] private float qteCooldown;
        private TickTimer _qteCooldownTimer;

        private static readonly InputButton[] CatchButtons =
        {
            InputButton.CatchUp, InputButton.CatchDown, InputButton.CatchLeft, InputButton.CatchRight,
        };

        private Slider _meter;
        private TextMeshProUGUI _keyPrompt;

        private TickTimer _mashTimer;
        private TickTimer _catchTimer;
        private TickTimer _runnerBoostTimer;
        private bool _canCatch;
        private bool _roundEnded;
        private bool _meterVisible;

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
        [Networked] private NetworkBool KeyPhaseActive { get; set; }
        [Networked] private NetworkBool MashPhaseActive { get; set; }

        public override void Spawned()
        {
            _meter = GetComponentInChildren<Slider>(true);
            _keyPrompt = GetComponentInChildren<TextMeshProUGUI>(true);
            if (_keyPrompt != null)
                _keyPrompt.gameObject.SetActive(false);

            if (!HasStateAuthority) return;
            EventBus.Subscribe<AllPlayersSpawnedEvent>(OnAllPlayersSpawned);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (!HasStateAuthority) return;
            EventBus.Unsubscribe<AllPlayersSpawnedEvent>(OnAllPlayersSpawned);
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || _roundEnded)
                return;

            // A default (never-started) TickTimer reports Expired() == false, so the
            // cooldown must be gated on IsRunning - an unconditional Expired check
            // here would block CheckDistance (and everything after it) forever.
            var qteOnCooldown = _qteCooldownTimer.IsRunning;
            if (qteOnCooldown && _qteCooldownTimer.Expired(Runner))
            {
                _qteCooldownTimer = default;
                qteOnCooldown = false;
            }

            CheckDistance(qteOnCooldown);

            // Runner speed boost expiry - replicated multiplier back to 1x.
            if (_runnerBoostTimer.IsRunning && _runnerBoostTimer.Expired(Runner))
            {
                _runnerBoostTimer = default;
                if (_runnerMovement != null)
                    _runnerMovement.SpeedBoostMultiplier = 1f;
            }

            if (_canCatch)
                HandleCatchKeyInput();
            else if (_mashTimer.IsRunning)
                HandleMashInput();
        }

        private void Update()
        {
            if (_meter)
                _meter.value = MeterValue;

            if (_keyPrompt == null)
                return;

            if (KeyPhaseActive)
            {
                _keyPrompt.gameObject.SetActive(true);
                _keyPrompt.SetText($"Press {GetKeyLabel(CurrentKeyIndex)}!");
            }
            else if (MashPhaseActive)
            {
                _keyPrompt.gameObject.SetActive(true);
                _keyPrompt.SetText($"Mash {GetMashLabel()}!");
            }
            else
            {
                _keyPrompt.gameObject.SetActive(false);
            }
        }

        /// <summary>Distributed round end. Safe to call from any machine; only State Authority acts.</summary>
        private void EndRound(bool runnerWins)
        {
            if (!HasStateAuthority || _roundEnded)
                return;

            _roundEnded = true;
            KeyPhaseActive = false;
            MashPhaseActive = false;
            if (_meterVisible)
            {
                RPC_ShowMeter(false);
                _meterVisible = false;
            }
            RPC_EndRound(runnerWins);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_EndRound(bool runnerWins)
        {
            EventBus.Raise(new RoundOverEvent(runnerWins));
        }

        private void CheckDistance(bool qteOnCooldown)
        {
            if (_chaserTransform == null || _runnerTransform == null)
                return;

            var distance = Vector3.Distance(_chaserTransform.position, _runnerTransform.position);

            // Too far apart - idle.
            if (distance > triggerDistance)
            {
                if (KeyPhaseActive || MashPhaseActive || _mashTimer.IsRunning || _meterVisible)
                {
                    if (_meterVisible)
                    {
                        RPC_ShowMeter(false);
                        _meterVisible = false;
                    }
                    KeyPhaseActive = false;
                    MashPhaseActive = false;
                    _canCatch = false;
                    _mashTimer = default;
                    _catchTimer = default;
                }
                MeterValue = mashStartValue;
                return;
            }

            // Direct physical contact - instant catch. Never gated by the QTE
            // cooldown, and measured against the physical capsule-contact distance,
            // not the raw serialized value (pivots can never get that close).
            if (distance <= _contactDistance)
            {
                EndRound(false);
                return;
            }

            // The escape cooldown only delays the next mash/key phase,
            // not the direct-contact catch above.
            if (qteOnCooldown)
                return;

            if (!_meterVisible)
            {
                RPC_ShowMeter(true);
                _meterVisible = true;
            }

            // Key phase - waiting for the Chaser to press the shown key.
            if (_canCatch)
            {
                // Chaser failed the key phase - the Runner escapes with a Speed Boost.
                if (_catchTimer.IsRunning && _catchTimer.Expired(Runner))
                {
                    EndQte();
                    GiveRunnerBoost();
                }
                return;
            }

            // Mash phase - start the tug-of-war timer once.
            if (!_mashTimer.IsRunning)
            {
                _mashTimer = TickTimer.CreateFromSeconds(Runner, qteDuration);
                MashPhaseActive = true;
                return;
            }

            // Mash phase - resolve when the timer expires.
            if (_mashTimer.Expired(Runner))
            {
                if (MeterValue >= chaserWinThreshold)
                {
                    MashPhaseActive = false;
                    StartKeyPhase();
                }
                else
                {
                    // Runner won the QTE - they escape with a Speed Boost, round continues.
                    EndQte();
                    GiveRunnerBoost();
                }
            }
        }

        /// <summary>Ends the QTE without ending the round - play continues.</summary>
        private void EndQte()
        {
            KeyPhaseActive = false;
            MashPhaseActive = false;
            _canCatch = false;
            _mashTimer = default;
            _catchTimer = default;
            MeterValue = mashStartValue;
            if (_meterVisible)
            {
                RPC_ShowMeter(false);
                _meterVisible = false;
            }

            _qteCooldownTimer = TickTimer.CreateFromSeconds(Runner, qteCooldown);
        }

        private void GiveRunnerBoost()
        {
            if (_runnerMovement == null)
                return;

            _runnerMovement.SpeedBoostMultiplier = runnerBoostMultiplier;

            // Extend the active boost rather than restarting it if one is still running.
            // RemainingTime is a nullable double - null when the timer is not running, so
            // coalesce to 0 to avoid an InvalidOperationException from the (float) cast.
            var remaining = _runnerBoostTimer.IsRunning
                ? (float)(_runnerBoostTimer.RemainingTime(Runner) ?? 0d)
                : 0f;
            _runnerBoostTimer = TickTimer.CreateFromSeconds(Runner, remaining + runnerBoostDuration);
        }

        private void StartKeyPhase()
        {
            _canCatch = true;
            CurrentKeyIndex = Random.Range(0, CatchButtons.Length);
            KeyPhaseActive = true;
            _catchTimer = TickTimer.CreateFromSeconds(Runner, catchDuration);
        }

        private void HandleMashInput()
        {
            if (Runner.TryGetInputForPlayer(_chaserPlayer, out GameplayInput chaserInput))
            {
                if (chaserInput.Buttons.WasPressed(_chaserPreviousButtons, InputButton.QteFight))
                    MeterValue = Mathf.Clamp01(MeterValue + qteStrength);
                _chaserPreviousButtons = chaserInput.Buttons;
            }

            if (Runner.TryGetInputForPlayer(_runnerPlayer, out GameplayInput runnerInput))
            {
                if (runnerInput.Buttons.WasPressed(_runnerPreviousButtons, InputButton.QteFight))
                    MeterValue = Mathf.Clamp01(MeterValue - qteStrength);
                _runnerPreviousButtons = runnerInput.Buttons;
            }
        }

        private void HandleCatchKeyInput()
        {
            if (!Runner.TryGetInputForPlayer(_chaserPlayer, out GameplayInput chaserInput))
                return;

            var requiredButton = CatchButtons[CurrentKeyIndex];
            if (chaserInput.Buttons.WasPressed(_chaserPreviousButtons, requiredButton))
            {
                EndRound(false);
                return;
            }

            // Wrong key or no press is NOT an immediate fail - the phase only fails
            // when the catch timer expires (handled in CheckDistance).
            _chaserPreviousButtons = chaserInput.Buttons;
        }

        private string GetKeyLabel(int index)
        {
            var labels = Gamepad.current != null ? gamepadKeyLabels : keyboardKeyLabels;
            if (index < 0 || index >= labels.Length)
                return "?";
            return labels[index];
        }

        private string GetMashLabel()
        {
            return Gamepad.current != null ? mashGamepadLabel : mashKeyboardLabel;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_ShowMeter(bool show) =>
            _meter.gameObject.SetActive(show);

        private void OnAllPlayersSpawned(AllPlayersSpawnedEvent ev)
        {
            foreach (var pair in GameManager.SpawnedPlayers)
            {
                var player = pair.Value.GetComponent<Player>();
                if (player == null)
                    continue;

                if (player.Role == PlayerRole.Chaser)
                {
                    _chaserPlayer = pair.Key;
                    _chaserTransform = pair.Value.transform;
                    _chaserController = pair.Value.GetComponent<CharacterController>();
                }
                else
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
