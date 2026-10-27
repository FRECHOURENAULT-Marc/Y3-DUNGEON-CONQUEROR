using System;
using System.Collections.Generic;
using UnityEngine;

public class SRPRoom : MonoBehaviour
{
    public GameObject root;
    public GameObject global;
    public GameObject floor;
    public List<GameObject> walls = new();
    public List<GameObject> doors = new();
    public List<GameObject> corridors = new();
    
    public int biome = int.MinValue;

    public bool IsFinished { get; private set; }

    public event Action OnFinished;

    void Awake()
    {
        root = gameObject;
    }
    
    void OnDestroy()
    {
        if(root != null)
            Utils.Destroy(root); // --> destroy root destroy all children
        
        if (corridors != null)
        {
            foreach (var go in corridors)
            {
                if(go == null) continue;
                if (go.TryGetComponent(out MeshFilter sharedMesh) == false) continue;
                Utils.Destroy(sharedMesh);
            }
            Utils.DestroyObjectsInList(ref corridors);
        }
    }
    
    public void Finish()
    {
        if (IsFinished)
            return;
        
        IsFinished = true;
        OnFinished?.Invoke();
    }
}
