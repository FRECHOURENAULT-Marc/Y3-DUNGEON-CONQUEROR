using System;
using UnityEngine;

public class SRPDoorData : MonoBehaviour
{
    public GameObject corridorStart;
    public bool isConnected; // Change by SRPRoom connected with

    private void Reset()
    {
        isConnected = false;
    }
}
