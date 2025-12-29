using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class Sword : SceneItem,IInteractable
{
    public void OnPlayerEnter(GameObject Player)
    {
        throw new System.NotImplementedException();
    }

    public void OnPlayerExit(GameObject interactor)
    {
        throw new System.NotImplementedException();
    }

    public void Interact()
    {
        
    }

    public void Interact(GameObject interactor)
    {
        Player player =  interactor.GetComponent<Player>();
        player.SetSword(true);

    }
}
