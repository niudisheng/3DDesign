using System.Collections;
using UnityEngine;

/// <summary>
/// 史莱姆敌人 - 简单的横向巡逻AI。
/// 设计目标：
/// - 在同一平面（水平）内朝一个方向移动（使用 Rigidbody2D.velocity）
/// - 前方如果检测到墙体（基于 LayerMask）或没有地面时，先短暂停顿（可配置），然后反向移动
/// - 采用合理的物理方式设置速度（不直接修改 transform），并与基类 SceneItem 的 rb 兼容
/// - 提供 Inspector 可调参数：moveSpeed、groundCheckDistance、wallCheckDistance、pauseOnTurn
///
/// 使用方法：在敌人预制体上挂载本脚本，并确保存在 Rigidbody2D、Collider2D；将 groundLayer 和 obstacleLayer 设置为地面/障碍层。
/// </summary>
public class Slime : Enemy
{
    [Header("Movement")] [SerializeField, Tooltip("巡逻速度（单位：world units/s）")]
    private float moveSpeed = 1.5f;

    [SerializeField, Tooltip("检测地面的距离（从 groundCheckPoint 向下）")]
    private float groundCheckDistance = 0.6f;

    [SerializeField, Tooltip("检测前方墙体的距离（从 wallCheckPoint 向前）")]
    private float wallCheckDistance = 0.4f;

    [SerializeField, Tooltip("停顿时间（当遇到墙/无地面时短暂停顿再掉头）")]
    private float pauseOnTurn = 0.4f;

    // 新增：跳跃相关参数
    [SerializeField, Tooltip("单次跳跃的水平速度（用于设置 x 分量）")]
    private float hopHorizontalSpeed = 1.2f;

    [SerializeField, Tooltip("单次跳跃的垂直速度（用于设置 y 分量）")]
    private float hopVerticalVelocity = 3f;

    [SerializeField, Tooltip("跳跃后的冷却时间（在冷却期间不再发起下一次跳跃）")]
    private float hopCooldown = 1.6f;

    [SerializeField, Tooltip("着地检测半径（用于 OverlapCircle）")]
    private float groundCheckRadius = 0.12f;

    [SerializeField, Tooltip("地面检测点（通常放在脚下）")]
    private Transform groundCheckPoint;

    [SerializeField, Tooltip("前方检测点（通常放在脚前）")]
    private Transform wallCheckPoint;

    [SerializeField, Tooltip("地面层（用于检测是否还有地面）")]
    private LayerMask groundLayer;

    [SerializeField, Tooltip("障碍层（用于检测墙体、不可通过的物体）")]
    private LayerMask obstacleLayer;

    [SerializeField] [Header("角色朝向，1=右，-1=左")]
    private int moveDir = 1;

    private bool isPaused = false;


    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();

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


    protected override void StartIntention()
    {
        // 如果未着地，不发起新的跳跃，允许物理自然下落/移动
        if (!IsGrounded())
        {
            animator.SetFloat("Jump Or Down", 1);
            return;
        }
        else
        {
            animator.SetFloat("Jump Or Down", 0);
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }

        if (isPaused)
        {
            return;
        }
        else
        {
            MoveOrTurn();
        }
    }

    private void MoveOrTurn()
    {
        // 检测前方是否有墙或没有地面
        bool needTurn = CheckWallAhead() || !CheckGroundAhead();

        if (needTurn)
        {
            StartCoroutine(TurnCoroutine());
            return;
        }


        // 发起一次跳跃：设置瞬时速度（x 和 y）然后进入冷却
        if (rb != null)
        {
            rb.velocity = new Vector2(moveDir * hopHorizontalSpeed, hopVerticalVelocity);
        }

        StartCoroutine(HopCooldown());
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

    // 着地检测（用于判断是否可以发起下一次跳跃）
    private bool IsGrounded()
    {
        if (Mathf.Approximately(0f, rb.velocity.y))
        {
            Collider2D hit = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
            return hit != null;
        }
        return false;
    }

    // 跳跃冷却协程
    private IEnumerator HopCooldown()
    {
        isPaused = true;
        yield return new WaitForSeconds(hopCooldown);
        isPaused = false;
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

    [ContextMenu("Die")]
    public void DieTest()
    {
        Die(this.gameObject);
    }

    // 编辑器模式下画出检测辅助
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }

        if (wallCheckPoint != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 dir = Vector3.right * moveDir * wallCheckDistance;
            Gizmos.DrawLine(wallCheckPoint.position, wallCheckPoint.position + dir);
        }
    }
}