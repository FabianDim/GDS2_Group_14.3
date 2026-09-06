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
        [SerializeField] private GameData gameData;

        public static Dictionary<PlayerRef, NetworkObject> SpawnedPlayers { get; private set; } = new();

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
            if (!runner.IsServer) return;
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
            var p1Ref = Runner.LocalPlayer;
            var p2Ref = players.First(p => p != p1Ref);

            var p1 = SpawnPlayer(p1Ref, 0);
            var p2 = SpawnPlayer(p2Ref, 1);

            // Roles are assigned only after both objects have spawned successfully.
            if (p1 == null || p2 == null) return;

            gameData.GenerateRole();
            p1.GetComponent<Player>().Role = gameData.p1Role;
            p2.GetComponent<Player>().Role = gameData.p2Role;
            
            EventBus.Raise(new AllPlayersSpawnedEvent());
            return;

            NetworkObject SpawnPlayer(PlayerRef player, int spawnPointIndex)
            {
                // This also makes retries safe if one spawn succeeds and the other fails.
                if (SpawnedPlayers.TryGetValue(player, out var existingObject) && existingObject)
                    return existingObject;

                var spawnPoint = spawnPoints[spawnPointIndex];
                var playerObject = Runner.Spawn(
                    playerPrefab,
                    spawnPoint.position,
                    spawnPoint.rotation,
                    player
                );

                SpawnedPlayers[player] = playerObject;
                return playerObject;
            }
        }

        public override void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            MultiplayerLog.LogShutdown(runner, shutdownReason);

            // Static state must never survive into the next match or scene load.
            SpawnedPlayers.Clear();

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

        private void OnRunPhaseStarts(RunPhaseStartsEvent ev) => RpcStartsRunPhase();
        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RpcStartsRunPhase() => _runPhaseItems.SetActive(true);
    }
}
