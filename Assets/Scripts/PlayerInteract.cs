using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public struct PlayerData
    {
        public int health;
        public int Atk;
    }

    [SerializeField] private GameObject hitbox;

    [Header("Player Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;

    [Header("Damage Settings")]
    [SerializeField, Tooltip("Seconds of invincibility after taking a hit")] private float invincibleDuration = 1.0f;

    // track invincibility end time
    private float invincibleUntil = 0f;


    #region HurtPlayer Player

    public void Hurt(int damage)
    {
        float now = Time.time;
        if (now < invincibleUntil)
        {
            Debug.Log($"Player: Ignored Hurt({damage}) due to invincibility ({invincibleUntil - now:0.00}s left)");
            return;
        }

        invincibleUntil = now + invincibleDuration;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        Debug.Log($"Player: HurtPlayer called with damage {damage}. Health now {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player: Died");
        // TODO: add death handling (respawn, disable input, animation, etc.)
    }

    #endregion


    #region Attack Enemy
    
    
    /// <summary>
    /// 根据动画帧打开或关闭攻击判定
    /// </summary>
    /// <param name="isAttack"></param>
    public void Attack(bool isAttack)
    {
        if (hitbox != null)
            hitbox.SetActive(isAttack);
    }

    #endregion
}