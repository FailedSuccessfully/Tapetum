using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class Tapetum : MonoBehaviour
{
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


    void Awake()
    {
        vid = GetComponent<VideoPlayer>();
    }

    void Start()
    {
        projector.LightSwitch(false);
    }

    public void GetLightPosition(Vector2 pos){
        if (projector.On && wallState == (ImageDisplay.State.Searching | ImageDisplay.State.On))
            RequestSearch();
            
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            imgDisp.Wall.rectTransform,
            pos,
            Camera.main,
            out Vector2 localPos
        )){
            text.text = localPos.ToString();
        }
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
        StartCoroutine(CountToShutdown(10));
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
    
}
