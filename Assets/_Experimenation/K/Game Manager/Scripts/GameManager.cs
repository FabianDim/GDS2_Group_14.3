using System.Collections.Generic;
using System.Linq;
using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using UnityEngine;

namespace _Experimenation.K.Game_Manager.Scripts
{
    public class GameManager : NetworkRunnerCallbacks
    {
        [SerializeField] private NetworkPrefabRef playerPrefab;
        [SerializeField] private Transform[] spawnPoints; // assign 2 in Inspector, index 0 and 1

        private readonly Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new();

        public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            // Only the host/server spawns networked objects
            if (!runner.IsServer) return;

            // Use the number of already-spawned players to pick the next spawn point
            var spawnIndex = _spawnedPlayers.Count % spawnPoints.Length;
            var spawnPos = spawnPoints[spawnIndex].position;
            var spawnRot = spawnPoints[spawnIndex].rotation;

            var playerObject = runner.Spawn(
                playerPrefab,
                spawnPos,
                spawnRot,
                player // this sets Input Authority to the joining player
            );
            
            //Assign roles
            if (_spawnedPlayers.Count == 2)
            {
                var player1 = _spawnedPlayers.ElementAt(0).Value
                    .GetComponent<Player>();

                var player2 = _spawnedPlayers.ElementAt(1).Value
                    .GetComponent<Player>();

                var player1IsRunner = Random.Range(0, 2) == 0;

                player1.Role = player1IsRunner
                    ? PlayerRole.Runner
                    : PlayerRole.Chaser;

                player2.Role = player1IsRunner
                    ? PlayerRole.Chaser
                    : PlayerRole.Runner;
            }
            
            _spawnedPlayers[player] = playerObject;
        }

        public override void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (!runner.IsServer) return;

            if (!_spawnedPlayers.TryGetValue(player, out var playerObject)) return;
            runner.Despawn(playerObject);
            _spawnedPlayers.Remove(player);
        }
    }
}