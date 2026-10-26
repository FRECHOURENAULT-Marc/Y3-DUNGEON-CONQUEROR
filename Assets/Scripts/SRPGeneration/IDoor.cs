using System;
using System.Collections.Generic;
using UnityEngine;

public class IDoor : MonoBehaviour
{
    [Header("SRP Common Fields")]
    
    public SRPRoom roomFrom;
    
    public GameObject corridorStart;
    public List<GameObject> doorWalls;
    public bool isUsed;
    
    protected virtual void Reset()
    {
        isUsed = false;
    }
    
    protected virtual void OnDestroy()
    {
        Utils.DestroyObjectsInList(ref doorWalls);
    }

    public virtual void Init()
    {
        roomFrom.OnFinished += OnRoomFinished;
    }
    
    protected virtual void OnRoomFinished() {}
}
