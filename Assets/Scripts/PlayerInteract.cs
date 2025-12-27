using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerInteract：负责玩家的攻击判定和受伤逻辑的集中放置点。
/// 关键责任：
/// - 管理玩家生命与受伤（invincibleDuration）
/// - 管理攻击判定（hitbox），在攻击开始时增加 attack id 并传递给 hitbox
/// - 提供外部接口给动画或 Player 控制器调用（例如 Attack(true/false)）
/// 
/// 设计要点（重要）：
/// - 当玩家被多个敌人同时攻击时，避免多次扣血的核心在于这里的 invincibleDuration（短暂无敌）
///   即：Enemy.Hurt 会调用 PlayerInteract.Hurt，Hurt 内部会检测 invincibleUntil 来决定是否扣血，从而实现“同时被多个敌人打只扣一次血”。
/// - 玩家每次发起攻击时会自增 currentAttackId 并把该 id 传给 Hitbox；Hitbox 会在与敌人碰撞时把该 id 发给 Enemy.ReceiveHit，
///   Enemy 会根据 attackId 去重，保证同一次攻击对同一个敌人只造成一次伤害。
/// </summary>
public class PlayerInteract : MonoBehaviour
{

    [SerializeField] private GameObject hitbox; // 玩家攻击时用到的碰撞体（通常为子对象），通过激活/禁用来控制判定时机

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
    // 说明：每次攻击（例如动画触发打开 hitbox）会自增该 id，确保同一次攻击的所有命中共享同一 id
    private int currentAttackId = 0;


    #region HurtPlayer Player

    /// <summary>
    /// 玩家受伤入口。注意：本方法负责实现短暂无敌（invincibleDuration），以避免在同一时间窗口内被多次扣血。
    /// 参数 damage：受到的伤害数值。
    /// </summary>
    public void Hurt(int damage)
    {
        float now = Time.time;
        if (now < invincibleUntil)
        {
            // Debug.Log($"Player: Ignored Hurt({damage}) due to invincibility ({invincibleUntil - now:0.00}s left)");
            return;
        }

        // 进入短暂无敌状态，确保接下来的一段时间内不会再被扣血
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
    /// isAttack = true 时：
    /// - 自增 attack id
    /// - 把 attack 信息（id、attacker、damage）写入 hitbox（Hitbox.SetAttackInfo）
    /// - 激活 hitbox（使其能与敌人产生触发）
    /// isAttack = false 时：
    /// - 关闭 hitbox
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
}

