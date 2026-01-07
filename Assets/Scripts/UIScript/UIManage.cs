using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManage : MonoBehaviour
{
    private PlayerControls controls;
    private enum UIState { Start, Settings, OriginalSettings, KeysSet, OriginalKeys, Normal, Bag }
    [Header("角色")] 
    [SerializeField] private GameObject Player;

    [Header("场景")]
    [SerializeField] private GameObject BackGround;

    [Header("页面")]
    [SerializeField] private GameObject StartCanvas;
    [SerializeField] private GameObject SetCanvas;
    [SerializeField] private GameObject OriginalSetCanvas;
    [SerializeField] private GameObject NormalCanvas;
    [SerializeField] private GameObject KeysCanvas;
    [SerializeField] private GameObject OriginalKeysCanvas;
    private UIState currentState;

    void Awake()
    {
        controls = new PlayerControls();
        controls.Player.UISettings.performed += ctx => OnSettingsButton();
        ChangeState(UIState.Start); 
    }
    void OnEnable()
    {
        controls.Player.Enable();  
    }

    void OnDisable()
    {
        controls.Player.Disable();  
    }

    private void ChangeState(UIState newState)
    {
        if (currentState == UIState.Settings || currentState == UIState.KeysSet)
        {
            // 退出Settings和KeysSet状态时恢复时间
            Time.timeScale = 1f;
            Player.SetActive(false);    
            BackGround.SetActive(false); 
        }
        // 隐藏所有Canvas
        StartCanvas.SetActive(false);
        SetCanvas.SetActive(false);
        NormalCanvas.SetActive(false);
        KeysCanvas.SetActive(false);
        OriginalSetCanvas.SetActive(false);
        OriginalKeysCanvas.SetActive(false);


        switch (newState)
        {
            case UIState.Start: 
                StartCanvas.SetActive(true);
                Player.SetActive(false);
                BackGround.SetActive(false);
                break;
            case UIState.Settings: 
                SetCanvas.SetActive(true);
                Player.SetActive(true);
                BackGround.SetActive(true);
                Time.timeScale = 0f; //暂停时间
                break;
            case UIState.KeysSet:
                KeysCanvas.SetActive(true);
                Player.SetActive(true);
                BackGround.SetActive(true);
                Time.timeScale = 0f; 
                break;
            case UIState.OriginalSettings:  // 开始界面的Settings
                OriginalSetCanvas.SetActive(true);
                Player.SetActive(false);
                BackGround.SetActive(false);
                break;
            case UIState.OriginalKeys:  
                OriginalKeysCanvas.SetActive(true);
                Player.SetActive(false);
                BackGround.SetActive(false);
                break;
            case UIState.Normal:
                NormalCanvas.SetActive(true);
                Player.SetActive(true);
                BackGround.SetActive(true);
                break;
        }

        currentState = newState;

    }
    public void OnSettingsButton() => ChangeState(UIState.Settings);
    public void OnStartButton() => ChangeState(UIState.Start);
    public void OnKeysButton() => ChangeState(UIState.KeysSet);
    public void OnNormalButton() => ChangeState(UIState.Normal);
    public void OnOriginalSettings() => ChangeState(UIState.OriginalSettings); 
    public void OnOriginalKeysButton() => ChangeState(UIState.OriginalKeys);
}
