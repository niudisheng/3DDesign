using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using State = PlayerStateController.State;

public class PlayerActionControler : MonoBehaviour
{
    private InputBuffer inputBuffer;
    private Rigidbody2D rb => Player.instance.rb;
    private PlayerController playerController => Player.instance.playerController;
    private PlayerStateController playerStateController => Player.instance.playerStateController;
    public float jumpSpeed = 16f;
    public bool isGrounded;

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
        if (isGrounded && inputBuffer.TryConsume(InputIntent.Jump, 0.2f))
        {
            Jump();
        }
    }

    public void AddInput(InputIntent intent)
    {
        inputBuffer.Record(intent);
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        isGrounded = false;
    }

    void Update()
    {
        DownCheck();


        if (isGrounded && playerController.moveInputVector2.x == 0f)
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

    private void FixedUpdate()
    {
        GroundJumpCheck();
    }


    #region Ground Check

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


    private void DownCheck()
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
}