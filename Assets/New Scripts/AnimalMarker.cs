using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalMarker : MonoBehaviour
{
    public Animal animal;

    public bool activeInSphere = false;

    public void Reset()
    {
        activeInSphere= false;
        Debug.Log($"{gameObject.name} reset");
    }
}
