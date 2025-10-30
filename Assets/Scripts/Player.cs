using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using State = PlayerStateController.State;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")] public float moveSpeed = 5f;
    public float dashSpeed = 10f;
    public float jumpForce = 12f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public PlayerStateController playerStateController;

    private Rigidbody2D rb;
    private PlayerControls controls;
    private Vector2 moveInputVector2;
    public bool isJumpPressed;
    public bool isGrounded;

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
        controls.Player.Jump.performed += ctx => isJumpPressed = true;
        controls.Player.Dash.performed += ctx => TryDash();
    }

    void OnEnable() => controls.Player.Enable();
    void OnDisable() => controls.Player.Disable();

    void Update()
    {
        isGrounded = true; // 跳过检测，直接假设在地面上
        // 跳跃输入检测
        if (isJumpPressed && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        isJumpPressed = false; // 重置跳跃输入
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
        if (moveInputVector2.x == 0)
        {
            PlayerStateController.Instance.ChangeState(State.Idle);
        }

        if (!isDashing)
            Move(); // 冲刺时禁止 Move()
    }

    private void Move()
    {
        playerStateController.ChangeState(State.Walk);
        
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
}