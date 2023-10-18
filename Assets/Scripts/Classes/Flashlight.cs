using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    Vector3 point = Vector3.positiveInfinity;
    Quaternion tilt;
    public SphereCollider targetIntersect;
    public AnimalMarker[] markers;
    // Start is called before the first frame update
    void Start()
    {
        tilt = Quaternion.identity;

        targetIntersect.transform.localScale *= 1;//tc.radius;
        SortMarkers();
    }

    // Update is called once per frame
    void Update()
    {
        /*if (tc.flashLightLock) {
            Vector3 project = new Ray(transform.position, tilt * Vector3.forward).GetPoint(tc.range);
            if (Vector3.Distance(project, targetIntersect.transform.position) >= tc.dist)
            {
                tc.flashLightLock = false;
            }
        } else {
            Quaternion rotation = Quaternion.RotateTowards(transform.rotation, tilt, 180 * Time.deltaTime);
            transform.rotation = rotation;
        } */
    }

    void FixedUpdate()
    {
        //targetIntersect.transform.position = new Ray(transform.position, transform.forward).GetPoint(tc.range);
    }

    public void ReceiveTilt(Quaternion q)
    {
        tilt = q;
    }

    public void SortMarkers()
    {
        Quaternion rot = transform.localRotation;
        foreach (AnimalMarker marker in markers)
        {
            transform.localRotation = Quaternion.Euler(marker.animal.Orientation);
            Ray r = new Ray(transform.position, transform.forward);
            /*marker.transform.position = r.GetPoint(tc.range);
            marker.transform.localScale = Vector3.one * tc.size;*/
        }
        transform.localRotation = rot;
    }
}
