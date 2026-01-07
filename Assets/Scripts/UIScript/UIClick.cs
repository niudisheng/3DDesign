using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIClick : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject left;
    private GameObject right;
    private Vector3 originalScale;
    void Awake()
    {
        originalScale = transform.localScale;
        left = this.transform.Find("left").gameObject;
        right = this.transform.Find("right").gameObject;
        ResetVisualState();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale*1.2f;
        left.SetActive(true);
        right.SetActive(true);
    }
    public void OnPointerExit(PointerEventData eventData) 
    {
        ResetVisualState();
    }
    
    void OnDisable()
    {
        ResetVisualState();// 当脚本所在 GameObject 被禁用时，强制重置状态
    }
    private void ResetVisualState()
    {
        transform.localScale = originalScale;
        if (left != null) left.SetActive(false);
        if (right != null) right.SetActive(false);
    }

}
