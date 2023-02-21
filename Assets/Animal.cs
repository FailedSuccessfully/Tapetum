using UnityEngine;

[CreateAssetMenu(menuName = "Tapetum/Animal")]
public class Animal : ScriptableObject
{
    public Vector2 DefualtPosition;
    public Vector2 Position;
    public Vector3 DefualtOrientation, Orientation;
    public Sprite Image;
}