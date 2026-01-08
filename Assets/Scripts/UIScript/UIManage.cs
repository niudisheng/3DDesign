using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIManage : MonoBehaviour
{
    public static UIManage Instance { get; private set; }
    private PlayerControls controls;
    private enum UIState { Start, Settings, OriginalSettings, KeysSet, OriginalKeys, Normal, Bag }
    private GameObject Player;
    private GameObject BackGround;

    [Header("页面")]
    [SerializeField] private GameObject StartCanvas;
    [SerializeField] private GameObject SetCanvas;
    [SerializeField] private GameObject OriginalSetCanvas;
    [SerializeField] private GameObject NormalCanvas;
    [SerializeField] private GameObject KeysCanvas;
    [SerializeField] private GameObject OriginalKeysCanvas;
    private UIState currentState;
    private bool isInitialized = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // 如果已经有一个实例存在，销毁这个新的
            Destroy(gameObject);
            return; 
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (!isInitialized)
        {
            Initialize();
            isInitialized = true;
        }
    }
    void Initialize()//初始化
    {
        controls = new PlayerControls();
        controls.Player.UISettings.performed += ctx => OnSettingsButton();
        ChangeState(UIState.Start);
    }
    void OnDisable()
    {
        controls.Player.Disable();  
    }

    private void ChangeState(UIState newState)
    {
        if (currentState == UIState.Settings || currentState == UIState.KeysSet)
        {
            Time.timeScale = 1f;
            if (Player != null) Player.SetActive(false);
            if (BackGround != null) BackGround.SetActive(false);
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
                if (Player != null) Player.SetActive(false);
                if (BackGround != null) BackGround.SetActive(false);
                DisableGameplayInput();
                Debug.Log("禁用输入");
                break;
            case UIState.Settings:
                SetCanvas.SetActive(true);
                if (Player != null) Player.SetActive(true);
                if (BackGround != null) BackGround.SetActive(true);
                Time.timeScale = 0f;
                DisableGameplayInput();
                break;
            case UIState.KeysSet:
                KeysCanvas.SetActive(true);
                if (Player != null) Player.SetActive(true);
                if (BackGround != null) BackGround.SetActive(true);
                Time.timeScale = 0f;
                DisableGameplayInput();
                break;
            case UIState.OriginalSettings:
                OriginalSetCanvas.SetActive(true);
                if (Player != null) Player.SetActive(false);
                if (BackGround != null) BackGround.SetActive(false);
                DisableGameplayInput();
                break;
            case UIState.OriginalKeys:
                OriginalKeysCanvas.SetActive(true);
                if (Player != null) Player.SetActive(false);
                if (BackGround != null) BackGround.SetActive(false);
                DisableGameplayInput();
                break;
            case UIState.Normal:
                NormalCanvas.SetActive(true);
                if (Player != null) Player.SetActive(true);
                if (BackGround != null) BackGround.SetActive(true);
                EnableGameplayInput();
                OnBeginButton();
                break;
        }

        currentState = newState;
    }

    private void OnBeginButton()
    {
        /*
        NormalCanvas.SetActive(true);
        Player.SetActive(true);
        BackGround.SetActive(true);
        */
        SceceLoadManager.LoadScene(GlobalValues.SceneData.StartScene);
        
    }

    public void OnSettingsButton() => ChangeState(UIState.Settings);
    public void OnStartButton() => ChangeState(UIState.Start);
    public void OnKeysButton() => ChangeState(UIState.KeysSet);
    public void OnNormalButton() => ChangeState(UIState.Normal);
    public void OnOriginalSettings() => ChangeState(UIState.OriginalSettings); 
    public void OnOriginalKeysButton() => ChangeState(UIState.OriginalKeys);
    
    private void EnableGameplayInput()// 启用游戏输入
    {
        controls.Player.Enable();
    }
    private void DisableGameplayInput() // 禁用游戏输入
    {
        controls.Player.Disable();
    }
    public void SetCurrentSceneObjects(GameObject scenePlayer, GameObject sceneBackground)
    {
        if (scenePlayer != null)
        {
            Player = scenePlayer;
        }
        if (sceneBackground != null)
        {
            BackGround = sceneBackground;
        }
    }
    // 清除场景对象
    public void ClearSceneObjects()
    {
        Player = null;
        BackGround = null;
    }
}
