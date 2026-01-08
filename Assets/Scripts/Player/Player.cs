using System;
using System.Collections;
using Game;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    public static Player instance;

    // 运动相关


    [Header("公共变量")] [HideInInspector] public Rigidbody2D rb;
    public PlayerStateController playerStateController;
    public PlayerInteract playerInteract;
    [HideInInspector] public Animator animator;
    [HideInInspector] public PlayerControls controls;
    [HideInInspector] public int faceDir = 1; // 1 向右，-1 向左
    public PlayerController playerController;

    private Transform _spriteTransform;


    [Header("Have sword")] public bool haveSword = true;

    [Header("摄像机跟随目标")] [SerializeField] private GameObject _cameraFollowGo;

    private CameraFollowObject _cameraFollowObject;
    private float _fallSpeedYDampingChangeThreshold;

    private int _dieTriggerHash;
    private int _wakeTriggerHash;

    // 新增：是否锁定其他模块（死亡/重生期间）
    private bool _modulesLocked;
    public bool ModulesLocked => _modulesLocked;

    void Awake()
    {
        // 单例
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        _spriteTransform = playerStateController != null ? playerStateController.transform : this.transform;
        controls = new PlayerControls();


        _cameraFollowObject = _cameraFollowGo.GetComponent<CameraFollowObject>();

        _fallSpeedYDampingChangeThreshold = CameraManager.instance._fallSpeedYDampingChangeThreshold;

        // 缓存 trigger 的哈希，避免字符串查找
        _dieTriggerHash = Animator.StringToHash("Die");
        _wakeTriggerHash = Animator.StringToHash("Wake");
    }

    private void Start()
    {
        playerStateController.ChangeAnimator(haveSword);
    }

    private void Update()
    {
        CameraDownCheck();
    }

    void OnEnable()
    {
        // 只有在没有被模块锁定时才启用输入，否则保持禁用，避免在重生/死亡时收到输入
        if (controls == null) controls = new PlayerControls();
        if (!_modulesLocked)
            controls.Player.Enable();
        _cameraFollowObject.SetPlayer(this.transform);
    }

    void OnDisable() => controls.Player.Disable();

    // 锁定（禁用）其它模块：输入、控制器、交互和物理仿真（可选）
    private void LockModules()
    {
        if (_modulesLocked) return;
        _modulesLocked = true;

        try
        {
            controls?.Player.Disable();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to disable controls: {ex}");
        }

        if (playerController != null)
            playerController.enabled = false;

        if (playerStateController != null)
            playerStateController.enabled = false;

        if (playerInteract != null)
            playerInteract.enabled = false;

        // 停止物理仿真，避免在死亡/重生期间被外力影响（在 Unlock 时恢复）
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }
    }

    // 恢复模块
    private void UnlockModules()
    {
        if (!_modulesLocked) return;
        _modulesLocked = false;

        try
        {
            controls?.Player.Enable();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to enable controls: {ex}");
        }

        if (playerController != null)
            playerController.enabled = true;

        if (playerStateController != null)
            playerStateController.enabled = true;

        if (playerInteract != null)
            playerInteract.enabled = true;

        if (rb != null)
            rb.simulated = true;
    }

    private void CameraDownCheck()
    {
        if (rb.velocity.y < _fallSpeedYDampingChangeThreshold && !CameraManager.instance.IsLerpingYDamping &&
            !CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpYDamping(true);
        }


        if (rb.velocity.y >= 0f && !CameraManager.instance.IsLerpingYDamping &&
            CameraManager.instance.LerpedFromPlayerFalling)
        {
            //reset so it can be called again
            CameraManager.instance.LerpedFromPlayerFalling = false;
            CameraManager.instance.LerpYDamping(false);
        }
    }

    public void ChangeDir(float moveX)
    {
        // 翻转朝向（只有移动时才改变）
        if (moveX > 0.1f)
        {
            faceDir = 1;
            _spriteTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
            _cameraFollowObject.CallTurn();
        }
        else if (moveX < -0.1f)
        {
            faceDir = -1;
            _spriteTransform.rotation = Quaternion.Euler(0f, 180f, 0f);
            _cameraFollowObject.CallTurn();
        }
    }


    #region 剑代码

    /// <summary>
    /// 玩家获得剑
    /// </summary>
    /// <param name="hasSword"></param>
    public void SetSword(bool hasSword)
    {
        haveSword = hasSword;
        playerStateController.ChangeAnimator(hasSword);
    }

    public void GetSword()
    {
        animator.SetTrigger("GetSword");
        LockModules();
        WaitForAnimation("GetSword", () =>
        {
            SetSword(true);
            UnlockModules();
        });
    }

    #endregion

    #region 死亡与重生

    [ContextMenu("Test Die")]
    public void OnDie()
    {
        // 立即锁定其它模块，防止在死亡动画或流程中任何输入/控制/交互发生
        LockModules();

        // 清理状态（立即禁用控制以防止中途操作）

        rb.velocity = Vector2.zero;
        // 播放死亡动画（使用哈希触发器更高效）
        animator.SetTrigger(_dieTriggerHash);

        // 最佳做法：在动画剪辑末端添加一个 Animation Event 调用 `OnDieAnimationEnd`。
        // 如果你没有添加事件，下面的 coroutine 会作为回退路径，在状态结束后执行。
        WaitForAnimation("Die", () =>
        {
            // 如果动画事件没有调用，这里会作为回退执行
            GameManager.Instance.LoadGame();
        });
    }

    public void OnRespawn(SaveData saveData)
    {
        Debug.Log("Player: OnRespawn called");
        // 在重生开始时也锁住模块，直到重生动画完全结束再恢复
        LockModules();

        // 将玩家移动到存档位置（先设置位置以便重生动画以正确位置播放）
        if (rb != null)
        {
            // 如果物理被禁用了，先确保位置直接设置
            rb.simulated = false;
        }

        transform.position = saveData.playerPosition;

        // 清理速度以免残留
        if (rb != null) rb.velocity = Vector2.zero;


        // 播放重生动画
        animator.SetTrigger(_wakeTriggerHash);

        // 推荐在 Wake 动画末尾添加 Animation Event 调用 `OnWakeAnimationEnd`。
        // 同时保留 coroutine 回退机制。
        WaitForAnimation("Wake", UnlockModules);
        /*
        StartCoroutine(WaitForAnimationEnd("Wake", () =>
        {
            // 动画结束后恢复模块
            UnlockModules();
        }));
        */
    }

    public void WaitForAnimation(string stateName, UnityAction onComplete)
    {
        StartCoroutine(GameManager.Instance.WaitForAnimationEnd(stateName, onComplete,animator));
    }


    

    #endregion
}