using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SerialController))]
public class SerialCommunicator : MonoBehaviour
{
    public UnityEvent<bool> NotifyActivity;
    public UnityEvent<Vector2> NotifyOrientation;
    public UnityEvent<Quaternion> NotifyOrientationQ;
    public UnityEvent<string> NotifyOrientationString;

    [SerializeReference] TapetumController tc;

    string cfg = Application.streamingAssetsPath + "/settings.cfg";
    SerialController controller;
    public Quaternion offset, storedOrientation;
    Flashlight flight;
    // Start is called before the first frame update
    void Awake()
    {
        offset = Quaternion.identity;
        controller = GetComponent<SerialController>();

        if (File.Exists(cfg)){
            string[] data = File.ReadAllLines(cfg);
            if (data[0] != ""){
                controller.portName = data[0];
            }
            if (data.Length > 1 && data[1] != ""){
                string[] angles = data[1].Trim(new[] {'[', ']'}).Split(',');
                offset = Quaternion.Euler(float.Parse(angles[0]), float.Parse(angles[1]), float.Parse(angles[2]));
            }
            if (data.Length > 2 && data[2] != ""){
                tc.radius = float.Parse(data[2]);
            }
        }
        
    }
    void Start()
    {
        NotifyOrientation.AddListener(vec => NotifyOrientationString.Invoke(vec.ToString()));
        storedOrientation = Quaternion.identity;
        flight = GetComponentInChildren<Flashlight>();
    }

    // Update is called once per frame
    void  Update()
    {
        AcceptMessage();
    }

    void AcceptMessage(){
        string message = controller.ReadSerialMessage();

        if (message is null || message.Length == 0){
            return;
        }

        char symbol = message[0];
        switch (symbol){
            case '!': {
                ParseIdleData(message.Substring(1));
                break;
            }
            case '@': {
                ParseOrientationData(message.Substring(1));
                break;
            }
        }
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

        Quaternion q = new Quaternion(float.Parse(values[1]), float.Parse(values[2]),float.Parse(values[3]),float.Parse(values[0]));

        NotifyOrientationQ.Invoke(q*offset);
        storedOrientation = q;
        Debug.Log($"orientation - {q} | offset - {offset} | orientation with offset = {q*offset}" );
    }

    void ParseIdleData(string s){

        if (bool.TryParse(s, out bool activity)){
            NotifyActivity.Invoke(activity);
        }
        else
            Debug.LogError($"Failed to Parse Idle data for string - {s}", this);
    }
}
