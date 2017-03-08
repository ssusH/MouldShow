using UnityEngine;
using System.Collections;
using LitJson;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System;
using System.Runtime.InteropServices;
using UnityEngine.UI;

public class RenderStatus : MonoBehaviour
{
    //根模型 
    public GameObject mainObj;
    //根模型的名字
    string mainObjName;
    //要找子物体的那个物体
    Bounds bound;
    public Transform parent;
    //想找的子物体的名字
    public string childName;
    //
    //状态文件
    private JsonData statusData;
    private string jsonString;
    public TextAsset jsonFile;
    //用鼠标控制模型旋转、缩放、打开
    public Transform target;

    private int MouseWheelSensitivity = 50;
    private int MouseZoomMin = 20;
    private int MouseZoomMax = 112;
    private float normalDistance = 500;
    private bool flag_Roable = true;//自动旋转标志	
    private Vector3 originPos;
    private Quaternion originRotate;
    private Vector3 originScale;
    private float x = 0.0f;
    private float y = 0.0f;
    private System.DateTime oldTime;
    private System.DateTime nowTime;
    private string hitName = "";
    float xpos = 0, ypos = 0;
    //模型是否处于打开状态
    private bool openUp = false;
    private Color tempColor = new Color();
    Ray cameraRay;
    RaycastHit hit;
    Renderer preRender = null;
    string preHitname = "";
    string info = "";

    // Use this for initialization

  string jsonPath ="http://172.18.16.23:87/monitor/show3d";
  //string jsonPath = "http://172.18.16.21/getUrl/items.txt";
    // private string modelPath= "file://" + Application.streamingAssetsPath + "/AircraftEngine.3ds";
    private WWW www;
    private WWW wwwModel;
    private bool isCompleted = false; //配置文件是否加载完成
    private int ldStat = 0;
    private float lastTime = 0.0f;
    private int interval = 2;//interval of  read json file
    private float nextActionTime = 0.0f;
    public float period = 1.0f;
    private float flashlight = 0.0f;
    private float flashlight2 = 0.0f;
    private float guiTime = 0.0f;
    string sceneName;
    [DllImport("__Internal")]
    private static extern string StringReturnValueFunction();
    public static string UrlMsg = string.Empty;
    public Transform messageBox;

    void Start()
    {//用下面这一句才可以在webgl运行
     //访问本机json文件
        jsonString = ((TextAsset)Resources.Load("items")).text;
        statusData = JsonMapper.ToObject(jsonString);
        mainObjName = "AircraftEngine";
        mainObj = (GameObject)Instantiate(Resources.Load(mainObjName));
        AddCollider(mainObj);
        mainObj.name = mainObjName;
        openUp = false;
        //UrlMsg = StringReturnValueFunction();
        //jsonPath ="http://"+ UrlMsg + "/monitor/items.txt";
        //jsonPath = "http://" + UrlMsg + "/monitor/show3d";
        //StartCoroutine(LoadJson());
        //Camera.main.ViewportToWorldPoint()
        calculateObjectsize(mainObj);
        GetComponent<CameraFlyController>().speed = (bound.size.x + bound.size.z + bound.size.y) / 3 / 2;
        Camera.main.transform.position = bound.center + new Vector3(0, 0, -(bound.size.x + bound.size.z + bound.size.y) / 3 * 2);
        
    }

    private IEnumerator LoadJson()

    {
        www = new WWW(jsonPath);
        //Debug.Log(jsonPath);
        yield return new WaitForSeconds(1);
        yield return www;
        jsonString = "";
        if (string.IsNullOrEmpty(www.error))
        {
            jsonString = System.Text.Encoding.UTF8.GetString(www.bytes);
       //jsonString = System.Text.Encoding.UTF8.GetString(www.bytes, 3, www.bytes.Length - 3);  // Skip thr first 3 bytes (i.e. the UTF8 BOM)
            statusData = JsonMapper.ToObject(jsonString);
        }
  
        //获取根模型，供MoveModel.cs使用
        mainObjName = "AircraftEngine";
        // mainObj = (GameObject)Resources.LoadAssetAtPath("AssetsAssets\Resources\AircraftEngine.3ds", typeof(GameObject));
        mainObj = (GameObject)Instantiate(Resources.Load(mainObjName));
        AddCollider(mainObj);
        mainObj.name = "AircraftEngine";
    }
    private IEnumerator LoadStatData()
    {
        WWW wwwState = new WWW(jsonPath);
        yield return wwwState;
        jsonString = "";
        if (string.IsNullOrEmpty(wwwState.error))
        {
            jsonString = System.Text.Encoding.UTF8.GetString(www.bytes);
        // jsonString = System.Text.Encoding.UTF8.GetString(wwwState.bytes, 3, wwwState.bytes.Length - 3);  // Skip thr first 3 bytes (i.e. the UTF8 BOM)
            statusData = JsonMapper.ToObject(jsonString);
        }
    }

