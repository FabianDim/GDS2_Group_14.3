using System;
using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using Random = UnityEngine.Random;

namespace _Experimenation.K.Game_Manager.Scripts
{
    public struct PlayerData : INetworkStruct, IEquatable<PlayerData>
    {
        public NetworkString<_32> Username;
        public int Points;
        public int Score;
        public PlayerRole Role;

        public static bool operator ==(PlayerData x, PlayerData y) =>
            x.Equals(y);
        public static bool operator !=(PlayerData x, PlayerData y) =>
            !(x == y);

        public bool Equals(PlayerData other)
        {
            return Points == other.Points && Score == other.Score && Role == other.Role;
        }
        
        public override bool Equals(object obj)
        {
            return obj is PlayerData other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(Points, Score, (int)Role);
        }
    }
    
    public class GameData : NetworkBehaviour
    {
        public static GameData Instance;
        
        [Networked] public ref PlayerData P1Data => ref MakeRef<PlayerData>();
        [Networked] public ref PlayerData P2Data => ref MakeRef<PlayerData>();
        public int numberOfRounds;
        public int currentRound = 1;

        public override void Spawned()
        {
            if (!Instance && FindAnyObjectByType<GameData>() != this)
                Runner.Despawn(Object);
            else Instance = this;
            Runner.MakeDontDestroyOnLoad(gameObject);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void SetUsernameRpc(NetworkString<_32> username, RpcInfo info = default)
        {
            if (info.Source == Runner.LocalPlayer)
                P1Data.Username = username;
            else
                P2Data.Username = username;
        }

        public void GenerateRole()
        {
            if (P1Data.Role == 0 || P2Data.Role == 0)
            {
                P1Data.Role = (PlayerRole)Random.Range(1, 3);
                P2Data.Role = P1Data.Role == PlayerRole.Chaser ? PlayerRole.Runner : PlayerRole.Chaser;
            }
            else
                (P1Data.Role, P2Data.Role) = (P2Data.Role, P1Data.Role);
        }

        public void UpdateScore(bool runnerScores)
        {
            var runner = P1Data.Role == PlayerRole.Runner ? P1Data : P2Data;
            var chaser = runner == P1Data ? P2Data : P1Data;
            if(runnerScores) runner.Score++;
            else chaser.Score++;
        }
    }
}
