using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using State = PlayerStateController.State;

public class PlayerActionControler : MonoBehaviour
{
    #region 一坨变量

    private InputBuffer inputBuffer;
    private Rigidbody2D rb => Player.instance.rb;
    private PlayerStateController playerStateController => Player.instance.playerStateController;
    private PlayerInteract playerInteract => Player.instance.playerInteract;
    public float dashSpeed = 20f;
    public float jumpSpeed = 16f;
    public float dashDuration = 0.25f;
    public float dashCanMoveTime = 0.15f;
    public Vector2 moveInputVector2 => Player.instance.playerController.moveInputVector2;
    public bool isGrounded;
    public float moveSpeed = 8f;

    public AnimationCurve dashCurve;

    // 是否用过冲刺
    private bool isDashing = false;

    // 受伤击退/眩晕标志
    private bool isStunned = false;

    private bool isAttacking = false;

    #endregion

    private void OnEnable()
    {
        inputBuffer = new InputBuffer();
    }

    private void OnDisable()
    {
        inputBuffer = null;
    }

    public void JumpCheck()
    {
        if (CanMove())
        {
            ApplyJumpPhysics();
            if (isGrounded)
            {
                if (inputBuffer.TryConsume(InputIntent.Jump, 0.2f))
                {
                    Jump();
                }
            }
        }
    }

    public void AddInput(InputIntent intent)
    {
        inputBuffer.Record(intent);
    }


    void Update()
    {
        if (CanMove())
        {
            JumpDownCheck();
            if (!isGrounded) return;
            if (Mathf.Approximately(moveInputVector2.x, 0f))
            {
                playerStateController.ChangeState(State.Idle);
            }

            else
            {
                playerStateController.ChangeState(State.Walk);
            }
        }
    }

    /// <summary>
    /// 存放物理逻辑
    /// </summary>
    private void FixedUpdate()
    {
        MoveCheck();
        JumpCheck();
        DashCheck();
    }


    #region Jump And Down

    [Header("Ground Settings")] public float groundCheckDistance = 0.1f;
    public Transform groundCheckPoint; // 脚底检测点
    public LayerMask groundLayer;


    private void JumpDownCheck()
    {
        Debug.Log("Velocity X: " + rb.velocity.x);
        Debug.Log("Velocity Y: " + rb.velocity.y);
        CheckGround();
        if (rb.velocity.y > 0f && playerStateController.GetCurrentState() != State.Jump && !isGrounded)
        {
            playerStateController.ChangeState(State.Jump);
        }

        if (rb.velocity.y < 0f && playerStateController.GetCurrentState() != State.Down && !isGrounded)
        {
            playerStateController.ChangeState(State.Down);
            StartCoroutine(DownCoroutine());
        }
    }

    private IEnumerator DownCoroutine()
    {
        // 等待落地
        while (!isGrounded)
        {
            //下落时才进行地面检测

            yield return null;
        }

        Debug.Log("Landed");
    }

    private void CheckGround()
    {
        // 射线向下检测
        RaycastHit2D hit = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckDistance, groundLayer);


        isGrounded = hit.collider != null;


