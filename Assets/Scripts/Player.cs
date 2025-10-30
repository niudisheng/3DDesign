using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using State = PlayerStateController.State;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")] public float moveSpeed = 5f;
    public float dashSpeed = 10f;
    public float jumpSpeed = 12f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public PlayerStateController playerStateController;

    private Rigidbody2D rb;
    private PlayerControls controls;
    public Vector2 moveInputVector2;
    public bool isJumping;
    public bool isGrounded;
    public float jumpCheckDelay = 0.1f;

    private bool isDashing = false;
    private int faceDir = 1; // 1 向右，-1 向左
    private float dashDuration = 0.5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controls = new PlayerControls();

        // 监听输入
        controls.Player.Move.performed += ctx => moveInputVector2 = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInputVector2 = Vector2.zero;
        controls.Player.Jump.performed += ctx => TryJump();
        controls.Player.Dash.performed += ctx => TryDash();
    }

    void OnEnable() => controls.Player.Enable();
    void OnDisable() => controls.Player.Disable();

    void Update()
    {
        
        
    }

    private void TryJump()
    {
        if (isGrounded)
        {
            Jump();
        }
    }

    private void Jump()
    {
        if (isJumping || !isGrounded) return; // 防止空中再次跳

        rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        playerStateController.ChangeState(State.Jump);
        StartCoroutine(JumpCoroutine());
    }

    private IEnumerator JumpCoroutine()
    {
        isJumping = true;
        isGrounded = false;

        // 持续检测竖直速度，直到开始下落
        while (rb.velocity.y > 0f)
        {
            yield return new WaitForSeconds(jumpCheckDelay);
        }

        // 当上升到顶点速度变为0（或以下）时，切换到下落状态
        playerStateController.ChangeState(State.Down);

        // 等待落地
        while (!isGrounded)
        {
            yield return null;
        }
        
        isJumping = false;
    }


    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    void FixedUpdate()
    {
        if (moveInputVector2.x == 0 && isGrounded)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            PlayerStateController.Instance.ChangeState(State.Idle);
        }
        else if (!isDashing)
        {
            Move(); // 冲刺时禁止 Move()
        }
    }

    private void Move()
    {   
        
        if (isGrounded)
        {
            playerStateController.ChangeState(State.Walk);
        }

        float moveX = moveInputVector2.x * moveSpeed;
        rb.velocity = new Vector2(moveX, rb.velocity.y);

        // 翻转朝向（只有移动时才改变）
        if (moveX > 0.1f)
        {
            faceDir = 1;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (moveX < -0.1f)
        {
            faceDir = -1;
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void TryDash()
    {
        if (!isDashing)
            StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        rb.velocity = new Vector2(faceDir * dashSpeed, 0f); // 冲刺锁定方向，并锁 y

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            Debug.Log("Grounded");
        }
    }
}