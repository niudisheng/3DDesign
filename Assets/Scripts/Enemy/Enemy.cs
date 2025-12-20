// Enemy.cs
// 说明：本文件实现了敌人的伤害判定逻辑，使用可配置的触发器（Trigger hitbox）来判断玩家进入/停留/离开
// 主要功能：
// - 可在 Inspector 中配置伤害值 (damage)、对同一玩家的伤害冷却时间 (hitCooldown)
// - 使用子对象的 Collider2D（isTrigger = true）作为敌人的伤害区域（damageTrigger）
// - 支持进入时立即伤害（autoHurtOnEnter）和停留时按冷却持续伤害（damageWhileInside）
// - 对每个被伤害的 actor 使用字典记录上一次受击时间，防止短时间内重复受伤
// - 当玩家离开触发区或敌人销毁时会清理记录，避免内存泄漏

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

    [Header("Damage Settings")]
    // hitCooldown：两个连续对同一 actor 造成伤害的最小间隔（秒）
    [SerializeField, Tooltip("Seconds between consecutive damage to the same interactor")] 
    private float hitCooldown = 1.0f;

    [Header("Physics (Trigger Hitbox)")]
    // damageTrigger：在场景中将子物体的 Collider2D（Is Trigger = true）拖到这里
    // 该 Collider2D 定义了敌人的伤害范围（例如近战的攻击范围或持续伤害区域）
    [SerializeField, Tooltip("A Collider2D configured as a trigger that represents the enemy's damaging area. Assign a child collider set to 'isTrigger' = true.")]
    private Collider2D damageTrigger;

    // autoHurtOnEnter：当 Player 进入触发器时是否自动调用 Hurt 造成伤害
    // 如果你需要在特定动画帧触发伤害，可以把这个设为 false 并在动画事件或代码里手动调用 Hurt()
    [SerializeField, Tooltip("If true, the enemy will automatically call Hurt when a player enters the trigger. Otherwise you can call Hurt manually (e.g. by animation).")]
    private bool autoHurtOnEnter = false;

    // StayingDamage：如果为 true，则当 Player 停留在触发器内时会持续尝试调用 Hurt()（但 Hurt 本身会依据 hitCooldown 判断）
    [FormerlySerializedAs("StayingDamage")] [FormerlySerializedAs("damageWhileInside")] [SerializeField, Tooltip("If true, the enemy will keep attempting to damage the player while they remain inside the trigger (subject to hitCooldown).")]
    private bool stayingDamage = true;

    // 用于记录每个 actor（如玩家）上次被本敌人伤害的时间（Time.time），以实现 per-actor 冷却
    // key: 被伤害的 GameObject，value: 上次受击时刻
    private Dictionary<GameObject, float> lastHitTime = new Dictionary<GameObject, float>();

    // Reset 在编辑器上添加组件时调用，尝试自动寻找位于子对象的 Collider2D 以便快速配置
    private void Reset()
    {
        if (damageTrigger == null)
        {
            damageTrigger = GetComponentInChildren<Collider2D>();
        }
    }

    // 使用 Trigger 来判断玩家是否进入伤害范围：要生效需要如下物理设置之一
    // - 玩家有 Rigidbody2D（通常是），且玩家或敌人的触发器 Collider2D 的 isTrigger = true
    // - Unity 的 2D 碰撞规则：至少一个碰撞体拥有 Rigidbody2D 才能触发 OnTriggerEnter2D/Stay/Exit
    // 请在 Inspector 中把玩家 GameObject 的 Tag 设置为 "Player"，或者改用 LayerMask 以更精确控制
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;
        GameObject otherGo = other.gameObject;

        // 这里使用 Tag 判断是否为玩家（轻量且直观）。如果你更偏好 LayerMask，我可以帮你改成 Layer 判断。
        if (otherGo.CompareTag("Player"))
        {
            Debug.Log($"Enemy: Trigger enter by {otherGo.name}");

            // autoHurtOnEnter 为 true 时会立刻调用 Hurt，触发一次受击
            if (autoHurtOnEnter)
            {
                Hurt(otherGo);
            }
            else
            {
                // 如果不自动伤害，仍调用 OnPlayerEnter 保持兼容（可以用来播放提示、开始蓄力等）
                OnPlayerEnter(otherGo);
            }
        }
    }

    // 可选：当玩家停留在触发区内，每帧（物理帧）都会调用 OnTriggerStay2D
    // 如果 StayingDamage 开启，那么会不断尝试调用 Hurt，Hurt 会根据 hitCooldown 决定是否真正造成伤害
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!stayingDamage) return;
        if (other == null) return;
        GameObject otherGo = other.gameObject;
        if (otherGo.CompareTag("Player"))
        {
            // Hurt 内部会判断 cooldown，保证不会过快重复伤害
            Hurt(otherGo);
        }
    }

    // 当玩家离开触发区时，清理 lastHitTime 中的记录，避免字典长期持有引用
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == null) return;
        GameObject otherGo = other.gameObject;
        if (otherGo.CompareTag("Player"))
        {
            Debug.Log($"Enemy: Trigger exit by {otherGo.name}");

            // 从字典中移除以释放引用并重置该玩家被本敌人的冷却状态
            if (lastHitTime.ContainsKey(otherGo))
            {
                lastHitTime.Remove(otherGo);
            }

            // 触发 OnPlayerExit 回调（兼容旧逻辑或用于状态清理）
            OnPlayerExit(otherGo);
        }
    }

    // 兼容接口 IInteractable：玩家进入范围时的回调（可被外部调用或保留以兼容其它系统）
    public void OnPlayerEnter(GameObject interactor)
    {
        Debug.Log($"Enemy: Player entered interaction range. player={interactor?.name}");
        if (interactor != null && interactor.CompareTag("Player"))
        {
            if (autoHurtOnEnter)
                Hurt(interactor);
        }
    }

    // 兼容接口 IInteractable：玩家离开范围时的回调
    public void OnPlayerExit(GameObject interactor)
    {
        Debug.Log($"Enemy: Player exited interaction range. player={interactor?.name}");
        // OnTriggerExit2D 已经做了字典清理，这里做一次保险性的移除
        if (interactor != null && lastHitTime.ContainsKey(interactor))
        {
            lastHitTime.Remove(interactor);
        }
    }

    // 兼容接口 IInteractable：交互（非伤害）回调，供对话、拾取等使用
    public void Interact(GameObject interactor)
    {
        Debug.Log($"Enemy: Interact called by {interactor?.name}");
        // TODO: 根据需要实现对话/拾取/触发事件等交互逻辑
    }

    // IHurtPlayer 接口实现：对指定 actor 造成伤害（遵循 per-actor 冷却）
    // 该函数负责：
    //  - 检查 actor 是否在冷却期内（若是，则跳过并记录日志）
    //  - 更新 lastHitTime 并尝试将伤害交给 actor 上的 PlayerInteract 组件处理
    public void Hurt(GameObject actor)
    {
        if (actor == null) return;

        float now = Time.time;

        // 如果字典中已有上次受击时间且未过冷却，则跳过造成伤害
        if (lastHitTime.TryGetValue(actor, out float last))
        {
            if (now - last < hitCooldown)
            {
                Debug.Log($"Enemy: Skip hurting {actor.name}, cooldown ({now - last:0.00}s) < {hitCooldown}s");
                return;
            }
        }

        // 更新该 actor 的上次受击时间为当前时间
        lastHitTime[actor] = now;

        Debug.Log($"Enemy: Hurt called on actor {actor.name}. Damage={damage}");

        // 如果 actor 上有 PlayerInteract 组件，则调用其 Hurt(int) 方法处理血量与受击无敌窗
        PlayerInteract player = actor.GetComponent<PlayerInteract>();
        if (player != null)
        {
            player.Hurt(damage);
        }
        else
        {
            // 如果没有 PlayerInteract，则记录警告，避免出现 NullReferenceException
            Debug.LogWarning($"Enemy: Actor {actor.name} has no PlayerInteract component to receive damage.");
        }
    }
    
    // 敌人销毁时清理字典，避免对已销毁对象继续持有引用
    private void OnDestroy()
    {
        lastHitTime.Clear();
    }
}
