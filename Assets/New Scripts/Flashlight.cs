using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    Vector3 point = Vector3.positiveInfinity;
    Quaternion tilt;
    SerialCommunicator scom;
    float range = 50;
    public SphereCollider targetIntersect;
    public AnimalMarker[] markers;
    public TapetumController tc;
    // Start is called before the first frame update
    void Start()
    {
        scom = GetComponentInParent<SerialCommunicator>();
        tilt = Quaternion.identity;

        targetIntersect.transform.localScale *= tc.radius;
        SortMarkers();
    }

    // Update is called once per frame
    void Update()
    { 
        //Quaternion rotation = Quaternion.RotateTowards(transform.rotation, tilt, 180 * Time.deltaTime);
        //transform.rotation = rotation;
    }

    void FixedUpdate()
    {
        //CastOnWall(); 
        targetIntersect.transform.position = new Ray(transform.position, transform.forward).GetPoint(range);
    }

    public void ReceiveTilt(Quaternion q){
        tilt = q;
        Vector3 vTilt = tilt.eulerAngles;
        vTilt.z = 0;
        transform.localRotation = Quaternion.Euler(vTilt);
    }

    void CastOnWall(){
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit hit, 100))
        {
            point = hit.point;
            targetIntersect.transform.position = point;
            //Debug.Log(hit.transform.GetComponent<RectTransform>().TransformPoint(point));
            scom.NotifyOrientation.Invoke(point.normalized);
            //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.green);
        }        
        else
        {
            point = Vector3.positiveInfinity;
            //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.red);
        }
    }
    public void SortMarkers()
    {
        Quaternion rot = transform.localRotation;
        foreach (AnimalMarker marker in markers)
        {
            Vector3 vTilt = Quaternion.Euler(marker.animal.Orientation).eulerAngles;
            vTilt.z = 0;
            transform.localRotation = Quaternion.Euler(vTilt);
            Ray r = new Ray(transform.position, transform.forward);
            marker.transform.position = r.GetPoint(range);
        }
        transform.localRotation = rot;
    }
}
