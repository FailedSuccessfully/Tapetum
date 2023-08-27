using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingUtility : MonoBehaviour
{
    public TextAsset input;
    SerialCommunicator sc;
    string[] lines;
    // Start is called before the first frame update
    void Awake()
    {
        sc = GetComponent<SerialCommunicator>();
        lines = input.text.Split(System.Environment.NewLine);
    }
    void Start()
    {
        StartCoroutine(Feed());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public IEnumerator Feed(){
        foreach (string line in lines)
        {
            if (line.IndexOf('@') != -1){
                sc.AcceptMessage(line.Substring(line.IndexOf('@')));
                yield return new WaitForSeconds(0.1f);
            }
            else {
                Debug.Log(line);
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
