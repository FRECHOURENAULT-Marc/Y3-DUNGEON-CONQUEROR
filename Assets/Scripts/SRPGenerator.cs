
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class SRPGenerator : MonoBehaviour
{
    [Header("Room Style")] 
    [SerializeField] RoomStyleSO roomStyle;
    
    [Header("Map Style")]
    [SerializeField] MapStyleSO mapStyle;
    [Range(0, 1000)] 
    [SerializeField] int tries;

    [Header("Seed")] 
    [SerializeField] int seed;
    [SerializeField] bool randomSeed;
    
    List<GameObject> _finalRooms;
    List<GameObject> _floors;
    List<GameObject> _walls;

    public int Seed { 
        get => seed;
        private set
        {
            seed = value; 
            InitSeed();
        } 
    }
    
    // Unity
    
    private void Reset()
    {
        tries = 100;
        
        seed = 0;
        randomSeed = true;
        
    }

    // Generation

    [Button]
    public void GenerateAllSteps()
    {
        ResetAll();
        GenerateRooms();
        BuildRoomsFloors();
        BuildRoomsWalls();
    }
    
    [Button]
    void GenerateRooms()
    {
        for (int i = 0; i < tries; i++)
        {
            Vector3 offest = new Vector3(
                Random.Range(-mapStyle.width, mapStyle.width) / 2.0f, 
                Random.Range(-mapStyle.height, mapStyle.height) / 2.0f, 
                Random.Range(-mapStyle.depth, mapStyle.depth) / 2.0f);
            Vector3 pos = transform.position + offest;
            Vector3 scale = new Vector3(
                Random.Range(roomStyle.minWidth, roomStyle.maxWidth), 
                Random.Range(roomStyle.minHeight, roomStyle.maxHeight), 
                Random.Range(roomStyle.minDepth, roomStyle.maxDepth));
            
            GameObject room = Instantiate(roomStyle.roomPrefab, transform);
            room.transform.position = pos;
            room.transform.localScale = scale;

            if (IsRoomValid(room) == false)
            {
                DestroyImmediate(room);
                continue;
            }
            
            _finalRooms.Add(room);
        }
    }

    [Button]
    void BuildRoomsFloors()
    {
        foreach (GameObject go in _finalRooms)
        {
            Vector3 offset = new Vector3(0, - go.transform.localScale.y / 2 + roomStyle.floorPrefab.transform.localScale.y, 0);
            GameObject floor = Instantiate(roomStyle.floorPrefab, transform);
            floor.transform.position = go.transform.position + offset;
            floor.transform.localScale = new Vector3(go.transform.localScale.x, 1, go.transform.localScale.z);
            _floors.Add(floor);
        }
    }
    
    [Button]
    void BuildRoomsWalls()
    {
        foreach (GameObject go in _finalRooms)
        {
            List<Vector3> scales = new()
            {
                new Vector3(1/go.transform.localScale.x, 1, 1),
                new Vector3(1/go.transform.localScale.x, 1, 1),
                new Vector3(1, 1, 1/go.transform.localScale.z),
                new Vector3(1, 1, 1/go.transform.localScale.z),
            };
            List<Vector3> offsets = new()
            {
                new Vector3(-go.transform.localScale.x/2, 0, 0),
                new Vector3(go.transform.localScale.x/2, 0, 0),
                new Vector3(0, 0, -go.transform.localScale.z/2),
                new Vector3(0, 0, go.transform.localScale.z/2),
            };            

            
            for (int i = 0; i < offsets.Count; i++)
            {
                Vector3 offset = offsets[i];
                Vector3 scale = scales[i];
                GameObject wall = Instantiate(roomStyle.wallPrefab, go.transform);
                wall.transform.position = go.transform.position + offset;
                wall.transform.localScale = scale;
                _walls.Add(wall);
            }
        }
    }
    
    [Button]
    void BuildCorridors()
    {}
    
    [Button]
    void ResetAll()
    {
        InitSeed();
        InitIfNull();
        Clear();
    }
    
    // Utils

    void InitSeed()
    {
        if(seed == 0 || randomSeed)
            seed = Random.Range(int.MinValue, int.MaxValue);
        Random.InitState(seed);
    }
    
    void InitIfNull()
    {
        if (mapStyle == null)
        {
            Debug.LogError("Must have a map style");
            return;
        }        
        if (roomStyle == null)
        {
            Debug.LogError("Must have a room style");
            return;
        }
 
        _finalRooms ??= new List<GameObject>();
        _floors ??= new List<GameObject>();
        _walls ??= new List<GameObject>();


    }
    
    void Clear()
    {
        for (int i = _finalRooms.Count - 1; i >= 0; i--)
            DestroyImmediate(_finalRooms[i]);
        _finalRooms.Clear();
        for (int i = _floors.Count - 1; i >= 0; i--)
            DestroyImmediate(_floors[i]);
        _floors.Clear();
        for (int i = _walls.Count - 1; i >= 0; i--)
            DestroyImmediate(_walls[i]);
        _walls.Clear();
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }
    
    bool IsRoomValid(GameObject go)
    {
        bool isValid = true;
        foreach (GameObject valid in _finalRooms)
        {
            if (CollidingWith(go, valid))
            {
                isValid = false;
                break;
            }
        }
        return isValid;
    }
    
    bool CollidingWith(GameObject c1, GameObject c2)
    {
        Bounds b1 = new Bounds(c1.transform.position, new Vector3(
            c1.transform.localScale.x + roomStyle.padding, 
            c1.transform.localScale.y + roomStyle.padding, 
            c1.transform.localScale.z + roomStyle.padding));        
        Bounds b2 = new Bounds(c2.transform.position, c2.transform.localScale);
        
        return b1.Intersects(b2);
    }    
    bool CollidingWith(Collider c1, Collider c2)
    {
        Bounds b1 = new Bounds(c1.transform.position, new Vector3(
            c1.bounds.size.x + roomStyle.padding, 
            c1.bounds.size.y + roomStyle.padding, 
            c1.bounds.size.z + roomStyle.padding));
        Bounds b2 = c2.bounds;
        
        return b1.Intersects(b2);
    }
}
