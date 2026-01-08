using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class SavePoint : SceneItem,IInteractable
{
    public Sound saveSound;
    public GameObject campFire;
    public void OnPlayerEnter(GameObject player)
    {
        
    }

    public void OnPlayerExit(GameObject interactor)
    {

    }

    public void Interact()
    {
        campFire.SetActive(true);
        SoundManager.Instance.PlaySound(saveSound);
        GameManager.Instance.SaveGame(Player.instance.gameObject);
    }

    public void Interact(GameObject gameObject)
    {
        throw new System.NotImplementedException();
    }
}
