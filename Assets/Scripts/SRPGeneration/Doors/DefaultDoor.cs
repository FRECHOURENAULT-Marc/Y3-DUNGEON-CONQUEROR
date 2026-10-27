using System;
using System.Collections.Generic;
using UnityEngine;

public class DefaultDoor : IDoor
{
    [Header("Inputs")]
    
    [SerializeField] GameObject closeDoorPrefab;
    [SerializeField] GameObject openDoorPrefab;

    GameObject closeDoor;
    GameObject openDoor;
    
    public override void Init()
    {
        base.Init();
        closeDoor = Instantiate(closeDoorPrefab, transform);
        closeDoor.transform.localPosition = Vector3.zero;
    }
    
    protected override void OpenDoor()
    {
        base.OpenDoor();
        Utils.Destroy(closeDoor);
        openDoor = Instantiate(openDoorPrefab, transform);
        openDoor.transform.localPosition = Vector3.zero;
    }
    
}
