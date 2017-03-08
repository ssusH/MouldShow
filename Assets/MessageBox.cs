using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour {

    public GameObject obj;
	// Use this for initialization
	void Start () {
        obj = Instantiate(obj);
        obj.name = "Messagebox";
        obj.transform.parent = GameObject.Find("Canvas").transform;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    public void showMessage(string message)
    {
        obj.transform.FindChild("Text").GetComponent<Text>().text = message;
    }
}
