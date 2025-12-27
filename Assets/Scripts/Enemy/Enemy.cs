// Enemy.cs
// 说明：本文件实现了敌人的伤害判定逻辑，使用可配置的触发器（Trigger hitbox）来判断玩家进入/停留/离开
// 主要功能：
// - 可在 Inspector 中配置伤害值 (damage)、对同一玩家的（可选）短内置冷却 (perActorCooldown)
// - 使用子对象的 Collider2D（isTrigger = true）作为敌人的伤害区域（damageTrigger）
// - 支持进入时立即伤害（autoHurtOnEnter）和停留时按配置持续伤害（stayingDamage）
// - 对每个被伤害的 actor 使用字典记录上一次受击时间（仅在 perActorCooldown>0 时启用），防止短时间内重复受伤
// - 当玩家离开触发区或敌人销毁时会清理记录，避免内存泄漏
// - 玩家真正的无敌处理（防止被多个敌人同时伤害造成多次扣血）应由 Player 端实现（PlayerInteract.invincibleDuration）

using System;
using System.Collections.Generic;
using Game; // 我们把接口放在 Game 命名空间下（例如 IInteractable, IHurtPlayer）
using UnityEngine;
using UnityEngine.Serialization;

public class Enemy : SceneItem, IInteractable, IHurtPlayer
{
    // 基础伤害值，可以在 Inspector 中调整
    [SerializeField]
    private int damage = 10;

    // 敌人生命值（玩家攻击会减少这里的血量）
    [Header("Enemy Stats")]
    [SerializeField, Tooltip("Enemy health")] private int maxHealth = 50;
    private int currentHealth;

    [Header("Damage Settings")]
    // perActorCooldown：两个连续对同一 actor 造成伤害的最小间隔（秒）——这是敌人侧的可选短 CD，用来避免同一敌人在一帧或连续触发中反复调用玩家受伤
    // 注：为了实现“玩家被多个敌人同时攻击时只受一次伤害”的需求，核心的无敌判定应在 PlayerInteract.Hurt 中处理（invincibleDuration）。
    [SerializeField, Tooltip("Optional: seconds between consecutive damage from this enemy to the same actor. Set to 0 to disable.")]
    private float perActorCooldown = 0f;

    [Header("Physics (Trigger Hitbox)")]
    [SerializeField, Tooltip("A Collider2D configured as a trigger that represents the enemy's damaging area. Assign a child collider set to 'isTrigger' = true.")]
    private Collider2D damageTrigger;

    [SerializeField, Tooltip("If true, the enemy will automatically call Hurt when a player enters the trigger. Otherwise you can call Hurt manually (e.g. by animation).")]
    private bool autoHurtOnEnter = false;

    [FormerlySerializedAs("StayingDamage")]
    [FormerlySerializedAs("damageWhileInside")]
    [SerializeField, Tooltip("If true, the enemy will keep attempting to damage the player while they remain inside the trigger (subject to perActorCooldown if set).")]
    private bool stayingDamage = true;

    // 用于记录每个 actor（如玩家）上次被本敌人伤害的时间（Time.time），以实现 per-actor 冷却（仅在 perActorCooldown>0 时启用）
    // key: 被伤害的 GameObject（使用 PlayerInteract.gameObject 作为 key），value: 上次受击时刻
    private Dictionary<GameObject, float> lastHitTime = new Dictionary<GameObject, float>();

    // 对于玩家的攻击（外部 hitbox 触发的 ReceiveHit），我们使用 attackId 来去重：同一 attackId 对同一个敌人只生效一次
    // processedAttackIds 保存最近处理过的 attackId，用于快速判断是否已处理
    private HashSet<int> processedAttackIds = new HashSet<int>();

    // 简单的 processed attack id 清理策略：当集合过大时清空（这里阈值可调），避免无限增长
    private const int ProcessedAttackIdThreshold = 1024;

    private void Reset()
    {
        if (damageTrigger == null)
        {
            damageTrigger = GetComponentInChildren<Collider2D>();
        }
        currentHealth = maxHealth;
    }

    // 确保调用基类 Awake（SceneItem 会缓存 Rigidbody2D 到 Rb）
    protected override void Awake()
    {
        base.Awake();
        // 初始化血量
        currentHealth = maxHealth;
    }

    // 使用 GetComponentInParent<PlayerInteract> 来统一识别玩家（避免子 Collider 导致的不同 GameObject 引用）
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        // 优先通过 PlayerInteract 组件来识别玩家
        var player = other.GetComponentInParent<PlayerInteract>();
        if (player != null)
        {
            GameObject playerRoot = player.gameObject;
            if (autoHurtOnEnter)
            {
                Hurt(playerRoot);
            }
            else
            {
                OnPlayerEnter(playerRoot);
            }
            return;
        }

