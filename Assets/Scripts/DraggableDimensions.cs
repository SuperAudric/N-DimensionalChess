using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class DraggableDimensions : MonoBehaviour, IDragHandler
{
    bool isDragging = false;
    bool wasDragging = false;
    private RectTransform myRectTransform;
    public Canvas myCanvas;
    public DraggableDimensionsController controller;
    // Start is called before the first frame update
    void Start()
    {
        myRectTransform = GetComponent<RectTransform>();
        controller = GameObject.Find("DraggableDimensionsBox").GetComponent<DraggableDimensionsController>();
        
        //myCanvas = transform.parent.parent.parent.GetComponent<Canvas>();
    }
    void Update()
    {
        if(wasDragging&!isDragging)
        {
            EndDrag();
        }
        wasDragging = isDragging;
        if(!Input.GetKey(KeyCode.Mouse0))
        {
            isDragging = false;
        }
    }
    public void SetText(int input)
    {
        gameObject.GetComponentInChildren<Text>().text = (input+1).ToString();
    }
    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;
        myRectTransform.anchoredPosition += eventData.delta;
    }
    public void EndDrag()
    {
        if (controller != null)
        {
            controller.ReadDimensionDisplayOrder();
        }
    }
}
