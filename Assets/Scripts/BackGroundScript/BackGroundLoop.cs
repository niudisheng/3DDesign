using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundLoop : MonoBehaviour
{
    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;
    private float backGroundWidth;
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        backGroundWidth = spriteRenderer.bounds.size.x;
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Loop();
    }
    private void Loop()
    {
        
        if (mainCamera.transform.position.x - spriteRenderer.bounds.max.x > backGroundWidth / 2)
        {
            transform.position = new Vector3(transform.position.x + backGroundWidth * 2, transform.position.y,transform.position.z);
        }
        else if (spriteRenderer.bounds.min.x - mainCamera.transform.position.x > backGroundWidth / 2)
        {
            transform.position = new Vector3(transform.position.x - backGroundWidth * 2, transform.position.y, transform.position.z);
        }
    }
}
