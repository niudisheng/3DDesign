using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using State = PlayerStateController.State;

public class PlayerActionControler : MonoBehaviour
{
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

    // 是否用过冲刺
    private bool isDashing = false;

    // 受伤击退/眩晕标志
    private bool isStunned = false;

    private bool isAttacking = false;

    private void OnEnable()
    {
        inputBuffer = new InputBuffer();
    }

    private void OnDisable()
    {
        inputBuffer = null;
    }

    private void GroundJumpCheck()
    {
        if (isGrounded && CanMove())

        {
            if (inputBuffer.TryConsume(InputIntent.Jump, 0.2f))
            {
                Jump();
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
            if (isGrounded && moveInputVector2.x == 0f)
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
    }

    private void FixedUpdate()
    {
        MoveCheck();
        GroundJumpCheck();
        DashCheck();
    }


    #region Jump And Down

    [Header("Ground Settings")] public float groundCheckDistance = 0.1f;
    public Transform groundCheckPoint; // 脚底检测点
    public LayerMask groundLayer;


    private void CheckGround()
    {
        // 射线向下检测
        RaycastHit2D hit = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckDistance, groundLayer);


        isGrounded = hit.collider != null;


        // 可视化射线
        Color color = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(groundCheckPoint.position, Vector2.down * groundCheckDistance, color);
    }


    private void JumpDownCheck()
    {
        if (rb.velocity.y > 0f && playerStateController.GetCurrentState() != State.Down && !isGrounded)
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
            CheckGround();
            yield return null;
        }
    }

    #endregion

    #region Dash

    private void DashCheck()
    {
        if (inputBuffer.TryConsume(InputIntent.Dash, 0.1f) && CanMove())
        {
            Dash();
        }
    }

    #endregion

    public void MoveCheck()
    {
        if (CanMove())
        {
            Move();
        }
    }


    #region All Actions

    private void Move()
    {
        float moveX = moveInputVector2.x * moveSpeed;
        // Debug.Log("MoveX: " + moveX);
        rb.velocity = new Vector2(moveX, rb.velocity.y);

        Player.instance.ChangeDir(moveX);
    }

    private bool CanMove()
    {
        // 将 isStunned 纳入判断，眩晕/击退期间禁止移动
        return !isDashing && !isAttacking && !isStunned;
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        isGrounded = false;
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

        bool result = OnDashCheck(dashCanMoveTime,dashFair);
        if (result)
        {
            isDashing = false;
            yield break;
        }

        yield return new WaitForSeconds(dashDuration - dashCanMoveTime);
        isDashing = false;

        playerStateController.DisableState(State.Dash);
    }

    private bool OnDashCheck(float dashCanMoveTime,int dashFair)
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