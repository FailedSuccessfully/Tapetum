using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SphereTrigger : MonoBehaviour
{
    TapetumController tc;
    List<AnimalMarker> inTrigger;
    public float bufferTime;

    // Start is called before the first frame update
    void Start()
    {
        tc = transform.parent.GetComponentInChildren<Flashlight>().tc;
        inTrigger = new List<AnimalMarker>();
    }

    // Update is called once per frame
    void Update()
    { 
        Animal closest;
        var activeMarkers = inTrigger.Where(mark => mark.activeInSphere);
        if (activeMarkers.Count() ==0){
            closest = null;
        } else {
            closest = activeMarkers.OrderBy(mark => Vector3.Distance(transform.position, mark.transform.position)).First().animal;
        }
        tc.GetTarget(closest);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<AnimalMarker>(out AnimalMarker mark)){
            inTrigger.Add(mark);
            Debug.Log($"{other.gameObject.name} is added");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<AnimalMarker>(out AnimalMarker mark) && !mark.activeInSphere)
        {
            mark.timer += Time.deltaTime;
            if (mark.timer >= bufferTime)
            {
                mark.activeInSphere = true;
                Debug.Log($"{other.gameObject.name} is active");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<AnimalMarker>(out AnimalMarker mark)){
            mark.Reset();
            inTrigger.Remove(mark);
            Debug.Log($"{other.gameObject.name} removed");
        }
    }
}
