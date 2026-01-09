using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class teleportationPoint : SceneItem,IInteractable
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
        SceceLoadManager.LoadScene(GlobalValues.SceneData.Level2);
    }

    public void Interact(GameObject gameObject)
    {
        throw new System.NotImplementedException();
    }
}