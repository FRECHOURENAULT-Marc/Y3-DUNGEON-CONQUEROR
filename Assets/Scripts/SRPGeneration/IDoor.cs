using System;
using System.Collections.Generic;
using UnityEngine;

public class IDoor : MonoBehaviour
{
    [Header("SRP Common Fields")]
    
    public SRPRoom roomFrom;
    public IDoor otherDoor;
    
    public GameObject corridorStart;
    public List<GameObject> doorWalls;
    public bool isUsed;
    public bool isOpen;
    
    protected virtual void Reset() // override if needed
    {
        isUsed = false;
        isOpen = false;
    }
    
    protected virtual void OnDestroy() // override if needed
    {
        Utils.DestroyObjectsInList(ref doorWalls);
        if(roomFrom != null)
            roomFrom.OnFinished -= OpenDoor;
    }

    public virtual void Init() // override if needed
    {
        roomFrom.OnFinished += OpenDoor;
    }

    protected virtual void OpenDoor() // override
    {
        isOpen = true;
        if (otherDoor.isOpen)
            return;
        otherDoor.OpenDoor();
    }
}
