using System;
using UnityEngine;

public class SRPDoorData : MonoBehaviour
{
    public GameObject corridorStart;
    public bool isConnected;

    private void Reset()
    {
        isConnected = false;
    }
}
