using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGame : MonoBehaviour
{
    private GameObject startCanvas;
    private GameObject region;
    void Start()
    {
        region = transform.parent.gameObject;
        startCanvas = region.transform.parent.gameObject;
    }
    public void ButtonClick() 
    {
        startCanvas.SetActive(false);
    }
    
}
