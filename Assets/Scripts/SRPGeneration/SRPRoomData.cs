using System;
using System.Collections.Generic;
using UnityEngine;

public class SRPRoomData
{
    public GameObject root;
    public GameObject global;
    public GameObject floor;
    public List<GameObject> walls = new();
    public List<GameObject> doors = new();
    public List<GameObject> corridors = new();
    
    public void Destroy()
    {
        if(root != null)
            Utils.Destroy(root);
        if(global != null)
            Utils.Destroy(global);
        if(floor != null)
            Utils.Destroy(floor);
        
        if(walls != null)
            Utils.DestroyObjectsInList(ref walls);
        
        if (doors != null)
        {
            for(int i = doors.Count - 1; i >= 0; --i)
                Utils.Destroy(doors[i]);
            doors.Clear();
        }

        if (corridors != null)
        {
            foreach (var go in corridors)
            {
                if(go == null) continue;
                Utils.Destroy(go.GetComponent<MeshFilter>()?.sharedMesh);
            }
            Utils.DestroyObjectsInList(ref corridors);
        }
            
    }
}
