using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{

    public Player player;
    public SaveData saveData;
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void ChangePlayer()
    {
        player.GetSword();
    }
    public void SaveGame(GameObject player)
    {
        saveData = SaveData.SavePlayerState(player);
        // 实现保存游戏逻辑
        Debug.LogWarning("玩家成功存档");
    }
    public void LoadGame()
    {
        player.OnRespawn(saveData);
    }


    #region 特别好用的函数
    
    // 等待指定动画状态播放完毕（基于 state name）
    public IEnumerator WaitForAnimationEnd(string stateName, UnityAction onComplete,Animator animator)
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
    

    #endregion
}
