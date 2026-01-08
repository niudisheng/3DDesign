using UnityEngine;

public class Spider : Enemy
{
    [Header("Patrol")] [SerializeField, Tooltip("周期（s）：每隔多少秒发起一次短暂移动")]
    private float patrolInterval = 2f;

    [SerializeField, Tooltip("移动持续时间（s)")] private float moveDuration = 0.6f;
    [SerializeField, Tooltip("巡逻水平速度")] private float patrolSpeed = 2f;

    [SerializeField, Tooltip("初始朝向，1=右，-1=左")]
    private int patrolDir = 1;

    [Header("Chase")] [SerializeField, Tooltip("侦测玩家的半径（进入则开始追击）")]
    private float detectionRadius = 3f;

    [SerializeField, Tooltip("追击时的水平速度")] private float chaseSpeed = 3f;

    [SerializeField, Tooltip("当玩家离开侦测半径多远才停止追击（额外的回退距离）")]
    private float chaseExitBuffer = 0.5f;

    [Header("Debug / Layers")] [SerializeField, Tooltip("用于检测玩家的 LayerMask（或使用 Tag 'Player'）")]
    private LayerMask playerLayer;

    [SerializeField, Tooltip("是否使用 Tag 来检测玩家（优先于 LayerMask）")]
    private bool useTagDetection = true;

    [SerializeField, Tooltip("目标 Tag 名称，默认 'Player'")]
    private string targetTag = "Player";

    [SerializeField, Tooltip("是否开启调试日志")] private bool debugLogs = false;



    // 状态
    private float patrolTimer;
    private float moveTimer;
    private bool isMoving;
    private bool isChasing;
    private Transform targetPlayer;

    // 可复用的 Overlap 缓冲（非必须，但避免频繁分配）
    private Collider2D[] overlapBuffer = new Collider2D[8];

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        patrolTimer = 0f;
        moveTimer = 0f;
        isMoving = false;
        isChasing = false;
    }

    protected override void StartIntention()
    {
        // 1) 检测玩家（如果找到则设置追击目标）
        Transform detected = DetectPlayer();
        if (detected != null)
        {
            StartChase(detected);
        }
        else
        {
            UpdateChaseExit();
        }

        // 2) 执行当前状态逻辑
        if (isChasing && targetPlayer != null)
        {
            HandleChase();
        }
        else
        {
            HandlePatrol();
        }
    }


    // 封装玩家检测逻辑：根据 useTagDetection 或 LayerMask 返回第一个符合条件的 Transform（或 null）
    private Transform DetectPlayer()
    {
        if (useTagDetection)
        {
            // small optimization: use non-alloc version
            int n = Physics2D.OverlapCircleNonAlloc(transform.position, detectionRadius, overlapBuffer);
            if (debugLogs)
                Debug.Log(
                    $"Spider[{gameObject.name}] DetectPlayer: OverlapCircleNonAlloc found {n} colliders (bufferSize={overlapBuffer.Length})");
            for (int i = 0; i < n; i++)
            {
                Collider2D c = overlapBuffer[i];
                if (c != null && c.CompareTag(targetTag))
                {
                    if (debugLogs) Debug.Log("Spider: detected player by Tag " + c.name);
                    return c.transform;
                }
            }

            return null;
        }
        else
        {
            Collider2D col = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
            if (col != null)
            {
                if (debugLogs) Debug.Log("Spider: detected player by Layer " + col.name);
                return col.transform;
            }

            return null;
        }
    }

    private void StartChase(Transform playerT)
    {
        isChasing = true;
        targetPlayer = playerT;
        // 当刚开始追击时，不立刻清零速度（让物理自然过渡），但可以记录状态
        if (debugLogs) Debug.Log("Spider: start chasing " + targetPlayer.name);
    }

    // 如果没有检测到玩家，判断之前的 targetPlayer 是否真的离开（带缓冲）
    private void UpdateChaseExit()
    {
        if (!isChasing || targetPlayer == null) return;
        float dist = Vector2.Distance(transform.position, targetPlayer.position);
        if (dist > detectionRadius + chaseExitBuffer)
        {
            if (debugLogs) Debug.Log("Spider: lost player, exiting chase");
            isChasing = false;
            targetPlayer = null;
            // 恢复巡逻初始状态
            patrolTimer = 0f;
            isMoving = false;
            moveTimer = 0f;
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
    }

    private void HandleChase()
    {
        if (targetPlayer == null) return;
        Vector2 dir = (targetPlayer.position - transform.position);
        float signX = Mathf.Sign(dir.x);
        rb.velocity = new Vector2(signX * chaseSpeed, rb.velocity.y);
        patrolDir = signX >= 0 ? 1 : -1;
        transform.rotation = Quaternion.Euler(0f, patrolDir == 1 ? 0f : 180f, 0f);
    }

    private void HandlePatrol()
    {
        patrolTimer += Time.deltaTime;

        if (isMoving)
        {
            moveTimer += Time.deltaTime;
            rb.velocity = new Vector2(patrolDir * patrolSpeed, rb.velocity.y);
            if (moveTimer >= moveDuration)
            {
                isMoving = false;
                moveTimer = 0f;
                rb.velocity = new Vector2(0f, rb.velocity.y);
                patrolTimer = 0f; // 重置周期
                if (debugLogs) Debug.Log("Spider: finished patrol move");
            }
        }
        else
        {
            if (patrolTimer >= patrolInterval)
            {
                // 每次开始移动前可以尝试翻转方向或使用固定方向
                isMoving = true;
                moveTimer = 0f;
                patrolTimer = 0f;
                rb.velocity = new Vector2(patrolDir * patrolSpeed, rb.velocity.y);
                if (debugLogs) Debug.Log("Spider: start patrol move in dir " + patrolDir);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}