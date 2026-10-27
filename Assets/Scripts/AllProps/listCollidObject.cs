using System;
using UnityEngine;

public class listCollidObject : MonoBehaviour
{
    public event Action<GameObject> CollidEnter;
    public event Action<GameObject> CollidExit;


    void OnTriggerEnter(Collider col)
    {
        Debug.Log($"collider detected Enter: {col.gameObject}");
        CollidEnter?.Invoke(col.gameObject);
    }

    void OnTriggerExit(Collider col)
    {
        Debug.Log($"collider detected Exit: {col.gameObject}");
        CollidExit?.Invoke(col.gameObject);
    }
}
