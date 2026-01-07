using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using State = PlayerStateController.State;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")] public float moveSpeed = 8f;
    public float dashSpeed = 20f;
    public float jumpSpeed = 16f;
    public Vector2 moveInputVector2;
    public float dashDuration = 0.3f;

    private PlayerControls controls => Player.instance.controls;
    private PlayerInteract playerInteract => Player.instance.playerInteract;
    private Rigidbody2D rb => Player.instance.rb;
    private PlayerStateController playerStateController => Player.instance.playerStateController;

    #region 运动状态相关变量

    public bool canJumping = true;
    public bool isGrounded;
    private bool isDashing = false;

    private bool isAttacking = false;

    // 新增：击退/眩晕标志，击退期间应该禁止玩家控制
    private bool isStunned = false;

    #endregion


    private void Start()
    {
        // 监听输入
        controls.Player.Move.performed += ctx => moveInputVector2 = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInputVector2 = Vector2.zero;
        controls.Player.Jump.performed += ctx => TryJump();
        controls.Player.Dash.performed += ctx => TryDash();
        controls.Player.Interact.performed += ctx => playerInteract.TryInteract();
        controls.Player.Attack.performed += ctx => TryAttack();
        SetEndAttack();
    }


    #region Attack

    private void SetEndAttack()
    {
        Player.instance.playerStateController.EndAttack = EndAttack;
    }


    public void TryAttack()
    {
        if (CanMove() && Player.instance.haveSword)
        {
            isAttacking = true;
            playerInteract.Attack(true);
            playerStateController.ChangeState(State.Attack);
            rb.velocity = new Vector2(0, rb.velocity.y); // 攻击时锁定水平速度
        }
    }

    /// <summary>
    /// 动画事件调用，结束攻击
    /// </summary>
    public void EndAttack()
    {
        isAttacking = false;
        playerInteract.Attack(false);
        playerStateController.DisableState(State.Attack);
    }

    #endregion

    #region Jump

    private void TryJump()
    {
        if (canJumping && CanMove() && isGrounded)
        {
            Jump();
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        playerStateController.ChangeState(State.Jump);
        canJumping = false;
        isGrounded = false;
    }

    #endregion


    #region Move

    void FixedUpdate()
    {
        if (CanMove())
        {
            Move(); // 冲刺时禁止 Move()
        }
    }

    /// <summary>
    /// 是否能自由移动，不在冲刺或攻击状态中
    /// </summary>
    /// <returns></returns>
    private bool CanMove()
    {
        // 将 isStunned 纳入判断，眩晕/击退期间禁止移动
        return !isDashing && !isAttacking && !isStunned;
    }

    private void Move()
    {
        float moveX = moveInputVector2.x * moveSpeed;
        rb.velocity = new Vector2(moveX, rb.velocity.y);

        Player.instance.ChangeDir(moveX);
    }

    #endregion

    #region Dash

    private void TryDash()
    {
        if (CanMove())
            StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        rb.velocity = new Vector2(Player.instance.faceDir * dashSpeed, 0f); // 冲刺锁定方向，并锁 y
        playerStateController.ChangeState(State.Dash);
        yield return new WaitForSeconds(dashDuration);
        playerStateController.DisableState(State.Dash);
        isDashing = false;
    }

    #endregion

    #region Ground Check

    [Header("Ground Settings")] public float groundCheckDistance = 0.1f;
    public Transform groundCheckPoint; // 脚底检测点
    public LayerMask groundLayer;

    // public bool IsGrounded { get; private set; }


    private void CheckGround()
    {
        // 射线向下检测
        RaycastHit2D hit = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckDistance, groundLayer);
        // Debug.Log(hit.collider);

        isGrounded = hit.collider != null;


        // 可视化射线
        Color color = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(groundCheckPoint.position, Vector2.down * groundCheckDistance, color);
    }


    void Update()
    {
        DownCheck();

        if (Mathf.Approximately(rb.velocity.x, 0f) && isGrounded)
        {
            playerStateController.ChangeState(State.Idle);
        }
        else
        {
            if (isGrounded)
            {
                playerStateController.ChangeState(State.Walk);
            }
        }
    }

    private void DownCheck()
    {
        if (rb.velocity.y < 0f && playerStateController.GetCurrentState() != State.Down && !isGrounded)
        {
            StartCoroutine(DownCoroutine());
        }
    }

    private IEnumerator DownCoroutine()
    {
        // 当上升到顶点速度变为0（或以下）时，切换到下落状态
        playerStateController.ChangeState(State.Down);

        // 等待落地
        while (!isGrounded)
        {
            //下落时才进行地面检测
            CheckGround();
            yield return null;
        }

        canJumping = true;

        playerStateController.DisableState(State.Down);
    }

    #endregion

    // 新增：对外接口，应用击退并在短时间内禁用玩家控制
    public void ApplyKnockback(Vector2 dir, float force, float stunDuration = 0.25f)
    {
        // 开始协程执行击退逻辑
        StartCoroutine(KnockbackCoroutine(dir, force, stunDuration));
    }

    private IEnumerator KnockbackCoroutine(Vector2 dir, float force, float stunDuration)
    {
        // 如果已经在击退，则直接刷新时间并返回（或可以选择叠加）
        // 为简单起见，这里不叠加力，只刷新计时
        if (isStunned)
        {
            // 仍然施加一次瞬时力
            rb.velocity = Vector2.zero;
            rb.AddForce(dir.normalized * force, ForceMode2D.Impulse);
            yield break;
        }

        // 标记为眩晕，阻止输入和移动
        isStunned = true;

        // 中断冲刺/攻击状态
        isDashing = false;
        isAttacking = false;

        // 设置受击状态动画
        if (playerStateController != null)
            playerStateController.ChangeState(State.Hurt);

        // 施加物理冲量
        rb.velocity = Vector2.zero;
        rb.AddForce(dir.normalized * force, ForceMode2D.Impulse);

        // 等待眩晕结束
        yield return new WaitForSeconds(stunDuration);

        // 结束受击状态
        isStunned = false;

        // 清除受击动画状态（由状态控制器负责）
        if (playerStateController != null)
            playerStateController.DisableState(State.Hurt);
    }
}