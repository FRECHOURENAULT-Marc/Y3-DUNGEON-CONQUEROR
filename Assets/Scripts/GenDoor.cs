using System;
using UnityEngine;

public class GenDoor : MonoBehaviour
{
    public GameObject corridorStart;
    public bool isConnected;

    private void Reset()
    {
        isConnected = false;
    }
}
