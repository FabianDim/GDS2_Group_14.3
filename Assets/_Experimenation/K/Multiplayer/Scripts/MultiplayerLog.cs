using Fusion;

namespace _Experimenation.K.Multiplayer.Scripts
{
    public static class MultiplayerLog
    {
        private static string _shutdownReason;
        
        public static void LogShutdown(NetworkRunner runner, ShutdownReason reason) => 
            _shutdownReason = $"{runner.LocalPlayer} rage quit";

        public static void ClearLog()
        {
            _shutdownReason = null;
        }
        
        public static string GetLog()
        {
            return _shutdownReason;
        }
    }
}
