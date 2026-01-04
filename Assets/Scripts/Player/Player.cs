using System;
using System.Collections;
using Game;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    public static Player instance;

    // 运动相关


    [Header("公共变量")] 
    [HideInInspector] public Rigidbody2D rb;
    public PlayerStateController playerStateController;
    public PlayerInteract playerInteract;
    [HideInInspector] public Animator animator;
    [HideInInspector] public PlayerControls controls;
    [HideInInspector] public int faceDir = 1; // 1 向右，-1 向左
    public PlayerController playerController;
    private Transform SpriteTransform;
    


    [Header("Have sword")] public bool haveSword = true;

    [Header("摄像机跟随目标")] [SerializeField] private GameObject _cameraFollowGo;

    private CameraFollowObject _cameraFollowObject;
    private float _fallSpeedYDampingChangeThreshold;

    private int _dieTriggerHash;
    private int _wakeTriggerHash;

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
        SpriteTransform = playerStateController.transform;
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
        controls.Player.Enable();
        _cameraFollowObject.SetPlayer(this.transform);
    }

    void OnDisable() => controls.Player.Disable();


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
            SpriteTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
            _cameraFollowObject.CallTurn();
        }
        else if (moveX < -0.1f)
        {
            faceDir = -1;
            SpriteTransform.rotation = Quaternion.Euler(0f, 180f, 0f);
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
        SetSword(true);
    }

    #endregion

    #region 死亡与重生
    [ContextMenu("Test Die")]
    public void OnDie()
    {
        // 清理状态（立即禁用控制以防止中途操作）
        playerStateController.enabled = false;
        playerController.enabled = false;
        playerInteract.enabled = false;
        rb.velocity = Vector2.zero;
        // 播放死亡动画（使用哈希触发器更高效）
        animator.SetTrigger(_dieTriggerHash);

        // 最佳做法：在动画剪辑末端添加一个 Animation Event 调用 `OnDieAnimationEnd`。
        // 如果你没有添加事件，下面的 coroutine 会作为回退路径，在状态结束后执行。
        StartCoroutine(WaitForAnimationEnd("Die", () =>
        {
            // 如果动画事件没有调用，这里会作为回退执行
            GameManager.Instance.LoadGame();
        }));
    }
    
    public void OnRespawn(SaveData saveData)
    {
        // 将玩家移动到存档位置（先设置位置以便重生动画以正确位置播放）
        transform.position = saveData.playerPosition;

        // 保持控制器禁用，直到重生动画播完
        playerStateController.enabled = false;
        playerController.enabled = false;
        playerInteract.enabled = false;

        // 播放重生动画
        animator.SetTrigger(_wakeTriggerHash);

        // 推荐在 Wake 动画末尾添加 Animation Event 调用 `OnWakeAnimationEnd`。
        // 同时保留 coroutine 回退机制。
        StartCoroutine(WaitForAnimationEnd("Wake", () =>
        {
            playerStateController.enabled = true;
            playerController.enabled = true;
            playerInteract.enabled = true;
        }));
    }

    // 等待指定动画状态播放完毕（基于 state name）
    private IEnumerator WaitForAnimationEnd(string stateName, Action onComplete)
    {
        // 等待动画状态进入
        float enterTimeout = 2f;
        float timer = 0f;
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName) && timer < enterTimeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // 如果没进入指定状态，则直接触发回调以避免无限等待
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            onComplete?.Invoke();
            yield break;
        }

        // 等待动画播放完毕（normalizedTime >= 1 表示播放结束，但如果设置了 Loop 则不会为 >=1）
        float playTimeout = 10f; // 额外保险超时
        timer = 0f;
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f && timer < playTimeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        onComplete?.Invoke();
    }
    
    
    // 可被 Animation Event 调用：当 Die 动画在最后一帧设置 event 调用此方法
    public void OnDieAnimationEnd()
    {
        GameManager.Instance.LoadGame();
    }

    // 可被 Animation Event 调用：当 Wake 动画在最后一帧设置 event 调用此方法
    public void OnWakeAnimationEnd()
    {
        playerStateController.enabled = true;
        playerController.enabled = true;
        playerInteract.enabled = true;
    }

    #endregion
}