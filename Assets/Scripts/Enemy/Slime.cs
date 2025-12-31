using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 史莱姆敌人 - 简单的横向巡逻AI。
/// 设计目标：
/// - 在同一平面（水平）内朝一个方向移动（使用 Rigidbody2D.velocity）
/// - 前方如果检测到墙体（基于 LayerMask）或没有地面时，先短暂停顿（可配置），然后反向移动
/// - 采用合理的物理方式设置速度（不直接修改 transform），并与基类 SceneItem 的 Rb 兼容
/// - 提供 Inspector 可调参数：moveSpeed、groundCheckDistance、wallCheckDistance、pauseOnTurn
///
/// 使用方法：在敌人预制体上挂载本脚本，并确保存在 Rigidbody2D、Collider2D；将 groundLayer 和 obstacleLayer 设置为地面/障碍层。
/// </summary>
public class Slime : Enemy
{
    [Header("Movement")]
    [SerializeField, Tooltip("巡逻速度（单位：world units/s）")] private float moveSpeed = 1.5f;
    [SerializeField, Tooltip("检测地面的距离（从 groundCheckPoint 向下）")] private float groundCheckDistance = 0.6f;
    [SerializeField, Tooltip("检测前方墙体的距离（从 wallCheckPoint 向前）")] private float wallCheckDistance = 0.4f;
    [SerializeField, Tooltip("停顿时间（当遇到墙/无地面时短暂停顿再掉头）")] private float pauseOnTurn = 0.4f;

    [SerializeField, Tooltip("地面检测点（通常放在脚下）")] private Transform groundCheckPoint;
    [SerializeField, Tooltip("前方检测点（通常放在脚前）")] private Transform wallCheckPoint;

    [SerializeField, Tooltip("地面层（用于检测是否还有地面）")] private LayerMask groundLayer;
    [SerializeField, Tooltip("障碍层（用于检测墙体、不可通过的物体）")] private LayerMask obstacleLayer;
    
    [SerializeField]
    [Header("角色朝向，1=右，-1=左")]
    private int moveDir = 1; 
    private bool isPaused = false;
    
    protected Rigidbody2D Rb;
    
    protected  void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        
        // 如果没有设置检测点，自动创建
        if (groundCheckPoint == null)
        {
            GameObject go = new GameObject("GroundCheckPoint");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            groundCheckPoint = go.transform;
        }
        if (wallCheckPoint == null)
        {
            GameObject go = new GameObject("WallCheckPoint");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0.5f, 0f, 0f);
            wallCheckPoint = go.transform;
        }
    }

    private void FixedUpdate()
    {
        // 如果暂停中，不移动
        if (isPaused)
        {
            if (Rb != null)
            {
                Rb.velocity = new Vector2(0f, Rb.velocity.y);
            }
            return;
        }

        // 检测前方是否有墙或没有地面
        bool needTurn = CheckWallAhead() || !CheckGroundAhead();

        if (needTurn)
        {
            StartCoroutine(TurnCoroutine());
            return;
        }

        // 正常移动（保持 y 方向速度不变）
        if (Rb != null)
        {
            Rb.velocity = new Vector2(moveDir * moveSpeed, Rb.velocity.y);
        }
    }

    // 检测前方墙体（使用 wallCheckPoint 向 moveDir 方向射线检测）
    private bool CheckWallAhead()
    {
        if (wallCheckPoint == null) return false;
        Vector2 origin = wallCheckPoint.position;
        Vector2 dir = Vector2.right * moveDir;
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, wallCheckDistance, obstacleLayer);
        Debug.DrawRay(origin, dir * wallCheckDistance, hit.collider != null ? Color.red : Color.green);
        return hit.collider != null;
    }

    // 检测前方是否还有地面（从 groundCheckPoint 往下偏移到前方一点）
    private bool CheckGroundAhead()
    {
        if (groundCheckPoint == null) return true;
        Vector2 origin = groundCheckPoint.position + Vector3.right * moveDir * 0.2f;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);
        Debug.DrawRay(origin, Vector2.down * groundCheckDistance, hit.collider != null ? Color.green : Color.red);
        return hit.collider != null;
    }

    // 停顿并反向
    private IEnumerator TurnCoroutine()
    {
        isPaused = true;
        // 停顿一小会儿
        yield return new WaitForSeconds(pauseOnTurn);
        // 反向
        moveDir *= -1;
        // 翻转视觉（若需要翻转 Sprite，调整 transform）
        transform.rotation = Quaternion.Euler(0f, moveDir == 1 ? 0f : 180f, 0f);
        isPaused = false;
    }
}
