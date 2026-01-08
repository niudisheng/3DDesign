using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//  每个场景都放这个
public class LocalSceneManage : MonoBehaviour
{
    [Header("当前场景的人物")]
    [SerializeField] private GameObject localPlayer;

    [Header("当前场景的背景")]
    [SerializeField] private GameObject localBackground;

    void Start()
    {
        if (UIManage.Instance != null)
        {
            // 告诉UIManage使用本地的Player和Background
            UIManage.Instance.SetCurrentSceneObjects(localPlayer, localBackground);
        }
        else
        {
            Debug.LogError("UIManage实例不存在！");
        }
    }

    void OnDestroy()
    {
        if (UIManage.Instance != null)
        {
            // 场景卸载前清理引用
            UIManage.Instance.ClearSceneObjects();
        }
    }
}
