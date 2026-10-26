using System;
using System.Collections.Generic;
using UnityEngine;

public class SRPDoor : MonoBehaviour
{
    public SRPRoomData roomFrom;
    public GameObject corridorStart;
    public List<GameObject> doorWalls;
    public bool isUsed = false;

    void OnDestroy()
    {
        Utils.DestroyObjectsInList(ref doorWalls);
    }
}
