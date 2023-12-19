using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.IO;

public enum AppState
{
    IdleOff,
    InfoOn,
    Searching,
    AnimalScoped
}


public class TapetumController : MonoBehaviour{

    /// <summary>
    /// internal class to manage state
    /// probably over engieneered
    /// </summary>
    static class StateManager{
        public static AppState state = AppState.IdleOff;
        public static float StateTimer => outTimer;
        static TapetumController boundObject;

        public static bool flashlightOn;
        public static bool showcaseMode;
        static float outTimer = 0;
        static Animal target;
        static Coroutine myTimer = null;

        //TODO: delete if unused
        public static void ResetTimer(int time) => outTimer = time;

        // Too rigid 
        public static void ReceieveActivity(bool signal){
            if (signal != flashlightOn){
                if (!signal)
                {
                    SetState(AppState.IdleOff);
                }
                else {
                    if (target)
                        SetState(AppState.AnimalScoped);
                    else
                        SetState(AppState.InfoOn);
                }
            }
            flashlightOn = signal;

        }

        //kind of pointless i think
        public static void SetState(AppState toSet){
            Debug.Log($"Setting To: {toSet.ToString()}");
            state = toSet;
            boundObject.StateCallback();

            if (!showcaseMode && toSet == AppState.AnimalScoped){
                if (myTimer != null){
                    boundObject.StopCoroutine(myTimer);
                }
                myTimer = boundObject.StartCoroutine(Timer(5, () => { SetState(AppState.Searching); boundObject.flashLightLock = false; })); 
            }
        }

        //two steps necessiated by internal class. again, kind of pointless and needlessly confusing
        public static bool ReceiveAnimal(Animal animal)
        {
            bool hasAnimal = false;
            if (flashlightOn && target != animal){
                if (animal){
                    SetState(AppState.AnimalScoped);
                    hasAnimal = true;
                }
                else {
                    Debug.Log("animal null");
                    if (myTimer!= null){
                        boundObject.StopCoroutine(myTimer);
                        myTimer = null;
                        Debug.Log("Timer null");
                    }
                    if (state != AppState.Searching){
                        Debug.Log("going searching");
                        SetState(AppState.Searching);
                    }
                }
                target = animal;
            }
            return hasAnimal;
        }

        // why?
        public static void BindObject(TapetumController obj){
            boundObject = obj;
            //Debug.Log(boundObject.name);
        }

        // this is very flimsy as a solution to maintaining one timer as a coroutine
        static IEnumerator Timer(float time, UnityAction onEnd){
            float timer = time;
            while(timer > 0){
                timer -= Time.deltaTime;
                outTimer = timer;
                yield return new WaitForEndOfFrame();
            }
            onEnd.Invoke();
        }
    }
    
    static Animal target;
    static Animal lastTarget;

    public float radius, range, size, dist;
    public float animalCountdown;
    int nc = 0;

    public TMPro.TextMeshProUGUI myState;
    public TMPro.TextMeshProUGUI myTimer;

    float timer;
    Vector2 lastRead;
    static Vector2 projectionOffset;
    public static Vector2 GetOffset() => projectionOffset;
    public float dampAngle;
    

    [SerializeReference] SerialCommunicator serial;
    [SerializeReference] DisplayController display;
    [SerializeReference] Animal[] animals;
    [SerializeReference] TMPro.TextMeshProUGUI[] animalsDisplay;
    [SerializeReference] RectTransform lightSimulation;
    [SerializeReference] GameObject wallDisplay, debugDisplay, animalDebug;
    [SerializeField] bool showcaseMode;

    public bool flashLightActive, flashLightLock;
    public static AppState State => StateManager.state;
    public static Animal Target => target;

    string dataPath = "/animals.dat";

