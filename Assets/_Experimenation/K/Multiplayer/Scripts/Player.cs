using Fusion;

namespace _Experimenation.K.Multiplayer.Scripts
{
    public enum PlayerRole {Runner, Chaser}
    
    public class Player : NetworkBehaviour
    {
        [Networked] public PlayerRole Role { get; set; }
    }
}
