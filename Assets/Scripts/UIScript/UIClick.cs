using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class UIClick : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TextMeshProUGUI text;
    private GameObject left;
    private GameObject right;
    private float size;
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        left = this.transform.Find("left").gameObject;
        right = this.transform.Find("right").gameObject;
        size = text.fontSize;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        text.fontSize = size*1.2f;
        left.SetActive(true);
        right.SetActive(true);
    }
    public void OnPointerExit(PointerEventData eventData) 
    {
        text.fontSize = size;
        left.SetActive(false);
        right.SetActive(false);
    }
    
}
