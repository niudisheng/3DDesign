using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public GameObject player;
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
        player.GetComponent<Player>().GetSword();
    }
    public void SaveGame(GameObject player)
    {
        saveData = SaveData.SavePlayerState(player);
        // 实现保存游戏逻辑
        
    }
    public void LoadGame()
    {
        player.GetComponent<PlayerInteract>().InitPlayer(saveData);
    }
}
