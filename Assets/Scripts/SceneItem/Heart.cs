using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class Heart : SceneItem,IInteractable
{
    Animator animator ;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
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
        Player.instance.playerInteract.ChangeHealth(30);
        
        animator.SetTrigger("HeartFade");
        StartCoroutine(GameManager.Instance.WaitForAnimationEnd(
            "HeartFade", 
            () => { Destroy(this.gameObject); },animator
            ));

    }

    public void Interact(GameObject gameObject)
    {
        throw new System.NotImplementedException();
    }
}
