using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    #region Hurt Player

    public void Hurt(int damage)
    {
        Debug.Log($"Player: Hurt called with damage {damage}");
    }


    #endregion


    #region Attack Enemy
    public void Attack(GameObject target)
    {
        Debug.Log($"Player: Attack called on target {target?.name}");
        IHurtPlayer hurtable = target.GetComponent<IHurtPlayer>();
        if (hurtable != null)
        {
            hurtable.Hurt(this.gameObject);
        }
    }
    #endregion
}
