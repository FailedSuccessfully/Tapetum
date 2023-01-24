using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public struct TransitionData{
    public Sprite from;
    public Sprite to;
    public float duration;
    public bool immediate;
}
public class ImageDisplay : MonoBehaviour
{
    public enum State{
        Off,
        On,
        Scoped,
        Searching
    }
    Material mat;
    Image img;
    public Image Wall;
    bool busy;
    TransitionData? storedTransition;
    
    void Awake()
    {
        // instntiate material for runtime and set for image
        img = GetComponent<Image>();
        mat = Instantiate(img.material);
        img.material = mat;
        busy = false;
        storedTransition = null;
    }

    public void SetImmediate(Sprite sprite){
        storedTransition = new TransitionData{
                from = sprite,
                to = sprite,
                duration = 0,
                immediate = true
        };
        InvokeTransition(sprite, sprite, 0);
    }

    public bool InvokeTransition(Sprite from, Sprite to, float transitionTime){
        if (busy && !storedTransition.GetValueOrDefault().immediate) {
            storedTransition = new TransitionData{from = from, to = to, duration = transitionTime, immediate = false};
        } 
        else {
            busy = true;
            SetTransition(from, to);
            StartCoroutine(DoTransition(transitionTime));
        }
        return busy;
    }

    public void InvokeTransition(Sprite to, float transitionTime){
        Texture ttd = mat.GetTexture("_To");
        
        Sprite from = Sprite.Create((Texture2D)ttd, to.rect, to.pivot);
        InvokeTransition(from, to, transitionTime);
    }

    void SetTransition(Sprite from, Sprite to){
        mat.SetTexture("_From", from.texture);
        mat.SetTexture("_To", to.texture);
        mat.SetFloat("_Lerp", 0);
    }

    IEnumerator DoTransition(float transitionTime){
        if (transitionTime != 0){
            float rate = 1 / transitionTime;
            for (float t = 0; !storedTransition.GetValueOrDefault().immediate && t < transitionTime; t += Time.deltaTime){
                mat.SetFloat("_Lerp", t * rate);
                yield return new WaitForEndOfFrame();
            }
        }
        mat.SetFloat("_Lerp", 1);
        busy = false;
        yield return new WaitForEndOfFrame();

        if (storedTransition.HasValue){
            InvokeTransition(storedTransition.Value.to, storedTransition.Value.duration);
            storedTransition = null;
        }
    }

    public void foo(){
        Debug.Log("foo");
    }
}
