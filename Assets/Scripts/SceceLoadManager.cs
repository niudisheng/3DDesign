using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceceLoadManager : MonoBehaviour
{
    /// <summary>
    /// 加载场景
    /// </summary>
    /// <param name="index"></param>
    public static void LoadScene(int index)
    {
        SceneManager.LoadScene(index, LoadSceneMode.Single);
    }
    
    
}
