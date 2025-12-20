using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hitbox：附着在玩家攻击判定（hitbox）上，负责将攻击信息（attackId、attacker、damage）传递给被碰撞到的敌人。
/// 设计目标：同一次攻击（一次动画开启的 hitbox）对同一个敌人只造成一次伤害。
/// 使用方法：玩家在启动攻击时，增加 attackId 并通过 Hitbox.SetAttackInfo(...) 把攻击信息设置到 hitbox。
/// 当 hitbox 与敌人的 Collider2D 发生触发（Trigger）时，调用敌人的 ReceiveHit 方法。
/// </summary>
public class Hitbox : MonoBehaviour
{
    // 当前攻击 id（每次动画触发时递增）
    private int attackId = 0;
    // 攻击者（通常是玩家的根 GameObject）
    private GameObject attacker;
    // 伤害值
    private int damage = 10;

    // 记录已处理过的敌人，以防同一命中对一个敌人重复调用（基于 attackId + enemy）
    // 但主要的去重逻辑在 Enemy 侧（ReceiveHit）完成；这里备用以减少无效调用
    private HashSet<GameObject> localProcessed = new HashSet<GameObject>();

    public void SetAttackInfo(int id, GameObject attacker, int damage)
    {
        this.attackId = id;
        this.attacker = attacker;
        this.damage = damage;
        localProcessed.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        // 优先查找 Enemy 组件（可能在父对象上）
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null) return;

        GameObject enemyGo = enemy.gameObject;
        if (localProcessed.Contains(enemyGo)) return;
        localProcessed.Add(enemyGo);

        // 调用敌人的 ReceiveHit，敌人会根据 attackId 去重
        enemy.ReceiveHit(attackId, attacker, damage);
    }

    // 若 hitbox 也在停留期间造成多次触发，OnTriggerStay2D 可能也触发，我们通常只处理 Enter 即可
    private void OnTriggerStay2D(Collider2D other)
    {
        // Optional: do nothing (we handle on enter)
    }
}