    Coroutine StateTransition;
    // Start is called before the first frame update
    void Awake()
    {
        StateManager.BindObject(this);
        StateManager.showcaseMode = showcaseMode;

        dataPath = Application.persistentDataPath + dataPath;
    }
    void Start()
    {
        LoadData();
        StateTransition = null;
        lastTarget = null;
        timer = 0;
        lastRead = Vector2.zero;
        projectionOffset = Vector2.zero;
        flashLightActive = false;
        StateManager.ReceieveActivity(false);

        if (showcaseMode){
         StartCoroutine(Showcase());
         Debug.Log("0");
        }
    }
    
    internal void StateCallback(){
        display.HandleState();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < animals.Count(); i++){
            animalsDisplay[i].text = animals[i].Orientation.ToString();
        }
    }

    public IEnumerator Showcase(){
        float t = 5;
        StateManager.flashlightOn = true;
         yield return new WaitForSeconds(1);
        StateManager.SetState(AppState.IdleOff);
        Debug.Log("1");
        yield return new WaitForSeconds(t);
        StateManager.SetState(AppState.InfoOn);
        Debug.Log("2");
        yield return new WaitForSeconds(t);
        StateManager.SetState(AppState.Searching);
        Debug.Log("3");
        yield return new WaitForSeconds(t);
        
        for (int i = 0; i < animals.Count(); i++){
            target = animals[i];
            Debug.Log(i + 4);
            StateManager.ReceiveAnimal(target);
            yield return new WaitForSeconds(t);
        }
        StartCoroutine(Showcase());
    }

    void LateUpdate()
    {
        myState.text = StateManager.state.ToString();
        myTimer.text = StateManager.StateTimer > 0 ? Mathf.CeilToInt(StateManager.StateTimer).ToString() : "0";
        lastTarget = target;
        //display.HandleState();
    }

    public void OnCallActivity(bool activity){
        StateManager.ReceieveActivity(activity);
    }
    public void OnCallOrientation(Vector2 projection){
        lastRead = projection;
        projection -= projectionOffset;
        Vector2 extents = RectTransformUtility.CalculateRelativeRectTransformBounds(lightSimulation.parent).extents;
        Vector2 projectedPosition = new Vector2(projection.x, projection.y);
        projectedPosition.Scale(extents);
        lightSimulation.anchoredPosition = projectedPosition;

    }

    public void GetTarget(Animal animal){
        target = animal;
        if (target  != lastTarget){
            if (StateManager.ReceiveAnimal(target))
            {
                flashLightLock = true;
            }
        }
    }


    public void SaveCurrentPosition(InputAction.CallbackContext ctx){
        int index = int.Parse(ctx.control.displayName) - 1;
        Animal target = animals[index];
        target.Position = lastRead - projectionOffset;
        target.Orientation = (serial.storedOrientation * serial.offset).eulerAngles;
        SaveData();
        serial.flight.SortMarkers();
    }

    public void PositionDefaults(){
        foreach (Animal a in animals){
            a.Position = a.DefualtPosition;
            a.Orientation = a.DefualtOrientation;
        }
        SaveData();
    }

    public void QuitApplication() => Application.Quit();

    public void SetOffsets() {projectionOffset = lastRead; Debug.Log(projectionOffset);}

    public void ToggleWall(){
        wallDisplay.SetActive(!wallDisplay.activeSelf);   
    }
    public void ToggleDebug(){
        debugDisplay.SetActive(!debugDisplay.activeSelf);  
        animalDebug.SetActive(!animalDebug.activeSelf); 
    }

    void LoadData(){
        if (File.Exists(dataPath)){
            string[] loadedData = File.ReadAllLines(dataPath);
            for (int i = 0; i < animals.Length; i++){
                Sprite img = animals[i].Image;
                JsonUtility.FromJsonOverwrite(loadedData[i], animals[i]);
                animals[i].Image = img;
            }
        }
        else {
            SaveData();
        }
    }

    void SaveData(){
        string[] s = new string[animals.Length];
        for (int i = 0; i < animals.Length; i++){
            s[i] = animals[i].AsJSON();
        }

        File.WriteAllLines(dataPath, s);
    }
}