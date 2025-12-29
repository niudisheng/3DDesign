using UnityEngine;

namespace Game
{
    /// <summary>
    /// Contract for scene items that can be interacted with by an actor (usually the player).
    /// The interactor is passed as a GameObject to avoid coupling to a concrete Player type.
    /// </summary>
    public interface IInteractable
    {
        void OnPlayerEnter(GameObject Player);
        void OnPlayerExit(GameObject interactor);
        void Interact();
        void Interact(GameObject interactor);
    }
}
