using System.Collections.Generic;
using System.Linq;
using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using UnityEngine;

namespace _Experimenation.K.Game_Manager.Scripts
{
    public class GameManager : NetworkRunnerCallbacks
    {
        [Header("Players")]
        [SerializeField] private NetworkPrefabRef playerPrefab;
        [SerializeField] private Transform[] spawnPoints;
        private GameObject _runPhaseItems;
        
        [Header("Exfiltration Pod")]
        [SerializeField] private NetworkPrefabRef exitPodPrefab;

        private readonly Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new();
        private NetworkRunner _networkRunner;
        private PlayerRef? _runnerPlayer;

        public void Awake()
        {
            _networkRunner = FindAnyObjectByType<NetworkRunner>();
            if (_networkRunner == null)
            {
                Debug.LogError("GameManager: no NetworkRunner found in scene.");
                return;
            }
            _networkRunner.AddCallbacks(this);
            _runPhaseItems = transform.GetChild(0).gameObject;

            EventBus.Subscribe<RunPhaseStartsEvent>(OnRunPhaseStarts);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<RunPhaseStartsEvent>(OnRunPhaseStarts);
        }
        
        public override void OnSceneLoadDone(NetworkRunner runner)
        {
            if (!runner.IsServer) return;
            SpawnExitPod();
            SpawnPlayers();
        }

        private void SpawnPlayers()
        {
            var players = _networkRunner.ActivePlayers.ToList();
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

            // Choose the Runner once so a retry cannot swap roles after a partial spawn.
            _runnerPlayer ??= players[Random.Range(0, 2)];

            var runnerPlayer = _runnerPlayer.Value;
            var chaserPlayer = players[0] == runnerPlayer ? players[1] : players[0];

            var runnerObject = SpawnPlayer(runnerPlayer, PlayerRole.Runner, 0);
            var chaserObject = SpawnPlayer(chaserPlayer, PlayerRole.Chaser, 1);

            // Roles are assigned only after both objects have spawned successfully.
            if (runnerObject == null || chaserObject == null) return;

            runnerObject.GetComponent<Player>().Role = PlayerRole.Runner;
            chaserObject.GetComponent<Player>().Role = PlayerRole.Chaser;
            
            EventBus.Raise(new PlayersSpawnedEvent(runnerObject, chaserObject));
            return;

            NetworkObject SpawnPlayer(PlayerRef player, PlayerRole role, int spawnPointIndex)
            {
                // This also makes retries safe if one spawn succeeds and the other fails.
                if (_spawnedPlayers.TryGetValue(player, out var existingObject) && existingObject != null)
                    return existingObject;

                var spawnPoint = spawnPoints[spawnPointIndex];
                var playerObject = _networkRunner.Spawn(
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

        private void SpawnExitPod()
        {
            NetworkObject pod = _networkRunner.Spawn(exitPodPrefab);
        }

        public override void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (!runner.IsServer) return;

            if (!_spawnedPlayers.TryGetValue(player, out var playerObject)) return;
            runner.Despawn(playerObject);
            _spawnedPlayers.Remove(player);
        }

        private void OnRunPhaseStarts(RunPhaseStartsEvent ev) => RpcStartsRunPhase();
        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RpcStartsRunPhase() => _runPhaseItems.SetActive(true);
    }
}
