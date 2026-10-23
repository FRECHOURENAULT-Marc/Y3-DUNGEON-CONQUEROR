using System;
using System.Collections.Generic;
using UnityEngine;

public class SRPRoomData
{
    public GameObject root;
    public GameObject global;
    public GameObject floor;
    public List<GameObject> walls = new();
    public List<SRPDoorData> doors = new();
    public List<GameObject> corridors = new();
    
    public List<SRPRoomData> neighbors = new();
    
    public bool isGenerated = false;

    public void RefreshNeighbors()
    {
        for (int i = neighbors.Count - 1; i >= 0; --i)
        {
            if (neighbors[i] != null)
                continue;
            neighbors.RemoveAt(i);
        }
    }
    
    public void Destroy()
    {
        if(root != null)
            UnityEngine.Object.DestroyImmediate(root);
        if(global != null)
            UnityEngine.Object.DestroyImmediate(global);
        if(floor != null)
            UnityEngine.Object.DestroyImmediate(floor);
        
        if(walls != null)
            SRPGenerator.DestroyObjectsInList(ref walls);
        
        if (doors != null)
        {
            for(int i = doors.Count - 1; i >= 0; --i)
                UnityEngine.Object.DestroyImmediate(doors[i]);
            doors.Clear();
        }

        if (corridors != null)
        {
            foreach (var go in corridors)
            {
                if(go == null) continue;
                UnityEngine.Object.DestroyImmediate(go.GetComponent<MeshFilter>()?.sharedMesh);
            }
            SRPGenerator.DestroyObjectsInList(ref corridors);
        }
            
    }
}
