using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : Tile
{
    protected override void OnStay(Collision2D other)
    {
        base.OnEnter(other);
        // 玩家进入 Spike 碰撞时的逻辑
        Debug.Log("Player entered Spike!");
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerInteract playerInteract = other.gameObject.GetComponent<PlayerInteract>();
            if (playerInteract != null)
            {
                playerInteract.Hurt(10, this.transform); 
            }
        }
    }
}