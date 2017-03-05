using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectToCameraCenter : MonoBehaviour {

    public GameObject mainObject;
    Bounds bound;
    
	// Use this for initialization
	void Start () {
        calculateObjectsize(mainObject);
        Camera.main.transform.position = bound.center + new Vector3(0, 0, -(bound.size.x + bound.size.z + bound.size.y) / 3 * 2);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    public void calculateObjectsize(GameObject obj)
    {
        Bounds temp = new Bounds();
        float minX, minY, minZ;
        float maxX, maxY, maxZ;
        minX = minY = minZ = 10000f;
        maxX = maxY = maxZ = -minX;
        foreach (Transform t in obj.transform.GetComponentsInChildren<Transform>())
        {
            if (t.name != mainObject.name)
                temp = t.GetComponent<Renderer>().bounds;

            if (temp.max.x > maxX)
                maxX = temp.max.x;
            if (temp.max.y > maxY)
                maxY = temp.max.y;
            if (temp.max.z > maxZ)
                maxZ = temp.max.z;

            if (temp.min.x < minX)
                minX = temp.min.x;
            if (temp.min.y < minY)
                minY = temp.min.y;
            if (temp.min.z < minZ)
                minZ = temp.min.z;
        }
        bound = new Bounds(new Vector3((maxX + minX) / 2, (maxY + minY) / 2, (maxZ + minZ) / 2), new Vector3(maxX - minX, maxY - minY, maxZ - minZ));
        Debug.Log((maxX + minX) / 2 + " " + (maxY + minY) / 2 + " " + (maxZ + minZ) / 2);
        Debug.Log((maxX - minX) + " " + (maxY - minY) + " " + (maxZ - minZ));
        obj.AddComponent<BoxCollider>();
        obj.GetComponent<BoxCollider>().center = bound.center;
        obj.GetComponent<BoxCollider>().size = bound.size;

    }
}
