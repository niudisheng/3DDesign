using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class transTheSteam : MonoBehaviour
{
    
    [Header("目标Transform")]
    public Transform targetTransform; // 要监测的2D Transform

    [Header("方向参数设置")]
    [SerializeField] private float directionParameter = 0f; // 最终的方向参数
    public Material waterMa;
    [SerializeField] private float smoothSpeed = 5f; // 平滑过渡速度
    [SerializeField] private float deadZone = 0.01f; // 死区阈值，避免微小抖动
        [SerializeField] private float returnSpeed = 2f;      // 回归速度
        [SerializeField] private float minParameter = -10f; // 平滑过渡速度
    [SerializeField] private float maxParameter = 10f; // 死区阈值，避免微小抖动

        [Header("参数范围")]
    public float minValue = 0.8f;    // 最小值
    public float maxValue = 1.2f;    // 最大值
    public float restValue = 1.0f;   // 静止时的值





    [Header("调试信息")]
    [SerializeField] private float currentDirection = 0f; // 当前方向（-1左，0静止，1右）
    [SerializeField] private bool isMovingRight = false; // 是否向右移动
    [SerializeField] private bool isMovingLeft = false;  // 是否向左移动

    // 私有变量
    private float previousX; // 上一帧的X位置
    private bool isFirstFrame = true; // 是否为第一帧（避免初始跳动）

    private bool isMoving;

    void Start()
    {
        // 初始化
        if (targetTransform == null)
        {
            targetTransform = transform; // 如果没有指定，使用自身的Transform
            Debug.LogWarning("未指定targetTransform，将使用脚本所在物体的Transform");
        }
        
        previousX = targetTransform.position.x;
        directionParameter = Mathf.Clamp(directionParameter, minParameter, maxParameter);
    }

    void Update()
    {
        if (targetTransform == null)
        {
            Debug.LogError("targetTransform未设置！");
            return;
        }

        // 获取当前帧的X位置
        float currentX = targetTransform.position.x;
        
        // 跳过第一帧，避免初始跳动
        if (isFirstFrame)
        {
            previousX = currentX;
            isFirstFrame = false;
            return;
        }

        // 计算X轴位移
        float deltaX = currentX - previousX;
        
        // 判断运动方向（考虑死区避免微小抖动）
        if (Mathf.Abs(deltaX) > deadZone)
        {
            if (deltaX > 0)
            {
                currentDirection = 1f; // 向右移动
                isMovingRight = true;
                isMovingLeft = false;
            }
            else
            {
                currentDirection = -1f; // 向左移动
                isMovingRight = false;
                isMovingLeft = true;
            }
        }
        else
        {
            currentDirection = 0f; // 静止
            isMovingRight = false;
            isMovingLeft = false;
        }

        // 根据方向平滑调整参数
        UpdateDirectionParameter(currentDirection);

        // 保存当前帧位置供下一帧使用
        previousX = currentX;
    }

    void UpdateDirectionParameter(float direction)
    {
        if (waterMa == null || !waterMa.HasProperty("_pianli"))
            return;
        
        // 获取当前值
        float currentValue = waterMa.GetFloat("_pianli");
        
        // 计算目标值
        float targetValue = restValue; // 默认目标值是静止值
        
        if (direction > 0.5f) // 明确向右移动
        {
            targetValue = maxValue; // 目标值设为最大值
            isMoving = true;
        }
        else if (direction < -0.5f) // 明确向左移动
        {
            targetValue = minValue; // 目标值设为最小值
            isMoving = true;
        }
        else if (Mathf.Abs(direction) > 0.1f) // 有轻微方向
        {
            // 根据方向向极值调整
            targetValue = Mathf.Lerp(restValue, direction > 0 ? maxValue : minValue, Mathf.Abs(direction));
            isMoving = true;
        }
        else // 静止或方向不明确
        {
            targetValue = restValue; // 回归到静止值
            isMoving = false;
        }
        
        // 根据是否移动选择不同的平滑速度
        float speed = isMoving ? smoothSpeed : returnSpeed;
        
        // 使用平滑插值
        float newValue = Mathf.Lerp(currentValue, targetValue, speed * Time.deltaTime);
        
        // 应用新值
        waterMa.SetFloat("_pianli", newValue);
       

    }

}
