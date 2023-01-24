using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class LightProjector : MonoBehaviour
{
    BoxCollider2D target;
    public float idleTime;
    public float tolerance;
    bool isOn = false;
    public bool On => isOn;
    float idleFor = 0;
    Vector2 frameValue;
    public UnityEvent onIdle;
    public UnityEvent<bool> onSwitch;
    public UnityEvent<Vector2> onRotation;
    public UnityEvent<string> onRotationUI;
    PlayerInput input;
    SerialController sc;
    float readTime = .25f, timer;

    void Awake(){
        timer = 0;
        input = GetComponent<PlayerInput>();
        sc = GetComponent<SerialController>();

        input.actions.FindAction("LightSwitch").performed += ctx => LightSwitch(!isOn);     
        input.actions.FindAction("Exit").performed += ctx => Application.Quit();
    }
    void Update()
    {
        timer += Time.deltaTime;
        idleFor += Time.deltaTime;
        if (idleFor > idleTime){
            onIdle.Invoke();
            idleFor = 0;
        }
        if (timer > readTime){
            foo();
            timer -= readTime;
        }
    }

    void foo(){
        char[] axis = new char[]{'X', 'Y', 'Z'};
        Vector3 rotValues = Vector3.zero;
        string output = "";
        for (int i = 0; i < 3; i++){
            string message = sc.ReadSerialMessage();
            output+= axis[i] + " ~ " + message + " | ";
            if (float.TryParse(message, out float value)){
                rotValues[i] = value;
            }
        }
        Debug.Log(rotValues * 360);
    }

    public void LightSwitch(bool isOn){
        idleFor = 0;
        this.isOn = isOn;
        onSwitch.Invoke(isOn);
    }

    public void Control(bool isOn){
        InputAction action = input.actions.FindAction("Move");

        if (isOn){
            action.performed += MoveLight;
        }
        else
            action.performed -= MoveLight;
    }

    void MoveLight(InputAction.CallbackContext ctx){
        Vector2 pos = ctx.ReadValue<Vector2>();
        if ((ctx.control.device as Pointer).delta.ReadValue().magnitude > tolerance){
            onRotation.Invoke(pos);
            //onRotationUI.Invoke(pos.ToString());
            idleFor = 0;
        }
    }

}
