using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SerialController))]
public class SerialCommunicator : MonoBehaviour
{
    string settingPath = "/settings.dat";

    public UnityEvent<bool> NotifyActivity;
    public UnityEvent<Vector2> NotifyOrientation;
    public UnityEvent<Quaternion> NotifyOrientationQ;
    public UnityEvent<string> NotifyOrientationString;

    [SerializeReference] TapetumController tc;
    [SerializeField] int udpQueueLength = 1;
    Vector2[] udpIncoming;
    int runner = 0;
    Vector2 nullVector => Vector2.one * -1;
    public Animal[] ta;
    public Camera mainCam, simCam;
    
    string cfg = Application.streamingAssetsPath + "/settings.cfg";
    SerialController controller;
    public Quaternion offset, storedOrientation, test;
    public Flashlight flight;
    char[] trim = {' ', '[', ']' };
    // Start is called before the first frame update
    void Awake()
    {
        settingPath = Application.persistentDataPath + settingPath;
        offset = Quaternion.identity;
        controller = GetComponent<SerialController>();

        if (File.Exists(cfg)){
            string[] data = File.ReadAllLines(cfg);
            if (data[0] != ""){
                controller.portName = data[0];
            }
            if (data.Length > 1 && data[1] != "")
            {
                tc.radius = float.Parse(data[1]);
            }
            if (data.Length > 2 && data[2] != "")
            {
                tc.range = float.Parse(data[2]);
            }
            if (data.Length > 3 && data[3] != "")
            {
                tc.size = float.Parse(data[3]);
            }
            if (data.Length > 4 && data[4] != "")
            {
                udpQueueLength = int.Parse(data[4]);
            }
        }

        udpIncoming = new Vector2[udpQueueLength];
        for (int i = 0; i < udpQueueLength; i++)
        {
            udpIncoming[i] = nullVector;
        }

    }
    void Start()
    {
        LoadData();
        NotifyOrientationQ.AddListener(vec => NotifyOrientationString.Invoke(vec.ToString()));
        storedOrientation = Quaternion.identity;
        flight = GetComponentInChildren<Flashlight>();


        
    }

    // Update is called once per frame
    void  Update()
    {
    }



    public void PrintSerial(string msg) => Debug.Log(msg);

    public void AcceptMessage(string message) { 

        char symbol = message[0];
        switch (symbol){
            case '!': {
                ParseIdleData(message.Substring(1));
                break;
            }
            case '@': {
                //Debug.Log($"accepting message - {message}");
                ParseOrientationData(message.Substring(1));
                break;
            }
            case '*': {
                StartCoroutine(Reconnect());
                break;
            }
            default: {
                Debug.Log(message);
                break;
            }
        }
    }
    IEnumerator Reconnect(){
        Debug.Log("reconnect start");
        controller.enabled = false;
        yield return new WaitForSeconds(5);
        controller.enabled = true;
        Debug.Log("reconnect done");
    }

    public void SaveOffset(InputAction.CallbackContext ctx){

        if (ctx.phase == InputActionPhase.Performed){
            Debug.Log($"Offset was - {offset}");
            Debug.Log($"Changing to - {storedOrientation}");
            offset = storedOrientation;
        }
    }

    void ParseOrientationData(string s){
        string[] values = s.Split('~');

        Quaternion q = new Quaternion(float.Parse(values[0]), float.Parse(values[1]),float.Parse(values[2]),float.Parse(values[3]));

        NotifyOrientationQ.Invoke(q*offset);
        storedOrientation = q;
        //Debug.Log($"orientation - {q} | offset - {offset} | orientation with offset = {q*offset}" );
    }

    void ParseIdleData(string s){

        if (bool.TryParse(s, out bool activity)){
            NotifyActivity.Invoke(activity);
        }
        else
            Debug.LogError($"Failed to Parse Idle data for string - {s}", this);
    }

    public void InvokeLight(){
        NotifyActivity.Invoke(true);
    }
    public void InvokeOrientation(){
        //NotifyOrientationQ.Invoke(test);
        StartCoroutine(testQ());

    }

    public void ReceiveUDP(string msg)
    {
        string[] incoming = msg.Trim(trim).Split(',');
        float x, y;
        if (float.TryParse(incoming[0].Trim(), out x) && float.TryParse(incoming[1].Trim(), out y))
        {
            //TODO: verify flashlight on
            // normalize values
            x = Mathf.Lerp(0, Screen.currentResolution.width, x / 640);
            y = Mathf.Lerp(Screen.currentResolution.height, 0, y / 480);
            udpIncoming[runner] = new Vector2(x, y);
            runner++;
            runner %= udpQueueLength;

            Vector2[] udpNonNull = udpIncoming.Where(vec => vec != nullVector).ToArray();
            if (udpNonNull.Length > 0)
            {
                Vector2 sum = udpNonNull.Aggregate(Vector2.zero, (total, next) => total += next) / udpNonNull.Length;
                NotifyOrientation.Invoke(sum);
                Debug.Log(sum);
            }
        }
    }
    public void JustPrint(string toPrint) => Debug.Log(toPrint);

    IEnumerator testQ(){
        Quaternion now = Quaternion.identity;
        Quaternion next;
        for (int i = 0; i < 1; i++){
            foreach(Animal a in ta){
                next = Quaternion.Euler(a.Orientation);

                for (float t = 0; t < 2; t+= Time.deltaTime){
                    NotifyOrientationQ.Invoke(Quaternion.Slerp(now, next, t * .5f));
                    yield return new WaitForEndOfFrame();
                }
                NotifyOrientationQ.Invoke(next);
                yield return new WaitForSeconds(1);
                now = Quaternion.Euler(a.Orientation);
            }
        }
        yield return null;
    }

    void LoadData()
    {
        if (File.Exists(settingPath))
        {
            string[] loadedData = File.ReadAllLines(settingPath);
            Vector3 loaded = JsonUtility.FromJson<Vector3>(loadedData[0]);
            offset = Quaternion.Euler(loaded);
        } else
        {
            SaveData();
        }
    }

    void SaveData()
    {
        string[] s = new string[1];
        s[0] = JsonUtility.ToJson(offset.eulerAngles);

        File.WriteAllLines(settingPath, s);
    }

    public void SetSimulationVisible(bool visible)
    {
        mainCam.gameObject.SetActive(!visible);
        simCam.gameObject.SetActive(visible);
    }
    void OnMessageArrived(string msg)
    {
       AcceptMessage(msg);
    }
    void OnConnectionEvent(bool success)
    {
        if (success)
        {
            Debug.Log("connection success");
        }
        else {
            Debug.LogError("connection failed");
        }
   }
}
