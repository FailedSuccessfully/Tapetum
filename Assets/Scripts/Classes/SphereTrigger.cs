using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SphereTrigger : MonoBehaviour
{
    List<AnimalMarker> inTrigger;
    Animal target = null;
    public float bufferTime;
    float timer = 0;

    // Start is called before the first frame update
    void Start()
    {
        inTrigger = new List<AnimalMarker>();
    }

    // Update is called once per frame
    void Update()
    {
        inTrigger = inTrigger.OrderBy(mark => Vector3.Distance(transform.position, mark.transform.position)).ToList();

        Animal closest = inTrigger.Count > 0 ? inTrigger[0].animal : null;
        if (closest != target)
        {
            timer = 0;
            target = closest;
        }

        timer += Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (timer > bufferTime)
        {
            //tc.GetTarget(target);
            timer = 0;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<AnimalMarker>(out AnimalMarker mark)){
            inTrigger.Add(mark);
            timer = 0;
            Debug.Log($"{other.gameObject.name} is added");
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
