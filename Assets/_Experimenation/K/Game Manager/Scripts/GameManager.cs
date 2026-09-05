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
        [SerializeField] private NetworkPrefabRef playerPrefab;
        [SerializeField] private Transform[] spawnPoints;
        private GameObject _runPhaseItems;

        public enum GamePhase
        {
            BUYPHASE,
            RUNPHASE,
            ROUNDCHANGE,
            GAMESTART
        }

        private readonly Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new();

        public override void Spawned()
        {
            Runner.AddCallbacks(this);
            _runPhaseItems = transform.GetChild(0).gameObject;

            if(HasStateAuthority)
                EventBus.Subscribe<RunPhaseStartsEvent>(OnRunPhaseStarts);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if(HasStateAuthority)
                EventBus.Unsubscribe<RunPhaseStartsEvent>(OnRunPhaseStarts);
        }

        public override void OnSceneLoadDone(NetworkRunner runner)
        {
            if (!runner.IsServer)
                return;

            SpawnPlayers();
        }

        private void SpawnPlayers()
        {
            var players = Runner.ActivePlayers.ToList();
            if (players.Count != 2)
            {
                Debug.LogWarning("GameManager: expected exactly two active players before spawning.");
                return;
            }

            if (spawnPoints == null || spawnPoints.Length < 2)
            {
                Debug.LogError("GameManager: two spawn points are required.");
                return;
            }

            // The host (local server player) is always the Chaser; the client is always the Runner.
            var runnerPlayer = Runner.LocalPlayer;
            var chaserPlayer = players.First(p => p != runnerPlayer);

            var runnerObject = SpawnPlayer(runnerPlayer, PlayerRole.Runner, 0);
            var chaserObject = SpawnPlayer(chaserPlayer, PlayerRole.Chaser, 1);

            // Roles are assigned only after both objects have spawned successfully.
            if (runnerObject == null || chaserObject == null) return;

            runnerObject.GetComponent<Player>().Role = PlayerRole.Runner;
            chaserObject.GetComponent<Player>().Role = PlayerRole.Chaser;
            
            EventBus.Raise(new AllPlayersSpawnedEvent());
            return;

            NetworkObject SpawnPlayer(PlayerRef player, PlayerRole role, int spawnPointIndex)
            {
                // This also makes retries safe if one spawn succeeds and the other fails.
                if (_spawnedPlayers.TryGetValue(player, out var existingObject) && existingObject != null)
                    return existingObject;

                var spawnPoint = spawnPoints[spawnPointIndex];
                var playerObject = Runner.Spawn(
                    playerPrefab,
                    spawnPoint.position,
                    spawnPoint.rotation,
                    player
                );

                if (playerObject == null)
                {
                    Debug.LogWarning($"GameManager: failed to spawn {role} player {player}.");
                    return null;
                }

                _spawnedPlayers[player] = playerObject;
                return playerObject;
            }
        }

        public override void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            MultiplayerLog.LogShutdown(runner, shutdownReason);
            EndGame();
        }

        public override void OnPlayerLeft(NetworkRunner runner, PlayerRef player) => EndGame();

        private async void EndGame()
        {
            try
            {
                await Runner.Shutdown();
                SceneManager.LoadScene(0);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error from GameManager.cs:\n {e}");
            }
        }

        private void OnRunPhaseStarts(RunPhaseStartsEvent ev)
        {
            if (!HasStateAuthority || _runPhaseItems == null)
                return;

            RpcStartsRunPhase();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RpcStartsRunPhase()
        {
            if (_runPhaseItems != null)
                _runPhaseItems.SetActive(true);
        }
    }
}
