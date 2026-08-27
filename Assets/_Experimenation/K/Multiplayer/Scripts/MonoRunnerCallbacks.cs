using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace _Experimenation.K.Multiplayer.Scripts
{
    public class MonoRunnerCallbacks : MonoBehaviour, INetworkRunnerCallbacks
    {
        // --- Required INetworkRunnerCallbacks stubs (leave empty unless needed) ---
        public virtual void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public virtual void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public virtual void OnInput(NetworkRunner runner, NetworkInput input) { }
        public virtual void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public virtual void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public virtual void OnConnectedToServer(NetworkRunner runner) { }
        public virtual void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public virtual void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public virtual void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public virtual void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public virtual void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public virtual void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public virtual void OnSceneLoadDone(NetworkRunner runner) { }
        public virtual void OnSceneLoadStart(NetworkRunner runner) { }
        public virtual void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public virtual void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public virtual void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data) { }
        public virtual void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    }
}
