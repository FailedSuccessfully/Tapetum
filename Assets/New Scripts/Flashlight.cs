using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    Vector3 point = Vector3.positiveInfinity;
    Quaternion tilt;
    SerialCommunicator scom;
    // Start is called before the first frame update
    void Start()
    {
        scom = GetComponentInParent<SerialCommunicator>();
        tilt = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    { 
        CastOnWall(); 
    }

    void FixedUpdate()
    {
        Quaternion rotation = Quaternion.RotateTowards(transform.rotation, tilt, 360 * Time.fixedDeltaTime);
        transform.rotation = rotation;
    }

    public void ReceiveTilt(Quaternion q){
        tilt = q;
    }

    void CastOnWall(){
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit hit, 100))
        {
            point = hit.point;
            //Debug.Log(hit.transform.GetComponent<RectTransform>().TransformPoint(point));
            scom.NotifyOrientation.Invoke(point.normalized);
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.green);
        }        
        else
        {
            point = Vector3.positiveInfinity;
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.red);
        }
    }

    void OnDrawGizmos()
    {
        if (point != Vector3.positiveInfinity){
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(point, 1);
        }
    }
}
