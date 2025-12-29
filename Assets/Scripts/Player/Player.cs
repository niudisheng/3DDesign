using System;
using System.Collections;
using Game;
using UnityEngine;
using UnityEngine.InputSystem;
using State = PlayerStateController.State;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Movement Settings")] public float moveSpeed = 5f;
    public float dashSpeed = 10f;
    public float jumpSpeed = 12f;
    public PlayerStateController playerStateController;
    public PlayerInteract playerInteract;

    private Rigidbody2D rb;
    private PlayerControls controls;
    private Vector2 moveInputVector2;
    private bool canJumping = true;
    public bool isGrounded;
    private float jumpCheckDelay = 0.1f;

    private bool isDashing = false;
    private bool isAttacking = false;
    public int faceDir = 1; // 1 向右，-1 向左
    public float dashDuration = 0.3f;

    [Header("Have sword")] public bool haveSword = true;
    
    [Header("摄像机跟随目标")]
    [SerializeField] private GameObject _cameraFollowGo;

    private CameraFollowObject _cameraFollowObject;
    private float _fallSpeedYDampingChangeThreshold;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controls = new PlayerControls();

        // 监听输入
        controls.Player.Move.performed += ctx => moveInputVector2 = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInputVector2 = Vector2.zero;
        controls.Player.Jump.performed += ctx => TryJump();
        controls.Player.Dash.performed += ctx => TryDash();
        controls.Player.Interact.performed += ctx => playerInteract.TryInteract();
        if (haveSword)
        {
            controls.Player.Attack.performed += ctx => TryAttack();
        }
        
        _cameraFollowObject= _cameraFollowGo.GetComponent<CameraFollowObject>();
        
        _fallSpeedYDampingChangeThreshold=CameraManager.instance._fallSpeedYDampingChangeThreshold;
    }

    void OnEnable()
    {
        controls.Player.Enable();
        _cameraFollowObject.SetPlayer(this.transform);
    }

    void OnDisable() => controls.Player.Disable();

    void Update()
    {
        CheckGround();
        // 混用跳跃过程，防止空中再次跳
        if (rb.velocity.y < 0f && !isGrounded)
        {
            StartCoroutine(DownCoroutine());
        }
        
        if (rb.velocity.y < _fallSpeedYDampingChangeThreshold && !CameraManager.instance.IsLerpingYDamping && !CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpYDamping(true);
        }


        if (rb.velocity.y >= 0f && !CameraManager.instance.IsLerpingYDamping && CameraManager.instance.LerpedFromPlayerFalling)
        {
            //reset so it can be called again
            CameraManager.instance.LerpedFromPlayerFalling = false;
            CameraManager.instance.LerpYDamping(false);
        }
        
        // Debug.Log($"Velocity: {rb.velocity.y}, Threshold: {_fallSpeedYDampingChangeThreshold}");
        // Debug.Log($"IsLerping: {CameraManager.instance.IsLerpingYDamping}");
        // Debug.Log($"LerpedFromFalling: {CameraManager.instance.LerpedFromPlayerFalling}");
    }

    #region Jump

    private void TryJump()
    {
        if (canJumping)
        {
            Jump();
        }
    }

    private void Jump()
    {
        isGrounded = false;
        rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        playerStateController.ChangeState(State.Jump);
        StartCoroutine(JumpCoroutine());
    }

    // 本质是一个判断空中的过程，速度快往上
    private IEnumerator JumpCoroutine()
    {
        canJumping = false;


        // 持续检测竖直速度，直到开始下落
        while (rb.velocity.y >= 0f && !isGrounded)
        {
            playerStateController.ChangeState(State.Jump);
            yield return new WaitForSeconds(jumpCheckDelay);
        }

        // 当上升到顶点速度变为0（或以下）时，切换到下落状态
        playerStateController.ChangeState(State.Down);

        // 等待落地
        while (!isGrounded)
        {
            yield return null;
        }

        canJumping = true;
    }

    private IEnumerator DownCoroutine()
    {
        // 当上升到顶点速度变为0（或以下）时，切换到下落状态
        playerStateController.ChangeState(State.Down);

        // 等待落地
        while (!isGrounded)
        {
            yield return null;
        }

        playerStateController.DisableState(State.Down);
    }

    #endregion


    void FixedUpdate()
    {
        if (moveInputVector2.x == 0 && isGrounded && CanMove())
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            PlayerStateController.Instance.ChangeState(State.Idle);
        }
        else if (CanMove())
        {
            Move(); // 冲刺时禁止 Move()
        }
    }

    private bool CanMove()
    {
        return !isDashing && !isAttacking;
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
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            _cameraFollowObject.CallTurn();
        }
        else if (moveX < -0.1f)
        {
            faceDir = -1;
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            _cameraFollowObject.CallTurn();
        }
    }

    #region Dash

    private void TryDash()
    {
        if (CanMove())
            StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        rb.velocity = new Vector2(faceDir * dashSpeed, 0f); // 冲刺锁定方向，并锁 y
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

    #endregion

    #region Attack

    public void TryAttack()
    {
        if (CanMove())
        {
            isAttacking = true;
            playerInteract.Attack(true);
            playerStateController.ChangeState(State.Attack);
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
        playerInteract.Attack(false);
        playerStateController.DisableState(State.Attack);
    }

    #endregion
    
    /// <summary>
    /// 玩家获得剑
    /// </summary>
    /// <param name="hasSword"></param>
    public void SetSword(bool hasSword)
    {
        hasSword = hasSword;
    }

}