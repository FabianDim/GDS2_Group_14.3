using System;
using _Experimenation.K.Multiplayer.Scripts;
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

        private string _username = string.Empty;

        public int numberOfRounds = 2;

        // The round must survive scene reloads and be visible on every peer. A plain
        // C# field would remain local to the host and reset on remote simulations.
        [Networked] public int CurrentRound { get; set; } = 1;

        public int roundDuration = 300;

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

        private static void TryDespawnDuplicate(NetworkRunner runner, NetworkObject networkObject)
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
