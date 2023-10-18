using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Tapetum/Animal"), Serializable]
public class Animal : ScriptableObject
{
    public Vector2 DefualtPosition;
    public Vector2 Position;
    public Vector3 DefualtOrientation, Orientation;
    public Sprite Image;

    public string AsJSON(){
        return JsonUtility.ToJson(this);
    } 
}