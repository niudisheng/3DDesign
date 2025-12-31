using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Effect = PlayerStateController.Effect;

public class SpeedDownTile : Tile
{
    protected override void OnEnter(Collision2D other)
    {
        
        other.gameObject.GetComponent<PlayerStateController>().PlayEffect(Effect.SpeedDown,true);
    }

    protected override void OnStay(Collision2D other)
    {
        throw new System.NotImplementedException();
    }

    protected override void OnExit(Collision2D other)
    {
        other.gameObject.GetComponent<PlayerStateController>().PlayEffect(Effect.SpeedDown,false);
    }
}