    // Update is called once per frame
    void Update()
    {
        nextActionTime += Time.deltaTime;
        if ((nextActionTime > period) && (Time.time > interval))
        {
            nextActionTime = 0.0f;
            //StartCoroutine(LoadStatData());
            //访问本机json文件

            jsonString = ((TextAsset)Resources.Load("items")).text;
            statusData = JsonMapper.ToObject(jsonString);
            
        }
        oldTime = System.DateTime.Now;
        originPos = transform.position;
        originRotate = transform.rotation;
        ////按照filePath指定的文件内容，显示模型状态（颜色）。
        if (statusData != null)
        {
            UpdateStatusAll(statusData);
            //用鼠标控制模型旋转，缩放，打开
            mouseRotateAndScale(mainObj.transform);
        }

        cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(cameraRay, out hit) && messageBox.gameObject.active != true) 
        {

            if (Input.GetMouseButtonDown(0))
                showMessage(hit.transform.name+":"+getIntroduce(hit.transform.name));
        }
        
        
        //定期接收新状态信息
        //实时更新显示各部件的状态
        //将状态文件读入到jsonString字符串
        // jsonString = File.ReadAllText(Application.dataPath + "/Resources/items.json");
        //用下面这一句才可以在webgl运行
        //  jsonString = ((TextAsset)Resources.Load("items")).text;
    }
    void AddCollider(GameObject check)
    {
        foreach (Transform t in check.GetComponentsInChildren<Transform>())
        {
            if (t.name != mainObjName)
                t.gameObject.AddComponent<MeshCollider>();
        }
    }
    //按照filePath指定的文件内容，显示模型状态（颜色）。适用于更新所有组件状态
    void UpdateStatusAll(JsonData statusData)
    {

        //将字符串转化为json object
         //statusData = JsonMapper.ToObject(filePath);

        //遍历所有装备模型子部件信息
        for (int i = 0; i < statusData["statusList"].Count; i++)
        {
            UpdateStatus(mainObj.transform, statusData["statusList"][i]["name"].ToString(), statusData["statusList"][i]["value"].ToString());
        }
    }

    //根据status值改变部件的颜色，适用于更新一个组件的状态
    void UpdateStatus(Transform check, string name, string status)
    {
        Transform t;
        t = check.FindChild(name);
        switch (status)
        {
            case "1":
                {
                    flashlight = flashlight + Time.deltaTime;

                    if (flashlight > 0.3)
                    {
                        flashlight2 = flashlight2 + Time.deltaTime;
                        if (flashlight2 > 0.1)
                        {
                            flashlight = 0;
                            flashlight2 = 0;
                        }
                        t.GetComponent<Renderer>().material.color = Color.yellow;
                        //    Debug.Log(flashlight + " yellow");

                    }
                    else
                    {
                        t.GetComponent<Renderer>().material.color = Color.red;
                        // Debug.Log(flashlight + " Red");
                    }
                    //  t.GetComponent<Renderer>().material.color =new Color(0.04f,0.89f,0.87f, 0.1f);;
                    break;
                }
            case "2":
                t.GetComponent<Renderer>().material.color = new Color(0.8f, 0.42f, 0.11f, 0.3f); ;
                break;
            case "3":
                t.GetComponent<Renderer>().material.color = new Color(0.5f, 0.2f, 0.7f, 0.6f);
                break;
            case "4":
                t.GetComponent<Renderer>().material.color = new Color(0.17f, 0.36f, 0.60f, 0.1f);
                break;
            case "5":
                t.GetComponent<Renderer>().material.color = new Color(0.39f, 0.88f, 0.03f, 0.6f);
                break;
            default:
                break;
        }
        //foreach (Transform t in check.GetComponentsInChildren<Transform>())
        //{
        //    if (t.name == name)
        //    {
        //        switch (status)
        //        {
        //            case "1":
        //                {
        //                    flashlight = flashlight + Time.deltaTime;

        //                    if (flashlight > 0.3)
        //                    {
        //                        flashlight2 = flashlight2 + Time.deltaTime;
        //                        if (flashlight2 > 0.1)
        //                        {
        //                            flashlight = 0;
        //                            flashlight2 = 0;
        //                        }
        //                        t.GetComponent<Renderer>().material.color = Color.yellow;
        //                    //    Debug.Log(flashlight + " yellow");

        //                    }
        //                    else
        //                    {
        //                        t.GetComponent<Renderer>().material.color = Color.red;
        //                       // Debug.Log(flashlight + " Red");
        //                    }
        //                    //  t.GetComponent<Renderer>().material.color =new Color(0.04f,0.89f,0.87f, 0.1f);;
        //                    break;
        //                }
        //            case "2":
        //                t.GetComponent<Renderer>().material.color = new Color(0.8f, 0.42f, 0.11f, 0.3f); ;
        //                break;
        //            case "3":
        //                t.GetComponent<Renderer>().material.color = new Color(0.5f, 0.2f, 0.7f, 0.6f);
        //                break;
        //            case "4":
        //                t.GetComponent<Renderer>().material.color = new Color(0.17f, 0.36f, 0.60f, 0.1f);
        //                break;
        //            case "5":
        //                t.GetComponent<Renderer>().material.color = new Color(0.39f, 0.88f, 0.03f, 0.6f);
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //}
    }
 
    void mouseRotateAndScale(Transform check)
    {
        nowTime = System.DateTime.Now;
        System.TimeSpan ts1 = new System.TimeSpan(oldTime.Ticks);
        System.TimeSpan ts2 = new System.TimeSpan(nowTime.Ticks);
        System.TimeSpan ts = ts2.Subtract(ts1).Duration();
        if (ts.Seconds > 5 && !Input.anyKey)
        {
            flag_Roable = true;
            oldTime = System.DateTime.Now;
        }
        if (flag_Roable)//自动旋转
        {
            x -= Time.deltaTime * 30;
            var rotation = Quaternion.Euler(0, x, 0);
            transform.RotateAround(mainObj.transform.position, Vector3.up, 0.3f);
        }
        
    }
    public void rotateCamera()
    {
        flag_Roable = true;
    }
    public void stoprotateCamera()
    {
        flag_Roable = false;
    }

    public void freeCamera()
    {
        transform.GetComponent<CameraFlyController>().enabled = true;
    }
    public void stopfreeCamera()
    {
        transform.GetComponent<CameraFlyController>().enabled = false;
    }
    public void openUpmodel()
    {
        
        foreach (Transform t in mainObj.transform.GetComponentsInChildren<Transform>())
        {
            if (t.name != mainObjName)
            {
                t.Translate(t.GetComponent<Renderer>().bounds.center * 0.1f, Space.World);
            }
                
        }
        openUp = true;
    }
    public void reBuildmodel()
    {
        Destroy(mainObj);
        mainObj = null;
        mainObj = (GameObject)Instantiate(Resources.Load(mainObjName));
        AddCollider(mainObj);
        mainObj.name = mainObjName;
        //      check = mainObj.transform;
        openUp = false;
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
            if(t.name != mainObjName)
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
        //obj.AddComponent<BoxCollider>();
        //obj.GetComponent<BoxCollider>().center = bound.center;
        //obj.GetComponent<BoxCollider>().size = bound.size;
        
        
    }

    public void showMessage(string message)
    {
        messageBox.gameObject.SetActive(true);
        //messageBox.GetComponent<RectTransform>().localPosition= Camera.main.ScreenToViewportPoint( Input.mousePosition);
        messageBox.FindChild("Text").GetComponent<Text>().text = message;
    }

    public string getIntroduce(string name)
    {
        for (int i = 0; i < statusData["statusList"].Count; i++)
        {
            if (statusData["statusList"][i]["name"].ToString() == name)
                return statusData["statusList"][i]["introduce"].ToString();
        }
        return "未找到介绍信息";
    }

}



