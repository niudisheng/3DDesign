using System;
using Game;
using UnityEngine;

public class Enemy : SceneItem, IInteractable, IHurtPlayer
{
    [SerializeField]
    private int damage = 10;
    private void OnCollisionEnter2D(Collision2D other)
    {
        OnPlayerEnter(other.gameObject);
    }

    public void OnPlayerEnter(GameObject interactor)
    {
        Debug.Log($"Enemy: Player entered interaction range. player={interactor?.name}");
        if (interactor != null && interactor.CompareTag("Player"))
        {
            PlayerInteract player = interactor.GetComponent<PlayerInteract>();
            player.Hurt(damage);
        }
        
    }

    public void OnPlayerExit(GameObject interactor)
    {
        Debug.Log($"Enemy: Player exited interaction range. player={interactor?.name}");
        // TODO: stop aggro, reset state, etc.
    }

    public void Interact(GameObject interactor)
    {
        Debug.Log($"Enemy: Interact called by {interactor?.name}");
        // TODO: interaction behavior (talk, attack trigger, loot, etc.)
    }

    public void Hurt(GameObject interactor)
    {
        Debug.Log($"Enemy: Hurt called on actor {interactor?.name}");
        // TODO: apply damage logic to the actor if it's a player
    }
}
