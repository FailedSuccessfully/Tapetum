using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class DisplayController : MonoBehaviour
{


    [SerializeField]Image target;
    [SerializeField]Sprite[] stateSprites;
    [SerializeField]float transitionTime;
    [SerializeField] Image sequence;

    Material matInstance;
    Coroutine transition;

    // Start is called before the first frame update
    void Start()
    {
        matInstance = Instantiate(target.material);
        target.material = matInstance;
    }

    public void HandleState(){
        Sprite spr;
        bool instant = false;
        switch(TapetumController.State){
            case AppState.IdleOff:{
                spr = stateSprites[0];
                instant = true;
                break;
            }
            case AppState.InfoOn:{
                spr = stateSprites[1];
                break;
            }
            case AppState.Searching:{
                spr = stateSprites[2];
                break;
            }
            case AppState.AnimalScoped:{
                spr = TapetumController.Target.Image;
                break;
            }
            default:{
                spr = null;
                break;
            }
        }
        sequence.gameObject.SetActive(TapetumController.State == AppState.IdleOff ? true : false);
        SetDisplay(spr, instant ? 0 : transitionTime);
    }

    void SetDisplay(Sprite spr, float time){
        if (! (transition is null))
            StopCoroutine(transition);
        Texture t = matInstance.GetTexture("_To");
        if (t){
            matInstance.SetTexture("_From", t);
        }
        matInstance.SetTexture("_To", spr.texture);
        matInstance.SetFloat("_Lerp", 0);

        transition = StartCoroutine(DoTransition(time));
    }
    IEnumerator DoTransition(float transitionTime){
        if (transitionTime != 0){
            float rate = 1 / transitionTime;
            for (float t = 0; t < transitionTime; t += Time.deltaTime){
                matInstance.SetFloat("_Lerp", t * rate);
                yield return new WaitForEndOfFrame();
            }
        }
        matInstance.SetFloat("_Lerp", 1);
    }

}
