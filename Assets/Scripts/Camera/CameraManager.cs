using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    [SerializeField] private CinemachineVirtualCamera[] _allVirtualCameras;

    [Header("下落时Y轴阻尼")]
    [SerializeField] private float _fallPanAmount = 0.25f;
    [SerializeField] private float _fallYPanTime = 0.35f;
    public float _fallSpeedYDampingChangeThreshold = -15f;

    public bool IsLerpingYDamping { get; private set; }
    public bool LerpedFromPlayerFalling { get; set; }

    private Coroutine _lerpYPanCoroutine;
    private Coroutine _PanCameraCoroutine;
    private Coroutine _bwCoroutine;
    
    private CinemachineVirtualCamera _currentCamera;
    private CinemachineFramingTransposer _framingTransposer;

    // Post-processing volume and vignette (optional)
    private Volume _postProcessVolume;
    private Vignette _vignetteSetting;

    private float _normYPanAmount;

    private Vector2 _startingTrackedObjectOffset;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        for (int i = 0; i < _allVirtualCameras.Length; i++)
        {
            if (_allVirtualCameras[i].enabled)
            {
                //set the current active camera
                _currentCamera = _allVirtualCameras[i];
                //set the framing transposer
                _framingTransposer = _currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
                
            }
        }
        
        _normYPanAmount = _framingTransposer.m_YDamping;
        
       _startingTrackedObjectOffset = _framingTransposer.m_TrackedObjectOffset;

        // Try to find a post-process Volume in the scene and cache the Vignette setting if present
        _postProcessVolume = FindObjectOfType<Volume>();
        if (_postProcessVolume != null && _postProcessVolume.profile != null)
        {
            _postProcessVolume.profile.TryGet<Vignette>(out _vignetteSetting);
        }
        
        
    }
    
    
    #region Y轴阻尼
    public void LerpYDamping(bool isPlayerFalling)
    {
        
        if (_lerpYPanCoroutine != null)
        {
            StopCoroutine(_lerpYPanCoroutine);
        }
        _lerpYPanCoroutine = StartCoroutine(LerpYAction(isPlayerFalling));
    }

    private IEnumerator LerpYAction(bool isPlayerFalling)
    {
        IsLerpingYDamping = true;
        

        //grab the starting damping amount
        float startDampAmount = _framingTransposer.m_YDamping;
        float endDampAmount = 0f;

        //determine the end damping amount
        if (isPlayerFalling)
        {
            endDampAmount = _fallPanAmount;
            LerpedFromPlayerFalling = true;
        }
        else
        {
            endDampAmount = _normYPanAmount;
        }

        //lerp the pan amount
        float elapsedTime = 0f;
        while (elapsedTime < _fallYPanTime)
        {
            elapsedTime += Time.deltaTime;
            float lerpedPanAmount = Mathf.Lerp(startDampAmount, endDampAmount, elapsedTime / _fallYPanTime);
            _framingTransposer.m_YDamping = lerpedPanAmount;
            yield return null;
            
        }

        IsLerpingYDamping = false;
        
    }
    
    
    #endregion
    
    #region 相机偏移
  

    public void PanCameraOnContact(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPos)
    {
        _PanCameraCoroutine = StartCoroutine(PanCamera(panDistance, panTime, panDirection, panToStartingPos));
    }

    private IEnumerator PanCamera(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPos)
    {
        Vector2 endPos = Vector2.zero;
        Vector2 startingPos = Vector2.zero;

        //set the direction and distance if we are panning in the direction indicated by the trigger object
        if (!panToStartingPos)
        {
            //set the direction
            switch (panDirection)
            {
                case PanDirection.Up:
                    endPos = Vector2.up;
                    break;
                case PanDirection.Down:
                    endPos = Vector2.down;
                    break;
                case PanDirection.Left:
                    endPos = Vector2.right;
                    break;
                case PanDirection.Right:
                    endPos = Vector2.left;
                    break;
                default:
                    break;
            }

            endPos *= panDistance;
            startingPos = _startingTrackedObjectOffset;
            endPos += startingPos;
        }
        //handle the direction settings when moving back to the starting position
        else
        {
            startingPos = _framingTransposer.m_TrackedObjectOffset;
            endPos = _startingTrackedObjectOffset;
        }

        //handle the actual panning of the camera
        float elapsedTime = 0f;
        while (elapsedTime < panTime)
        {
            elapsedTime += Time.deltaTime;
            Vector3 panLerp = Vector3.Lerp(startingPos, endPos, (elapsedTime / panTime));
            _framingTransposer.m_TrackedObjectOffset = panLerp;
            yield return null;
        }
    }
    #endregion
    
    #region 切换相机
   

    public void SwapCamera(CinemachineVirtualCamera cameraFromLeft, CinemachineVirtualCamera cameraFromRight, Vector2 triggerExitDirection)
    {
        //if the current camera is the camera on the left and our trigger exit direction was on the right
        if (_currentCamera == cameraFromLeft && triggerExitDirection.x > 0f)
        {
            //activate the new camera
            cameraFromRight.enabled = true;
            //deactivate the old camera
            cameraFromLeft.enabled = false;
            //set the new camera as the current camera
            _currentCamera = cameraFromRight;
            //update our composer variable
            _framingTransposer = _currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
        //if the current camera is the camera on the right and our trigger hit direction was on the left
        else if (_currentCamera == cameraFromRight && triggerExitDirection.x < 0f)
        {
            //activate the new camera
            cameraFromLeft.enabled = true;
            //deactivate the old camera
            cameraFromRight.enabled = false;
            //set the new camera as the current camera
            _currentCamera = cameraFromLeft;
            //update our composer variable
            _framingTransposer = _currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
    }
    #endregion
    
    [ContextMenu("Test Change BlackWhite Effect")]
    public void Change()
    {
        ChangeBlackWhiteEffect(1f, 1f);
    }


    /// <summary>
    /// 转场黑幕效果变化
    /// </summary>
    /// <param name="targetValue"></param>
    /// <param name="changeTime"></param>
    public void ChangeBlackWhiteEffect(float targetValue, float changeTime)
    {
        if (_bwCoroutine != null)
        {
            StopCoroutine(_bwCoroutine);
            _bwCoroutine = null;
        }
        _bwCoroutine = StartCoroutine(ChangeBWEffectCoroutine(targetValue, changeTime));
    }

    /// <summary>
    /// Indicates whether a BW change coroutine is currently running.
    /// </summary>
    public bool IsChangingBWEffect => _bwCoroutine != null;

    /// <summary>
    /// Starts a BW change and yields until it completes. Useful for other scripts to wait on the effect.
    /// </summary>
    public IEnumerator ChangeBlackWhiteEffectAsync(float targetValue, float changeTime)
    {
        // Start the existing effect which sets _bwCoroutine.
        ChangeBlackWhiteEffect(targetValue, changeTime);

        // Wait until the internal coroutine completes.
        while (_bwCoroutine != null)
        {
            yield return null;
        }
    }

    private IEnumerator ChangeBWEffectCoroutine(float targetValue, float changeTime)
    {
        // We rely on a Volume with a Vignette setting (URP/Volumes). If we don't have one, try to find it now.
        if (_vignetteSetting == null)
        {
            if (_postProcessVolume == null)
            {
                _postProcessVolume = FindObjectOfType<Volume>();
            }

            if (_postProcessVolume == null || _postProcessVolume.profile == null ||
                !_postProcessVolume.profile.TryGet<Vignette>(out _vignetteSetting))
            {
                // No vignette to animate; exit gracefully
                _bwCoroutine = null;
                yield break;
            }
        }

        // Read start value from the vignette setting
        float startValue = _vignetteSetting.intensity.value;

        // Clamp changeTime to avoid division by zero
        if (changeTime <= 0f)
        {
            _vignetteSetting.intensity.value = targetValue;
            _bwCoroutine = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < changeTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / changeTime);
            _vignetteSetting.intensity.value = Mathf.Lerp(startValue, targetValue, t);
            yield return null;
        }

        _vignetteSetting.intensity.value = targetValue;
        _bwCoroutine = null;
    }
}
