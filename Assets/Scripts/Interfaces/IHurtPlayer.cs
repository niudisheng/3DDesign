using UnityEngine;

namespace Game
{
    /// <summary>
    /// Contract for objects that can hurt a player or other actor.
    /// Implementations receive the actor GameObject which they can inspect for player components.
    /// </summary>
    public interface IHurtPlayer
    {
        void HurtPlayer(GameObject actor);
    }
}
