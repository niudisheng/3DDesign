using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class SavePoint : SceneItem,IInteractable
{
    public void OnPlayerEnter(GameObject player)
    {
        
    }

    public void OnPlayerExit(GameObject interactor)
    {

    }

    public void Interact()
    {
        throw new System.NotImplementedException();
    }

    public void Interact(GameObject gameObject)
    {
        GameManager.Instance.SaveGame(gameObject);
        
    }
}
