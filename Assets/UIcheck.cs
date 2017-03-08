using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class UIcheck : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler {
    RenderStatus RS;
    

    // Use this for initialization
    void Start () {
        RS = Camera.main.GetComponent<RenderStatus>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public void OnPointerEnter(PointerEventData eventData)
    {
        RS.UIcanvas = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RS.UIcanvas = false;
    }
}
