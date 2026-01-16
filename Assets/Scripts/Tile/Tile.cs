using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Tile : MonoBehaviour
{
    // 注意：Unity2D 碰撞回调需要满足以下条件之一：
    // - 两个对象的 Collider2D 都为非 trigger，且至少一个对象有 Rigidbody2D（动态或运动学/静态的组合依照 Unity 物理规则）。
    // - 如果使用 isTrigger = true，则会调用 OnTriggerEnter2D / OnTriggerStay2D / OnTriggerExit2D，而不是 OnCollisionEnter2D / ...
    // 常见问题排查（按顺序）:
    // 1) 确保玩家或 Tile 至少有一个 Rigidbody2D（动态或运动学），并且不是被错误地设为 "Is Trigger"（如果你想用碰撞而非触发）。
    // 2) 检查对象的 Layer 是否在 Physics2D 设置中被允许相互碰撞。
    // 3) 确保脚本和 Collider2D 都启用且 GameObject 处于激活状态。
    // 4) 确认使用的是 2D 回调（OnCollisionEnter2D / OnTriggerEnter2D），而非 3D 版本。

    // 如果 OnCollisionEnter2D 没有触发，这个类会在回调中输出详细信息，方便定位问题。

    protected virtual void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            OnEnter(other);
        }
    }

    // 保持原有抽象接口，供子类实现：这是非 trigger 碰撞时的回调。
    protected virtual void OnEnter(Collision2D other)
    {
    }


    protected virtual void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            OnStay(other);
        }
    }

    protected virtual void OnStay(Collision2D other)
    {
    }

    protected virtual void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            OnExit(other);
        }
    }

    protected virtual void OnExit(Collision2D other)
    {
    }


    // 下面为 trigger 的代理方法。如果 Tile 的 Collider2D 被设置为 isTrigger = true，
    // Unity 不会调用 OnCollisionEnter2D，而会调用下面这些方法。为了兼容两种情况，我们
    // 在 trigger 回调里记录日志并且调用一个可选的虚方法（子类如需处理 trigger 请重写）。

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // Debug.Log($"[Tile] OnTriggerEnter2D called on '{gameObject.name}' with '{other.gameObject.name}'. Components: " +
                  // $"HasCollider={GetComponent<Collider2D>()!=null}, IsTrigger={GetComponent<Collider2D>()?.isTrigger}, OtherIsTrigger={other.isTrigger}");

        if (other.gameObject.CompareTag("hitbox"))
        {
            // 子类可通过重写以下方法处理 trigger 情况
            OnEnterTrigger(other);
        }
    }

    protected virtual void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("hitbox"))
        {
            OnStayTrigger(other);
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("hitbox"))
        {
            OnExitTrigger(other);
        }
    }

    // 可选的 trigger 处理接口，默认空实现，子类按需重写
    protected virtual void OnEnterTrigger(Collider2D other) { }
    protected virtual void OnStayTrigger(Collider2D other) { }
    protected virtual void OnExitTrigger(Collider2D other) { }
}
