using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

public class Tapetum : MonoBehaviour
{
    [SerializeReference] Animal[] animals;
    ImageDisplay.State wallState;
    [SerializeField] TMPro.TextMeshProUGUI text;
    [SerializeField] float transitionTime = 5;
    [SerializeField] float margin, buffer;
    [SerializeField] Sprite[] stateSprites;
    [SerializeField] VideoClip[] stateClips;
    [SerializeField] GameObject[] stateExtras;
    public LightProjector projector;
    public ImageDisplay imgDisp;
    VideoPlayer vid;

    Vector2 current = Vector2.zero;


    void Awake()
    {
        vid = GetComponent<VideoPlayer>();
    }

    void Start()
    {
        projector.LightSwitch(false);
    }

    public void GetLightPosition(Vector2 pos){
        //Debug.Log(pos);
        current = pos;
        bool temp =  wallState == ImageDisplay.State.Searching ||  wallState == ImageDisplay.State.On;
        if (projector.On && temp){
            Animal closest = animals.OrderBy(a => Vector2.Distance(a.Position, pos)).First();
            if (Vector2.Distance(closest.Position, pos) <= margin){
                RequestTransition(closest.Image);
                //Debug.Log(closest.name);
            }
            else {
                RequestSearch();
            }
        }
            
        // if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
        //     imgDisp.Wall.rectTransform,
        //     pos,
        //     Camera.main,
        //     out Vector2 localPos
        // )){
        //     text.text = localPos.ToString();
        // }
    }

    public void ControlLight(bool control) => projector.Control(control);
    public void OnSwitch(bool isOn) {
        StopAllCoroutines();
        if (isOn){
            RequestTransition(stateSprites[1]);
            SetClip(1);
            EnableExtra(1);
            wallState = ImageDisplay.State.On;
        }
        else {
            imgDisp.SetImmediate(stateSprites[0]);
            SetClip(0);
            EnableExtra(0);
            wallState = ImageDisplay.State.Off;
        }
    }
    public void RequestTransition(Sprite to) => imgDisp.InvokeTransition(to, transitionTime);
    public void RequestSearch() { 
        if (wallState != ImageDisplay.State.Searching){
        if (projector.On){
            imgDisp.InvokeTransition(stateSprites[2], transitionTime);
            EnableExtra(2);
            SetClip(1);
        }
        wallState = ImageDisplay.State.Searching;
        //StartCoroutine(CountToShutdown(10));
        }
    }

    public void ScopeAnimal(){
        StopAllCoroutines();
        DisableExtras();
        wallState = ImageDisplay.State.Scoped;
        StartCoroutine(CountToSearch(5));
    }

    IEnumerator CountToShutdown(float timeout){
        while (timeout > 0 && wallState == ImageDisplay.State.Searching) {
            yield return new WaitForEndOfFrame();
            timeout -= Time.deltaTime;
        }
        if (timeout <= 0){
            projector.LightSwitch(false);
        }
    }

    IEnumerator CountToSearch(float timeout){
        while (timeout > 0 && wallState == ImageDisplay.State.Scoped) {
            yield return new WaitForEndOfFrame();
            timeout -= Time.deltaTime;
        }
        if (timeout <= 0){
            RequestSearch();
        }
    }

    void EnableExtra(int index){
        if (index < stateExtras.Length){
            DisableExtras();
            stateExtras[index].SetActive(true);
        }
    }

    void DisableExtras(){
            for (int i = 0; i < stateExtras.Length; i++){
                stateExtras[i].SetActive(false);
            }
    }

    void SetClip(int index){
        if (stateClips.Length > index && stateClips[index] != vid.clip){
            vid.clip = stateClips[index];
        }
            vid.time = 0;
            vid.Play();
    }
    
    public void SavePosition(int n){
        animals[n].Position = current;
    }

    public void ResetPositions(){
        foreach (Animal a in animals){
            a.Position = a.DefualtPosition;
        }
    }
    
}
