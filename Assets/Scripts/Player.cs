using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private PlayerControls controls;
    private Vector2 moveInput;
    public bool isJumpPressed;
    public bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controls = new PlayerControls();

        // 监听输入
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        controls.Player.Jump.performed += ctx => isJumpPressed = true;
    }

    void OnEnable() => controls.Player.Enable();
    void OnDisable() => controls.Player.Disable();

    void Update()
    {
        // 检测是否在地面上
        // isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isGrounded = true; // 跳过检测，直接假设在地面上
        // 跳跃输入检测
        if (isJumpPressed && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
        isJumpPressed = false; // 重置跳跃输入
    }

    void FixedUpdate()
    {
        // 移动（只影响x轴速度）
        float moveX = moveInput.x * moveSpeed;
        Debug.Log(moveX);
        rb.velocity = new Vector2(moveX, rb.velocity.y);

        // 可选：面向移动方向翻转角色
        if (moveX > 0.1f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveX < -0.1f)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}