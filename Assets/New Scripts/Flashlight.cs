using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    Vector3 point = Vector3.positiveInfinity;
    Quaternion tilt;
    SerialCommunicator scom;
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
        targetIntersect.transform.position = new Ray(transform.position, transform.forward).GetPoint(tc.range);
    }

    public void ReceiveTilt(Quaternion q)
    {
        if (Quaternion.Angle(tilt, q) > tc.deadAngle)
            tilt = q;
        transform.localRotation = tilt;
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
            transform.localRotation = Quaternion.Euler(marker.animal.Orientation);
            Ray r = new Ray(transform.position, transform.forward);
            marker.transform.position = r.GetPoint(tc.range);
            marker.transform.localScale = Vector3.one * tc.size;
        }
        transform.localRotation = rot;
    }
}
