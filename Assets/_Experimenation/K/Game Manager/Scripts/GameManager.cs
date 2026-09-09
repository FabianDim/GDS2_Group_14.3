using System;
using System.Collections.Generic;
using System.Linq;
using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Experimenation.K.Game_Manager.Scripts
{
    public class GameManager : NetworkRunnerCallbacks
    {
        private const int ExpectedPlayerCount = 2;
        private const int HostSpawnPointIndex = 0;
        private const int ClientSpawnPointIndex = 1;
        private const int MenuSceneIndex = 0;

        [Header("Player Spawning")]
        [SerializeField] private NetworkPrefabRef playerPrefab;
        [SerializeField] private Transform[] spawnPoints;

        // QteFight uses this registry to find the two players after spawning.
        public static Dictionary<PlayerRef, NetworkObject> SpawnedPlayers { get; } = new();
        
        private bool _shutdownRequested;
        private bool _menuLoadRequested;

        #region Fusion lifecycle

        public override void Spawned()
        {
            if (Runner == null)
            {
                Debug.LogError("GameManager spawned without a NetworkRunner.");
                return;
            }

            Runner.AddCallbacks(this);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            // Use Fusion's runner argument because the Runner property may already
            // be unavailable while this object is being despawned.
            runner?.RemoveCallbacks(this);
        }

        public override void OnSceneLoadDone(NetworkRunner runner)
        {
            if (runner == null || !runner.IsServer || !HasStateAuthority)
                return;

            SpawnPlayers(runner);
        }

        public override void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            if (runner != null)
                MultiplayerLog.LogShutdown(runner, shutdownReason);

            SpawnedPlayers.Clear();
            Cube_Tokens.Scripts.Token.ResetLiveCount();
            LoadMenuScene();
        }

        public override void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            ShutdownRunnerAndReturnToMenu(runner);
        }

        #endregion

        #region Player spawning

        private void SpawnPlayers(NetworkRunner runner)
        {
            if (!TryGetHostAndClient(runner, out var host, out var client))
                return;

            var hostObject = SpawnPlayer(runner, host, HostSpawnPointIndex);
            var clientObject = SpawnPlayer(runner, client, ClientSpawnPointIndex);

            if (!TryAssignRoles(hostObject, clientObject))
                return;

            EventBus.Raise(new AllPlayersSpawnedEvent());
        }

        private bool TryGetHostAndClient(
            NetworkRunner runner,
            out PlayerRef host,
            out PlayerRef client)
        {
            host = default;
            client = default;

            if (!ValidateSpawnConfiguration(runner))
                return false;

            var activePlayers = runner.ActivePlayers.ToList();
            if (activePlayers.Count != ExpectedPlayerCount)
            {
                Debug.LogWarning(
                    $"GameManager: expected {ExpectedPlayerCount} active players, " +
                    $"but found {activePlayers.Count}.");
                return false;
            }

            // In Host Mode, LocalPlayer is the host. The other active player is
            // therefore the joining client.
            host = runner.LocalPlayer;
            if (!host.IsValid || !activePlayers.Contains(host))
            {
                Debug.LogError("GameManager: the host is not present in ActivePlayers.");
                return false;
            }

            var @ref = host;
            client = activePlayers.FirstOrDefault(player => player != @ref);
            if (!client.IsValid)
            {
                Debug.LogError("GameManager: could not find the joining client.");
                return false;
            }

            return true;
        }

        private bool ValidateSpawnConfiguration(NetworkRunner runner)
        {
            if (runner == null || !runner.IsRunning || runner.IsShutdown)
            {
                Debug.LogError("GameManager: the network runner is not ready for spawning.");
                return false;
            }

            if (!playerPrefab.IsValid)
            {
                Debug.LogError("GameManager: playerPrefab is not assigned or is invalid.");
                return false;
            }

            if (spawnPoints == null || spawnPoints.Length < ExpectedPlayerCount)
            {
                Debug.LogError(
                    $"GameManager: {ExpectedPlayerCount} spawn points are required.");
                return false;
            }

            for (var index = 0; index < ExpectedPlayerCount; index++)
            {
                if (spawnPoints[index] != null)
                    continue;

                Debug.LogError($"GameManager: spawn point {index} is not assigned.");
                return false;
            }

            return true;
        }

        private NetworkObject SpawnPlayer(
            NetworkRunner runner,
            PlayerRef player,
            int spawnPointIndex)
        {
            if (SpawnedPlayers.TryGetValue(player, out var existingObject))
            {
                if (existingObject != null)
                    return existingObject;

                // Remove a destroyed object left over from a previous scene load.
                SpawnedPlayers.Remove(player);
            }

            var spawnPoint = spawnPoints[spawnPointIndex];
            try
            {
                var playerObject = runner.Spawn(
                    playerPrefab,
                    spawnPoint.position,
                    spawnPoint.rotation,
                    player);

                if (playerObject == null)
                {
                    Debug.LogError($"GameManager: Fusion failed to spawn player {player}.");
                    return null;
                }

                SpawnedPlayers[player] = playerObject;
                return playerObject;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"GameManager: exception while spawning player {player}: {exception}");
                return null;
            }
        }

        private bool TryAssignRoles(NetworkObject hostObject, NetworkObject clientObject)
        {
            if (hostObject == null || clientObject == null)
            {
                Debug.LogError("GameManager: both player objects must spawn before assigning roles.");
                return false;
            }

            if (!hostObject.TryGetComponent<Player>(out var hostPlayer))
            {
                Debug.LogError("GameManager: the host player prefab has no Player component.");
                return false;
            }

            if (!clientObject.TryGetComponent<Player>(out var clientPlayer))
            {
                Debug.LogError("GameManager: the client player prefab has no Player component.");
                return false;
            }

            var gameData = GameData.Instance;
            if (gameData == null)
            {
                Debug.LogError("GameManager: GameData is not available when players are spawned.");
                return false;
            }

            if (!gameData.HasStateAuthority)
            {
                Debug.LogError("GameManager: only GameData state authority can assign roles.");
                return false;
            }

            gameData.GenerateRole();

            // P1 is the host and P2 is the joining client in this project.
            hostPlayer.Role = gameData.P1Data.Role;
            clientPlayer.Role = gameData.P2Data.Role;
            return true;
        }

        #endregion

        #region Shutdown

        private async void ShutdownRunnerAndReturnToMenu(NetworkRunner runner)
        {
            try
            {
                if (_shutdownRequested)
                    return;

                _shutdownRequested = true;

                try
                {
                    if (runner != null && runner.IsRunning)
                        await runner.Shutdown();
                }
                catch (Exception exception)
                {
                    Debug.LogError($"Error shutting down the network runner:\n{exception}");
                }
                finally
                {
                    LoadMenuScene();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"From GameManager.cs: {e}");
            }
        }

        private void LoadMenuScene()
        {
            if (_menuLoadRequested)
                return;

            if (SceneManager.GetActiveScene().buildIndex == MenuSceneIndex)
                return;

            _menuLoadRequested = true;
            SceneManager.LoadScene(MenuSceneIndex);
        }

        #endregion
    }
}
