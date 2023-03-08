using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;


public class Collector : MonoBehaviour
{
    bool collecting => sc.enabled;
    [SerializeField]Vector3 gyroRead;
    List<Vector3> acelRead, magRead;
    SerialController sc;
    // Start is called before the first frame update
    void Start()
    {
        acelRead = new List<Vector3>();
        magRead = new List<Vector3>();
        sc = GetComponent<SerialController>();
        sc.ReadSerialMessage();
        //sc.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            sc.enabled = !(sc.enabled);
        
        if (Keyboard.current.sKey.wasPressedThisFrame){
            OutCSV(".\\g_cal.csv", new List<Vector3>(){gyroRead});
            OutCSV(".\\a_cal.csv", acelRead);
            OutCSV(".\\m_cal.csv", magRead);
        }
        if (collecting){

            string s = sc.ReadSerialMessage();
            // Debug.Log(s);
            if (s!= null && s.Length >= 2){
                switch(s.Substring(0,2)){
                    case ("~g"): {
                        HandleGyro(s.Substring(2));
                        break;
                    }
                    case ("~m"): {
                        HandleMag(s.Substring(2));
                        break;
                    }
                    default:{
                        Debug.Log(s);
                        break;
                    }
                }
            }
        }
    }

    Vector3 ParseVector(string[] vals){
        Vector3 vec = Vector3.zero;
        for (int i=0; i<=2; i++){
            if (float.TryParse(vals[i].Trim(), out float result))
                vec[i] = result;
        }

        return vec;
    }

    void HandleGyro(string s){
        gyroRead = ParseVector(s.Split(','));
    }

    void HandleMag(string s){
        string[] duplex = s.Split(',');
        acelRead.Add(ParseVector(duplex[0..3]));
        Debug.Log(acelRead[acelRead.Count -1]);
        magRead.Add(ParseVector(duplex[3..6]));
        Debug.Log(magRead[magRead.Count -1]);
    }

    bool OutCSV(string path, List<Vector3> data){
        FileStream stream = File.OpenWrite(path);
        FileInfo fi = new FileInfo(path);
        StreamWriter sw = new StreamWriter(stream);
        foreach(Vector3 entry in data){
            sw.WriteLine($"{entry.x}, {entry.y}, {entry.z},");
        }
        sw.Close();
        stream.Close();
        Debug.Log($"saved to path: {fi.FullName}");
        return true;
    }

}