        // 可视化射线
        Color color = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(groundCheckPoint.position, Vector2.down * groundCheckDistance, color);
    }

    #endregion


    private void DashCheck()
    {
        if (inputBuffer.TryConsume(InputIntent.Dash, 0.1f) && CanMove())
        {
            Dash();
        }
    }


    public void MoveCheck()
    {
        if (CanMove())
        {
            Move();
        }
    }

    private void ApplyHorizontalMove(AnimationCurve accelCurve, float inputX)
    {
        float groundAccel = 25f;
        float groundDecel = 45f;
        float groundMaxSpeed = moveSpeed;

        double airAccel = groundAccel * 0.6;
        double airDecel = groundDecel * 0.8;
        double airMaxSpeed = groundMaxSpeed * 0.95;


        double maxSpeed = isGrounded ? groundMaxSpeed : airMaxSpeed;


        float targetSpeed = inputX * groundMaxSpeed;

        // 计算速度差
        float speedDiff = targetSpeed - rb.velocity.x;
        // 如果速度差很小，直接设置速度,保持最高速度可达和保证静态不动


        // 归一化当前速度
        float normalizedSpeed = (float)(Mathf.Abs(rb.velocity.x) / maxSpeed);
        if (Mathf.Abs(normalizedSpeed) < 0.1f)
        {
            rb.velocity = new Vector2(targetSpeed, rb.velocity.y);
            return;
        }

        // Debug.Log("normalizedSpeed: " + normalizedSpeed);
        // 曲线倍率
        float accelMultiplier = accelCurve.Evaluate(normalizedSpeed);

        // 基础加速度值，可以根据需要调整


        double accel = isGrounded ? groundAccel : airAccel;
        double decel = isGrounded ? groundDecel : airDecel;
        // 玩家是否有输入
        bool hasInput = Mathf.Abs(inputX) > 0.01f;
        double rate = hasInput ? accel : decel;


        float movement = (float)(speedDiff *
                                 accelMultiplier *
                                 rate *
                                 Time.fixedDeltaTime);

        rb.velocity += new Vector2(movement, 0f);
    }

    #region JumpLogic

    [Header("Jump Base")] public float jumpForce = 14f; // 起跳初速度（决定“跳起来的爽感”）
    public float maxJumpHoldTime = 0.2f; // 最长按住跳跃的时间（长按跳的高度上限）

    [Header("Gravity")] public float gravityScale = 1f; // 基础重力
    public float fallGravityMultiplier = 2.5f; // 下落重力（短按跳“啪”下来的关键）
    public float jumpCutMultiplier = 0.5f; // 松开跳跃时，立刻削减上升速度

    [Header("Apex")] public float apexGravityMultiplier = 0.5f; // 跳跃顶点时的“悬停感”
    public float apexThreshold = 0.8f; // 速度接近 0 时，认为到顶点

    private bool isJumping; // 是否处于跳跃流程中
    private bool isHoldingJump; // 是否仍然按着跳跃键
    private float jumpHoldTimer; // 已按住跳跃多久


    private void Jump()
    {
        OnJumpStarted();
    }

    public void OnJumpStarted()
    {
        isGrounded = false;
        // 起跳：一次性给向上的速度
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);

        isJumping = true;
        isHoldingJump = true;
        jumpHoldTimer = 0f;
    }

    public void OnJumpCanceled()
    {
        isHoldingJump = false;

        // 如果还在上升，立刻削减速度（短按跳的关键）
        if (rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(
                rb.velocity.x,
                rb.velocity.y * jumpCutMultiplier
            );
        }
    }


    private void ApplyJumpPhysics()
    {
        // ===== 1️⃣ 长按跳：持续上升（有限时间） =====
        if (isJumping && isHoldingJump)
        {
            jumpHoldTimer += Time.fixedDeltaTime;

            if (jumpHoldTimer < maxJumpHoldTime)
            {
                // 在可变跳跃窗口内，减少重力 → 跳得更高
                rb.gravityScale = gravityScale;
            }
            else
            {
                isHoldingJump = false;
            }
        }

        // ===== 2️⃣ 下落：重力加大（爽） =====
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = gravityScale * fallGravityMultiplier;
        }

        // ===== 3️⃣ 顶点悬停（速度接近 0） =====
        else if (Mathf.Abs(rb.velocity.y) < apexThreshold)
        {
            rb.gravityScale = gravityScale * apexGravityMultiplier;
        }

        // ===== 4️⃣ 正常上升 =====
        else
        {
            rb.gravityScale = gravityScale;
        }
    }

    #endregion


    #region All Actions

    private void Move()
    {
        float moveX = moveInputVector2.x * moveSpeed;


        ApplyHorizontalMove(dashCurve, moveInputVector2.x);

        Player.instance.ChangeDir(moveX);
    }

    private bool CanMove()
    {
        // 将 isStunned 纳入判断，眩晕/击退期间禁止移动
        return !isDashing && !isAttacking && !isStunned;
    }


    private void Dash()
    {
        isDashing = true;
        playerStateController.ChangeState(State.Dash);
        StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine()
    {
        int dashFair = Player.instance.faceDir;
        rb.velocity = new Vector2(Player.instance.faceDir * dashSpeed, 0f); // 冲刺锁定方向，并锁 y

        yield return new WaitForSeconds(dashCanMoveTime);

        bool result = OnDashCheck(dashCanMoveTime, dashFair);
        if (result)
        {
            isDashing = false;
            yield break;
        }

        yield return new WaitForSeconds(dashDuration - dashCanMoveTime);
        isDashing = false;

        playerStateController.DisableState(State.Dash);
    }

    private bool OnDashCheck(float dashCanMoveTime, int dashFair)
    {
        if (inputBuffer.TryConsume(InputIntent.Jump, dashCanMoveTime))
        {
            Jump();
            return true;
        }

        else if (inputBuffer.TryConsume(InputIntent.Move, dashCanMoveTime))
        {
            // 冲刺时的方向与移动方向不同，取消冲刺状态
            if (dashFair != moveInputVector2.x)
            {
                Move();
                return true;
            }
        }


        return false;
    }


    private void Attack()
    {
        isAttacking = true;
        playerInteract.Attack(true);
        playerStateController.ChangeState(State.Attack);
        rb.velocity = new Vector2(0, rb.velocity.y); // 攻击时锁定水平速度}
    }

    public void EndAttack()
    {
        isAttacking = false;
        playerInteract.Attack(false);
        playerStateController.DisableState(State.Attack);
    }

    #endregion
}