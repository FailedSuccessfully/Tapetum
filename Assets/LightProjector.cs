using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class LightProjector : MonoBehaviour
{
    [SerializeField] Tapetum tap;
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
    [SerializeField]RectTransform projection;
    float readTime = .25f, timer;
    Vector2 pos = Vector2.zero;
    Vector2 offset = Vector2.zero;

    void Awake(){
        timer = 0;
        input = GetComponent<PlayerInput>();
        sc = GetComponent<SerialController>();

        input.actions.FindAction("LightSwitch").performed += ctx => LightSwitch(!isOn);     
        input.actions.FindAction("Exit").performed += ctx => Application.Quit();
        input.actions.FindAction("ResetPosition").performed += ctx => SetOffset();
        input.actions.FindAction("Save").performed += Save;
        input.actions.FindAction("Defaults").performed += Default;
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
            MoveLightSerial();
            timer -= readTime;
        }
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
    
    void MoveLightSerial(){
        string message = sc.ReadSerialMessage();

        if (message != null && message.Contains('~')){
            string[] values = message.Split('~');
            pos = new Vector2(float.Parse(values[0]), float.Parse(values[1]));
            Vector2 withOffset = pos - offset;
            onRotationUI.Invoke(withOffset.ToString());
            onRotation.Invoke(withOffset);
        }
       // Debug.Log(projection.anchoredPosition);
    }

    void SetOffset(){
        offset = pos;
    }

    void Save(InputAction.CallbackContext ctx){
        int index = int.Parse(ctx.control.displayName) - 1;
        tap.SavePosition(index);
    }
    void Default(InputAction.CallbackContext ctx){
        tap.ResetPositions();
    }

}
