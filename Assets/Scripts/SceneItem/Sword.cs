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
        GameManager.Instance.ChangePlayer();
    }

    public void Interact(GameObject gameObject)
    {
        

    }
}
