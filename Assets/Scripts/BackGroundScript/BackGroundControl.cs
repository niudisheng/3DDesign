using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundControl : MonoBehaviour
{
    // Start is called before the first frame update
    
    public Transform player;
    [Header("背景1")]
    public Transform first;
    public Transform second;
    public Transform third;
    public Transform fourth;
    [Header("背景2")]
    public Transform second2;
    public Transform third2;
    public Transform fourth2;

    private Vector2 lastPosition;

    void Awake()
    {
        lastPosition = player.position;//记录初始位置
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        DepthUpdata();

    }
    private void DepthUpdata()
    {
        //计算每一帧玩家移动的距离
        Vector2 eachPosition = new Vector2(player.position.x - lastPosition.x, player.position.y - lastPosition.y);
        //更新背景图层位置
        first.position += new Vector3(eachPosition.x, eachPosition.y, 0f);
        second.position += new Vector3(eachPosition.x * 0.8f, eachPosition.y * 0.3f, 0f);
        third.position += new Vector3(eachPosition.x * 0.6f, eachPosition.y * 0.2f, 0f);
        fourth.position += new Vector3(eachPosition.x * 0.4f, eachPosition.y * 0.1f, 0f);

        second2.position += new Vector3(eachPosition.x * 0.8f, eachPosition.y * 0.3f, 0f);
        third2.position += new Vector3(eachPosition.x * 0.6f, eachPosition.y * 0.2f, 0f);
        fourth2.position += new Vector3(eachPosition.x * 0.4f, eachPosition.y * 0.1f, 0f);
        //更新玩家位置
        lastPosition = player.position;
    }
}
