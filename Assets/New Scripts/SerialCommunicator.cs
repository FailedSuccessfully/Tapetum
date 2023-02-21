using System.Collections;
using System.Collections.Generic;
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

    SerialController controller;
    public Quaternion offset, storedOrientation;
    Flashlight flight;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<SerialController>();
        NotifyOrientation.AddListener(vec => NotifyOrientationString.Invoke(vec.ToString()));
        offset = Quaternion.identity;
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
            offset = storedOrientation;
        }
    }

    void ParseOrientationData(string s){
        string[] values = s.Split('~');

        Quaternion q = new Quaternion(float.Parse(values[1]), float.Parse(values[3]),float.Parse(values[2]),float.Parse(values[0]));

        NotifyOrientationQ.Invoke(q*offset);
        storedOrientation = q;
    }

    void ParseIdleData(string s){

        if (bool.TryParse(s, out bool activity)){
            NotifyActivity.Invoke(activity);
        }
        else
            Debug.LogError($"Failed to Parse Idle data for string - {s}", this);
    }
}
