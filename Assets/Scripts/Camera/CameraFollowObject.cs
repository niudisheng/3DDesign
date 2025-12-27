using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowObject : MonoBehaviour
{
    [Header("玩家位置")]
    [SerializeField] private Transform _playerTransform;

    [Header("翻转相机移动速率")]
    [SerializeField] private float flipvRotationTime =0.5f;
    
    private Coroutine _turnCoroutine;
    
<<<<<<< Updated upstream
    private Player _player;
=======
    private PlayerController _player;
>>>>>>> Stashed changes
    
    private int faceDir;

    private void Awake()
    {
<<<<<<< Updated upstream
        _player = _playerTransform.gameObject.GetComponent<Player>();
=======
        _player = _playerTransform.gameObject.GetComponent<PlayerController>();
>>>>>>> Stashed changes
            
        faceDir =_player.faceDir;
    }


    private void Update()
    {
        transform.position = _playerTransform.position;
    }

    public void CallTurn()
    {
        // 如果有正在进行的旋转，先停止它
        if (_turnCoroutine != null)
        {
            StopCoroutine(_turnCoroutine);
        }
        _turnCoroutine = StartCoroutine(FlipVLerp());
    }


    private IEnumerator FlipVLerp()
    {
        float startRotation = transform.localEulerAngles.y;
        float endRotationAmount = DetermineEndRotation();
        float yRotation = 0f;
        
        float elapsedTime = 0f;
        while (elapsedTime < flipvRotationTime)
        {
            elapsedTime += Time.deltaTime;
            
            yRotation = Mathf.Lerp(startRotation, endRotationAmount, elapsedTime / flipvRotationTime);
            transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
            
            yield return null;
            
        }
        
    }



    private float DetermineEndRotation()
    {
        // 直接从PlayerController获取当前朝向，而不是依赖本地faceDir
        int currentFaceDir = _player.faceDir;
        if (currentFaceDir == 1)
        {
            return 0f;
        }
        else
        {
            return 180f;
        }
    }
    

}