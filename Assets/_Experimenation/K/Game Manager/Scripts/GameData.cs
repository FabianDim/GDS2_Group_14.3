using System;
using _Experimenation.Fabian.Scripts.PhaseController;
using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Multiplayer.Scripts;
using _Project.Abilities.Scripts;
using Fusion;
using UnityEngine;

namespace _Experimenation.K.Game_Manager.Scripts
{
    public struct PlayerData : INetworkStruct, IEquatable<PlayerData>
    {
        public NetworkString<_32> Username;
        public int Points;
        public int Score;
        public PlayerRole Role;

        public static bool operator ==(PlayerData left, PlayerData right) =>
            left.Equals(right);

        public static bool operator !=(PlayerData left, PlayerData right) =>
            !left.Equals(right);

        public bool Equals(PlayerData other)
        {
            return Username == other.Username &&
                   Points == other.Points &&
                   Score == other.Score &&
                   Role == other.Role;
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Username.Value ?? string.Empty, Points, Score, (int)Role);
        }

        public override string ToString()
        {
            return $"{Username.Value ?? string.Empty}\n{Role}\n{Score}\n{Points}";
        }
    }

    public class GameData : NetworkBehaviour
    {
        private const string DefaultHostUsername = "Player 1";
        private const string DefaultClientUsername = "Player 2";

        public static GameData Instance;

        [Networked] public ref PlayerData P1Data => ref MakeRef<PlayerData>();
        [Networked] public ref PlayerData P2Data => ref MakeRef<PlayerData>();
        [Networked, Capacity(2)] public NetworkDictionary<PlayerRef, uint> PurchasedAbilities => default;
        [Networked] private int PurchaseRound { get; set; }

        public AbilityDatabase AbilityDatabase => abilityDatabase;

        public bool HasPurchased(PlayerRef player, int index) =>
            index >= 0 && index < 32 && PurchasedAbilities.TryGet(player, out var mask) &&
            (mask & (1u << index)) != 0;

        public int GetToolAbilityIndex(PlayerRef player, int slot)
        {
            if (abilityDatabase == null || slot < 0)
                return -1;

            for (var index = 0; index < abilityDatabase.allAbilities.Count; index++)
            {
                var ability = abilityDatabase.allAbilities[index];
                if (ability != null && ability.abilityType == AbilityType.Tool &&
                    HasPurchased(player, index) && slot-- == 0)
                    return index;
            }
            return -1;
        }

        public int LocalPoints => HasStateAuthority ? P1Data.Points : P2Data.Points;
        public PlayerRole LocalRole => HasStateAuthority ? P1Data.Role : P2Data.Role;

        public void ResetRoundPurchases()
        {
            if (!HasStateAuthority || PurchaseRound == CurrentRound)
                return;

            PurchasedAbilities.Clear();
            PurchaseRound = CurrentRound;
        }

        private string _username = string.Empty;

        public int numberOfRounds = 2;

        // The round must survive scene reloads and be visible on every peer. A plain
        // C# field would remain local to the host and reset on remote simulations.
        [Networked] public int CurrentRound { get; set; } = 1;

        public int roundDuration = 120;

        [SerializeField] private AbilityDatabase abilityDatabase;

        public override void Spawned()
        {
            name = nameof(GameData);

            var runner = Runner;
            var networkObject = Object;
            if (runner == null || networkObject == null)
            {
                Debug.LogError("GameData spawned without a valid NetworkRunner or NetworkObject.");
                return;
            }

            // GameData should be spawned once by the state authority. If a duplicate
            // arrives, do not replace the valid singleton with it.
            if (Instance != null && Instance != this)
            {
                if (HasStateAuthority)
                    TryDespawnDuplicate(runner, networkObject);

                return;
            }

            Instance = this;
            TryKeepAlive(runner);

            if (!HasStateAuthority)
                return;

            InitializeDefaultData();

            if (CurrentRound <= 0)
                CurrentRound = 1;

            // Spawned() can run before the caller invokes SetUsername(). Only write a
            // non-empty value here; NetworkString does not accept null.
            if (!string.IsNullOrEmpty(_username))
                P1Data.Username = NormalizeUsername(_username, DefaultHostUsername);
        }

        public static void TryDespawnDuplicate(NetworkRunner runner, NetworkObject networkObject)
        {
            try
            {
                runner.Despawn(networkObject);
            }
            catch (ArgumentNullException exception)
            {
                Debug.LogWarning($"GameData duplicate could not be despawned because an argument was missing: {exception.Message}");
            }
            catch (InvalidOperationException exception)
            {
                Debug.LogWarning($"GameData duplicate could not be despawned: {exception.Message}");
            }
        }

        private void TryKeepAlive(NetworkRunner runner)
        {
            if (runner.SceneManager == null)
            {
                Debug.LogError("GameData cannot persist because the NetworkRunner has no scene manager.");
                return;
            }

            if (gameObject == null)
            {
                Debug.LogError("GameData cannot persist because its GameObject is missing.");
                return;
            }

            try
            {
                runner.MakeDontDestroyOnLoad(gameObject);
            }
            catch (ArgumentNullException exception)
            {
                // Guard the scene-manager call as well as its inputs. This prevents
                // an invalid scene transition from taking down the network runner.
                Debug.LogError($"GameData could not be kept alive across scenes: {exception.Message}");
            }
            catch (InvalidOperationException exception)
            {
                Debug.LogError($"GameData could not be kept alive because the runner is not ready: {exception.Message}");
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (ReferenceEquals(Instance, this))
                Instance = null;

            _username = string.Empty;
        }

        private void OnDestroy()
        {
            // Despawned is normally called by Fusion, but this also prevents a
            // stale static reference during runner shutdown or editor teardown.
            if (ReferenceEquals(Instance, this))
                Instance = null;
        }

        /// <summary>
        /// Sets the local username. The host writes its own slot directly; a client
        /// sends its value to the state authority through the RPC below.
        /// </summary>
        public void SetUsername(string username)
        {
            if (Instance != null && Instance != this)
                return;

            _username = NormalizeUsername(username, string.Empty);

            var runner = Runner;
            if (runner == null || Object == null || !runner.IsRunning || runner.IsShutdown)
                return;

            if (HasStateAuthority)
            {
                P1Data.Username = NormalizeUsername(_username, DefaultHostUsername);
                return;
            }

            RPC_SetUsername(_username);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        private void RPC_SetUsername(NetworkString<_32> username, RpcInfo info = default)
        {
            if (!HasStateAuthority)
                return;

            // Host-mode RPCs have PlayerRef.None as their source. Client RPCs have
            // the joining client's PlayerRef, so the two-player mapping is explicit.
            if (info.Source == PlayerRef.None)
                P1Data.Username = NormalizeUsername(username.Value, DefaultHostUsername);
            else
                P2Data.Username = NormalizeUsername(username.Value, DefaultClientUsername);
        }

        public void GenerateRole()
        {
            if (!HasStateAuthority)
                return;

            // Solo play uses Runner so runner-only tools can be tested. Both the
            // menu's LocalRole and the spawned Player.Role must use this assignment.
            if (Runner.GameMode == GameMode.Single)
            {
                P1Data.Role = PlayerRole.Runner;
                P2Data.Role = PlayerRole.Chaser;
                return;
            }

            // P1 is the host in this project and P2 is the joining client. Keep the
            // assignment deterministic so it agrees with GameManager.SpawnPlayers.
            P1Data.Role = PlayerRole.Chaser;
            P2Data.Role = PlayerRole.Runner;
        }

        public void UpdateScore(bool runnerScores)
        {
            if (!HasStateAuthority)
                return;

            if (!IsValidRole(P1Data.Role) || !IsValidRole(P2Data.Role) ||
                P1Data.Role == P2Data.Role)
            {
                Debug.LogWarning("GameData cannot update the score because player roles are invalid.");
                return;
            }

            // P1Data/P2Data are networked structs. Copying them into local
            // variables and incrementing a field would not write the score back.
            var p1IsRunner = P1Data.Role == PlayerRole.Runner;
            if (runnerScores == p1IsRunner)
                P1Data.Score++;
            else
                P2Data.Score++;
        }

        private void InitializeDefaultData()
        {
            if (P1Data.Username.Length == 0)
                P1Data.Username = DefaultHostUsername;

            if (P2Data.Username.Length == 0)
                P2Data.Username = DefaultClientUsername;

            P1Data.Points = 100;
            P2Data.Points = 100;
        }

        public void RequestPurchase(int abilityIndex)
        {
            var runner = Runner;
            if (runner == null || Object == null || !runner.IsRunning || runner.IsShutdown)
                return;

            if (HasStateAuthority)
                TryPurchase(runner.LocalPlayer, abilityIndex);
            else
                RPC_RequestPurchase(abilityIndex);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        private void RPC_RequestPurchase(int abilityIndex, RpcInfo info = default)
        {
            if (!HasStateAuthority || info.Source == PlayerRef.None)
                return;

            TryPurchase(info.Source, abilityIndex);
        }

        private void TryPurchase(PlayerRef buyer, int abilityIndex)
        {
            if (!HasStateAuthority || !buyer.IsValid)
                return;

            var accepted = TryAssignPurchase(buyer, abilityIndex);
            RPC_PurchaseResult(buyer, abilityIndex, accepted);
        }

        private bool TryAssignPurchase(PlayerRef buyer, int abilityIndex)
        {
            if (abilityDatabase == null || abilityIndex < 0 || abilityIndex >= 32 ||
                abilityIndex >= abilityDatabase.allAbilities.Count || HasPurchased(buyer, abilityIndex) ||
                !GameManager.SpawnedPlayers.TryGetValue(buyer, out var playerObject) ||
                playerObject == null || !playerObject.TryGetComponent<Player>(out var player) ||
                !player.HasStateAuthority || player.Runner != Runner ||
                playerObject.InputAuthority != buyer)
                return false;

            var phase = FindAnyObjectByType<PhaseManager>();
            var ability = abilityDatabase.allAbilities[abilityIndex];
            var points = buyer == Runner.LocalPlayer ? P1Data.Points : P2Data.Points;
            if (phase == null || phase.Runner != Runner || phase.IsRunPhase ||
                ability == null || ability.abilityPrice < 0 || points < ability.abilityPrice ||
                ability.effects == null || ability.effects.Count == 0 ||
                ability.effects.Exists(effect => effect == null) ||
                (ability.abilityScope == AbilityScope.Runner && player.Role != PlayerRole.Runner) ||
                (ability.abilityScope == AbilityScope.Chaser && player.Role != PlayerRole.Chaser) ||
                (ability.abilityType == AbilityType.Tool && GetToolAbilityIndex(buyer, 3) >= 0))
                return false;

            // Assign before charging. Tools remain in the replicated loadout until
            // activated; upgrades modify only this buyer's authoritative player.
            if (ability.abilityType != AbilityType.Tool &&
                !AbilityEffectApplier.ApplyAll(ability, player))
                return false;

            PurchasedAbilities.TryGet(buyer, out var mask);
            PurchasedAbilities.Set(buyer, mask | (1u << abilityIndex));
            TrySpendPoints(buyer, ability.abilityPrice);
            return true;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_PurchaseResult(PlayerRef buyer, int abilityIndex, bool accepted)
        {
            EventBus.Raise(new AbilityPurchasedEvent(buyer, abilityIndex, accepted));
        }

        private bool TrySpendPoints(PlayerRef player, int amount)
        {
            if (!HasStateAuthority || !player.IsValid || amount < 0)
                return false;

            if (player == Runner.LocalPlayer)
            {
                if (P1Data.Points < amount)
                    return false;

                P1Data.Points -= amount;
                return true;
            }

            if (P2Data.Points < amount)
                return false;

            P2Data.Points -= amount;
            return true;
        }

        public void ChangePoints(PlayerRef collector, int points)
        {
            if (!HasStateAuthority || !collector.IsValid || points == 0)
                return;

            // This game has exactly two players: the state-authority player is P1
            // and the other player is P2. Only the host mutates the networked data.
            if (collector == Runner.LocalPlayer)
                P1Data.Points += points;
            else
                P2Data.Points += points;
        }

        private static bool IsValidRole(PlayerRole role)
        {
            return role == PlayerRole.Runner || role == PlayerRole.Chaser;
        }


        private static string NormalizeUsername(string username, string fallback)
        {
            return string.IsNullOrWhiteSpace(username) ? fallback : username.Trim();
        }
    }
}
