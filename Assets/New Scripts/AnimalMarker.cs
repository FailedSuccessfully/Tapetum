using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalMarker : MonoBehaviour
{
    public Animal animal;

    public bool activeInSphere = false;
    public float timer = 0;

    public void Reset()
    {
        activeInSphere= false;
        timer = 0;
        Debug.Log($"{gameObject.name} reset");
    }
}
