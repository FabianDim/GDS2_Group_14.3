using _Experimenation.K.Multiplayer.Scripts;
using Fusion;

public class PlayersSpawnedEvent
{
    public readonly NetworkObject Runner, Chaser;

    public PlayersSpawnedEvent(NetworkObject runner, NetworkObject chaser)
    {
        Runner = runner;
        Chaser = chaser;
    }
}
