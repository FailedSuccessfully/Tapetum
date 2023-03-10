using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class transform_test : MonoBehaviour
{
    SerialController sc;
    float roll, pitch, yaw;
    float w, x, y ,z;

    Quaternion offset;
    // Start is called before the first frame update
    void Start()
    {
        offset = Quaternion.identity;
        sc = GetComponent<SerialController>();
        roll = 0f;
        pitch = 0f;
        yaw = 0f;

        sc.ReadSerialMessage();// drop first message
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log(transform.rotation);
        // do_test();
        // transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(pitch, yaw, roll), 100f);

        do_test_q();
        Quaternion q = new Quaternion(x, z, y, w);
        if (Keyboard.current.rKey.wasPressedThisFrame){
            offset = q;
        }
        q*= offset;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, q, 100f);
        Debug.Log(transform.rotation);
    }

    void do_test(){
        string message = sc.ReadSerialMessage();

        if (message != null){
            string[] values = message.Split('~');
            float[] results = GetResults(values);
            yaw = results[0];
            pitch = results[1];
            roll = results[2];
        }
    }
    void do_test_q(){
        string message = sc.ReadSerialMessage()?.Substring(1);

        if (message != null){
            string[] values = message.Split('~');
            float[] results = GetResults(values);
            x = results[1];
            y = results[2];
            z = results[3];
            w = results[0];
        }
    }

    float[] GetResults(string[] input){
        float[] output = new float[input.Length];

        for (int i = 0; i < input.Length; i++){
            output[i] = 1f;
            if (input[i].StartsWith('-')){
                output[i] *= -1;
                input[i] = input[i].TrimStart('-');
            }

            if (float.TryParse(input[i], out float parsed))
                output[i] *= parsed;
        }

        return output;
    }
}
