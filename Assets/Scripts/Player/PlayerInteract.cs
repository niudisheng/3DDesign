using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{

    [SerializeField] private GameObject hitbox;

    [Header("Player Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;

    [Header("Damage Settings")]
    [SerializeField, Tooltip("Seconds of invincibility after taking a hit")] private float invincibleDuration = 1.0f;

    // attack damage
    [SerializeField, Tooltip("Damage dealt by player's basic attack")] private int attackDamage = 10;
    
    // track invincibility end time
    private float invincibleUntil = 0f;

    // attack id generator: increments each attack activation so enemies can dedupe by this id
    private int currentAttackId = 0;

    

    #region 受伤与死亡代码
    
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
        
        GameManager.Instance.LoadGame();
        
    }

    public void InitPlayer(SaveData saveData)
    {
        currentHealth = maxHealth;
        transform.position = saveData.playerPosition;
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
        {
            // When enabling the hitbox, increment attack id so enemies can dedupe by this id
            if (isAttack)
            {
                currentAttackId++;
                // Pass attack id to Hitbox component if present
                var hb = hitbox.GetComponent<global::Hitbox>();
                if (hb != null)
                {
                    hb.SetAttackInfo(currentAttackId, gameObject, attackDamage);
                }
            }
            hitbox.SetActive(isAttack);
        }
    }

    // Provide accessors for hitbox/attack info if other systems need them
    public int GetCurrentAttackId() => currentAttackId;

    #endregion

    #region Interact
    private  IInteractable currentItem;
    [SerializeField, Tooltip("可交互标识")] private GameObject InteractableIcon;

    private void Start()
    {
        InteractableIcon.SetActive(false);
    }

    public void TryInteract()
    {
        if (currentItem != null)
        {
            currentItem.Interact();
        }
    }
    
    // 注意：Unity 的触发器回调函数签名要求 Collider2D，而不是 Collision2D。
    // 触发器生效的基本规则（2D 物理）:
    //  - 要触发 OnTriggerEnter2D/Stay2D/Exit2D，至少一方必须有 Rigidbody2D（kinematic 或 dynamic），
    //    且触发器对象的 Collider2D 的 'Is Trigger' 勾选为 true。
    //  - 常见设置：玩家带 Rigidbody2D（常设为 Kinematic），玩家的 Collider2D 不是触发器；
    //    可拾取物品或交互体的 Collider2D 设置为 Is Trigger = true，这样玩家就能触发它们的触发器事件。
    //  - 如果都没有 Rigidbody2D，触发器/碰撞事件不会被调用。
    //  - 也要检查 Physics2D 的图层碰撞矩阵（Project Settings -> Physics2D）是否允许这两个图层发生碰撞/触发。
    
    // 将 Collision2D 改为 Collider2D，确保触发器能被检测到
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Debug.Log("PlayerInteract: OnTriggerStay2D with " + other.gameObject.name);
        var interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            // 如果检测到可交互对象，就保存引用（供 TryInteract 使用）
            currentItem = interactable;
            InteractableIcon.SetActive(true);
        }
    }

    // 当离开触发器时清理 currentItem，避免残留引用
    private void OnTriggerExit2D(Collider2D other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable != null && currentItem == interactable)
        {
            currentItem = null;
            InteractableIcon.SetActive(false);
        }
    }
    #endregion
}