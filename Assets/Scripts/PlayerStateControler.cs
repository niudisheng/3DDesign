using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerStateControler : MonoBehaviour
{
    enum State
    {
        Idle,
        Walk,
        Dash,
        Attack,
        Down,
        Jump,
    }

    private State currentState;
    private Rigidbody2D rb;

    // 玩家状态的相关参数
    public float walkSpeed = 2f;
    public float dashSpeed = 5f;
    public float jumpSpeed = 10f;
    public float attackTime = 1f;
    private float attackCooldown = 0;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentState = State.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        // 如果攻击冷却时间大于0，则减少冷却时间
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }

        // 根据输入改变状态
        HandleState();
    }

    void HandleState()
    {
        switch (currentState)
        {
            case State.Idle:
                Idle();
                break;
            case State.Walk:
                Walk();
                break;
            case State.Dash:
                Dash();
                break;
            case State.Attack:
                Attack();
                break;
            case State.Down:
                Down();
                break;
            case State.Jump:
                Jump();
                break;
        }
    }

    // 闲置状态
    void Idle()
    {
        // 如果按下了移动键，就进入行走状态
        if (Input.GetAxisRaw("Horizontal") != 0)
        {
            currentState = State.Walk;
        }

        // 如果按下跳跃键，就进入跳跃状态
        if (Input.GetButtonDown("Jump"))
        {
            currentState = State.Jump;
        }
    }

    // 行走状态
    void Walk()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * walkSpeed, rb.velocity.y);

        // 如果松开方向键，就回到闲置状态
        if (moveInput == 0)
        {
            currentState = State.Idle;
        }

        // 如果按下跳跃键，就进入跳跃状态
        if (Input.GetButtonDown("Jump"))
        {
            currentState = State.Jump;
        }

        // 如果按下冲刺键，就进入冲刺状态
        if (Input.GetButtonDown("Dash"))
        {
            currentState = State.Dash;
        }
    }

    // 冲刺状态
    void Dash()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * dashSpeed, rb.velocity.y);

        // 冲刺结束后回到闲置状态
        if (Input.GetButtonDown("Dash") || moveInput == 0)
        {
            currentState = State.Idle;
        }
    }

    // 攻击状态
    void Attack()
    {
        if (attackCooldown <= 0)
        {
            // 执行攻击行为
            Debug.Log("Attacking...");
            // 设置攻击冷却
            attackCooldown = attackTime;
        }

        // 完成攻击后回到闲置状态
        currentState = State.Idle;
    }

    // 跌倒状态
    void Down()
    {
        // 这里可以添加跌倒时的行为
        // 比如设置速度为0或播放跌倒动画
    }

    // 跳跃状态
    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        currentState = State.Idle;
    }
}
