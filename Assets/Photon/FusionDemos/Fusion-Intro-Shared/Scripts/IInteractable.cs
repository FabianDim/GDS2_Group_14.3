using Fusion;

namespace Photon.FusionDemos.Fusion_Intro_Shared.Scripts {
  
  /// <summary>
  /// Interface to indicate an object that the player can interact in the world.
  /// </summary>
  public interface IInteractable {
    public void Interact(NetworkObject interactingPlayer);
  }
}