        // 其他可能的交互物体可以在这里处理
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!stayingDamage) return;
        if (other == null) return;

        var player = other.GetComponentInParent<PlayerInteract>();
        if (player != null)
        {
            Hurt(player.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == null) return;
        var player = other.GetComponentInParent<PlayerInteract>();
        if (player != null)
        {
            GameObject playerRoot = player.gameObject;
            if (lastHitTime.ContainsKey(playerRoot))
            {
                lastHitTime.Remove(playerRoot);
            }
            OnPlayerExit(playerRoot);
        }
    }

    public void OnPlayerEnter(GameObject interactor)
    {
        if (interactor != null && interactor.GetComponentInParent<PlayerInteract>() != null)
        {
            if (autoHurtOnEnter)
                Hurt(interactor);
        }
    }

    public void OnPlayerExit(GameObject interactor)
    {
        // Debug.Log($"Enemy: Player exited interaction range. player={interactor?.name}");
        if (interactor != null && lastHitTime.ContainsKey(interactor))
        {
            lastHitTime.Remove(interactor);
        }
    }

    public void Interact(GameObject interactor)
    {
        Debug.Log($"Enemy: Interact called by {interactor?.name}");
    }

    // IHurtPlayer 接口实现：对指定 actor 造成伤害
    // 重要说明（设计决策）：
    // - 玩家是否真正受到伤害（避免同时被多个敌人重复扣血）应由玩家端的无敌机制来处理（PlayerInteract.Hurt 中的 invincibleDuration）。
    // - 本方法仅提供一个可选的每敌人内置短 CD（perActorCooldown），用于避免同一敌人在一帧内重复调用造成多次扣血或性能浪费。
    public void Hurt(GameObject actor)
    {
        if (actor == null) return;

        // 优先判断是否为玩家
        var player = actor.GetComponentInParent<PlayerInteract>();
        if (player != null)
        {
            // 如果设置了 per-actor 冷却，则按该冷却进行去重；否则直接调用玩家的 Hurt，由玩家自己处理无敌/扣血逻辑
            if (perActorCooldown > 0f)
            {
                GameObject key = player.gameObject;
                float now = Time.time;
                if (lastHitTime.TryGetValue(key, out float last))
                {
                    if (now - last < perActorCooldown)
                    {
                        return; // 敌人侧短 CD 生效，忽略本次调用
                    }
                }
                lastHitTime[key] = now;
            }
            
            // 直接调用玩家受伤，玩家会根据 invincibleDuration 决定是否真正扣血，从而实现“多个敌人同时打只扣一次血”的需求
            player.Hurt(damage);
            return;
        }

        // 如果不是玩家，也可以根据需要处理其他交互对象
        Debug.LogWarning($"Enemy: Actor {actor.name} has no PlayerInteract component to receive damage.");
    }

    // 新增：当受到玩家攻击（外部 hitbox）时调用。采用 attackId 去重，保证同一次攻击对本敌人只造成一次伤害。
    // attackId: 由攻击者在开启攻击时生成并传入（例如 PlayerInteract.currentAttackId）
    public void ReceiveHit(int attackId, GameObject attacker, int damageAmount)
    {
        // 简单去重：如果 processedAttackIds 包含该 attackId，则已处理
        if (processedAttackIds.Contains(attackId))
        {
            // Debug.Log($"Enemy: Already processed attackId={attackId}");
            return;
        }

        processedAttackIds.Add(attackId);
        // 清理 processedAttackIds（防止内存无限增长）
        if (processedAttackIds.Count > ProcessedAttackIdThreshold)
        {
            processedAttackIds.Clear();
        }

        // 处理受击：将伤害应用到本敌人（而不是反向伤害玩家）
        Debug.Log($"Enemy: ReceiveHit from {attacker?.name} attackId={attackId} damage={damageAmount}");

        TakeDamage(damageAmount, attacker);
    }

    // 敌人受伤逻辑：减少生命值，播放受击反馈，触发死亡时销毁
    private void TakeDamage(int amount, GameObject attacker)
    {
        currentHealth -= amount;
        Debug.Log($"Enemy: Took damage {amount}. Health now {currentHealth}/{maxHealth}");

        // TODO: 播放受击动画、音效、击退等，这里只做基础的逻辑

        if (currentHealth <= 0)
        {
            Die(attacker);
        }
    }

    private void Die(GameObject killer)
    {
        Debug.Log($"Enemy: Died. KilledBy={killer?.name}");
        // TODO: 播放死亡动画、掉落、移除对象等
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        lastHitTime.Clear();
        processedAttackIds.Clear();
    }
}
