using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    Vector3 point = Vector3.positiveInfinity;
    Quaternion tilt;
    SerialCommunicator scom;
    public SphereCollider targetIntersect;
    public SphereCollider[] markers;
    // Start is called before the first frame update
    void Start()
    {
        scom = GetComponentInParent<SerialCommunicator>();
        tilt = Quaternion.identity;

        SortMarkers();
    }

    // Update is called once per frame
    void Update()
    { 
        Quaternion rotation = Quaternion.RotateTowards(transform.rotation, tilt, 180 * Time.deltaTime);
        transform.rotation = rotation;
    }

    void FixedUpdate()
    {
        CastOnWall(); 
    }

    public void ReceiveTilt(Quaternion q){
        tilt = q;
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
        int count = 0;
        foreach (Animal a in scom.ta)
        {
            transform.rotation = Quaternion.Euler(a.Orientation);
            CastOnWall();
            markers[count].transform.position = point;
            count++;
        }
    }
}
