using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using State = PlayerStateController.State;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")] public Vector2 moveInputVector2;


    private PlayerControls controls => Player.instance.controls;
    private PlayerInteract playerInteract => Player.instance.playerInteract;
    private Rigidbody2D rb => Player.instance.rb;
    private PlayerStateController playerStateController => Player.instance.playerStateController;
    private PlayerActionControler playerActionControler => Player.instance.playerActionControler;

    #region 运动状态相关变量

    // 新增：击退/眩晕标志，击退期间应该禁止玩家控制
    private bool isStunned = false;
    public float MoveValue; 

    #endregion


    private void Start()
    {
        // 监听输入
        controls.Player.Move.performed += ctx => { TryMove(ctx.ReadValue<Vector2>()); };

        controls.Player.Move.canceled += ctx => CancelMove();
        controls.Player.MoveLeft.performed += ctx => TryMove(false);
        controls.Player.MoveRight.performed += ctx => TryMove(true);
        controls.Player.Jump.performed += ctx => TryJump();
        controls.Player.Dash.performed += ctx => TryDash();
        controls.Player.Interact.performed += ctx => playerInteract.TryInteract();
        controls.Player.Attack.performed += ctx => TryAttack();

        //测试用拔剑
        controls.Player.GetSword.performed += ctx => GameManager.Instance.ChangePlayer();
        SetEndAttack();
    }


    #region Attack

    private void SetEndAttack()
    {
        Player.instance.playerStateController.EndAttack = EndAttack;
    }


    public void TryAttack()
    {
        if (Player.instance.haveSword)
        {
            playerActionControler.AddInput(InputIntent.Attack);
        }
    }

    /// <summary>
    /// 动画事件调用，结束攻击
    /// </summary>
    public void EndAttack()
    {
        playerActionControler.EndAttack();
    }

    #endregion

    #region Jump

    private void TryJump()
    {
        
        playerActionControler.AddInput(InputIntent.Jump);
        return;
    }

    #endregion


    #region Move

    private void TryMove(Vector2 ctx)
    {
        return;
        if (ctx == Vector2.zero)
        {
            moveInputVector2 = new Vector2(-moveInputVector2.x, ctx.y);
        }
        else
        {
            moveInputVector2 = ctx;
        }
        Debug.Log("Move Input: " + moveInputVector2);

        playerActionControler.MoveCheck();
    }
    private void TryMove(bool isRight)
    {
        //当前没有输入，添加新的方向
        if (moveInputVector2 == Vector2.zero)
        {
            if (isRight)
            {
                moveInputVector2 = new Vector2(1, 0);
            }
            else
            {
                moveInputVector2 = new Vector2(-1, 0);
            }
        }
        else
        {
            // 相对于当前输入取反
            if (isRight && moveInputVector2.x < 0)
            {
                moveInputVector2 = new Vector2(-moveInputVector2.x, moveInputVector2.y);
            }
            else if (!isRight && moveInputVector2.x > 0)
            {
                moveInputVector2 = new Vector2(-moveInputVector2.x, moveInputVector2.y);
            }
        }
        
        Debug.Log("Move Input: " + moveInputVector2);

        playerActionControler.MoveCheck();
        playerActionControler.AddInput(InputIntent.Move);
    }
    
    
    
    
    private void CancelMove()
    {
        moveInputVector2 = Vector2.zero;
        playerActionControler.MoveCheck();
    }

    #endregion

    private void TryDash()
    {
        playerActionControler.AddInput(InputIntent.Dash);
    }


    #region 击退代码

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

    #endregion
}