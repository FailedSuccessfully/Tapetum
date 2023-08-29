using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SphereTrigger : MonoBehaviour
{
    TapetumController tc;
    List<AnimalMarker> inTrigger;

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
        if (inTrigger.Count ==0){
            closest = null;
        } else {
            closest = inTrigger.OrderBy(mark => Vector3.Distance(transform.position, mark.transform.position)).First().animal;
        }
        tc.GetTarget(closest);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<AnimalMarker>(out AnimalMarker mark)){
            inTrigger.Add(mark);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<AnimalMarker>(out AnimalMarker mark)){
            inTrigger.Remove(mark);
        }
    }
}
