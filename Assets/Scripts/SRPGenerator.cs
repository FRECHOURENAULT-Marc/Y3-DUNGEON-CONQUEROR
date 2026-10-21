
using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Utils {
    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static void Shuffle<T>(IList<T> ts) {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i) {
            var r = Random.Range(i, count);
            (ts[i], ts[r]) = (ts[r], ts[i]);
        }
    }
}

public class RayBis
{
    public Vector3 origin;
    public Vector3 direction;
}

public class GenRoom
{
    public GameObject root;
    public GameObject global;
    public GameObject floor;
    public List<GameObject> walls = new();
    public List<GenDoor> doors = new();
    public List<GameObject> corridors = new();

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
        
        if(corridors != null)
            SRPGenerator.DestroyObjectsInList(ref corridors);;
    }
}

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
    
    List<GenRoom> _connectedRooms;
    List<GenRoom> _validatedRooms;
    
    List<GameObject> _finalRooms;
    
    // Debug
    List<RayBis> _rays = new();
    List<Bounds> _boxs = new();

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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var ray in _rays)
            Gizmos.DrawLine(ray.origin, ray.origin + ray.direction);

        foreach (var box in _boxs)
            Gizmos.DrawWireCube(box.center, box.size);
    }

    // Generation

    [Button]
    public void GenerateAllSteps()
    {
        ResetAll();
        GenerateRooms();
        BuildRoomsFloors();
        BuildRoomsWallsAndDoors();
        BuildCorridorsAsync();
    }
    
    [Button]
    void GenerateRooms()
    {
        InitSeed();
        
        for (int i = 0; i < tries; i++)
        {
            Vector3 offest = new Vector3(
                (int)Math.Round(Random.Range(-mapStyle.width, mapStyle.width) / 2.0f), 
                (int)Math.Round(Random.Range(-mapStyle.height, mapStyle.height) / 2.0f), 
                (int)Math.Round(Random.Range(-mapStyle.depth, mapStyle.depth) / 2.0f));
            Vector3 pos = transform.position + offest;
            Vector3 scale = new Vector3(
                Random.Range(roomStyle.minWidth, roomStyle.maxWidth), 
                Random.Range(roomStyle.minHeight, roomStyle.maxHeight), 
                Random.Range(roomStyle.minDepth, roomStyle.maxDepth));
            
            GenRoom nGenRoom = new ()
            {
                root = Instantiate(roomStyle.rootPrefab, transform),
            };
            nGenRoom.global = Instantiate(roomStyle.roomPrefab, nGenRoom.root.transform);
            
            nGenRoom.root.transform.position = pos;
            nGenRoom.global.transform.localScale = scale;
            

            if (IsRoomValid(nGenRoom.global) == false)
            {
                nGenRoom.Destroy();
                continue;
            }
            
            _validatedRooms.Add(nGenRoom);
        }
    }

    [Button]
    void BuildRoomsFloors()
    {
        foreach (GenRoom r in _validatedRooms)
        {
            r.floor = Instantiate(roomStyle.floorPrefab, r.root.transform);
            Vector3 roomScale = r.global.transform.localScale;
            Vector3 floorScaleFactor = r.floor.transform.localScale;
            Vector3 offset = new Vector3(0, -r.global.transform.localScale.y / 2, 0);
            r.floor.transform.localScale = new Vector3(
                roomScale.x * floorScaleFactor.x,
                roomScale.y * floorScaleFactor.y,
                roomScale.z * floorScaleFactor.z);
            r.floor.transform.localPosition = offset;
        }
    }
    
    [Button]
    void BuildRoomsWallsAndDoors()
    {
        foreach (GenRoom r in _validatedRooms)
        {
            List<Vector3> sideOffset = new ()
            {
                new Vector3(0, 0, r.global.transform.localScale.z/2),
                new Vector3(0, 0, -r.global.transform.localScale.z/2),
                new Vector3(r.global.transform.localScale.x/2, 0, 0),
                new Vector3(-r.global.transform.localScale.x/2, 0, 0),
            };
            List<Vector3> rotations = new()
            {
                new Vector3(0, 180, 0),
                new Vector3(0, 0, 0),
                new Vector3(0, -90, 0),
                new Vector3(0, 90, 0),
            };

            List<Vector3> defaultWallScales = new()
            {
                new Vector3(r.global.transform.localScale.x, r.global.transform.localScale.y, 1),
                new Vector3(r.global.transform.localScale.z, r.global.transform.localScale.y, 1),
            };
            
            // choose rdm sides for doors
            List<int> doorSideIndex = new() { 0, 1, 2, 3 };
            Utils.Shuffle(doorSideIndex);
            int count = Random.Range(roomStyle.minConnexion, roomStyle.maxConnexion + 1);
            doorSideIndex = doorSideIndex.GetRange(0, count);
            
            // build walls for each side
            for (int i = 0; i < sideOffset.Count; i++)
            {
                // scale.x with global.localScale.x and after global.transform.localScale.y
                bool scaleFromX = i < 2;
                Vector3 wallScale = scaleFromX ? defaultWallScales[0] : defaultWallScales[1]; 
                Vector3 wallOffset = sideOffset[i];
                Vector3 wallRot = rotations[i];
                
                // generate 2 pieces wall and one door
                if (doorSideIndex.Contains(i))
                {
                    doorSideIndex.Remove(i);
                    
                    // Add door
                    GameObject doorGo = Instantiate(roomStyle.doorPrefab, r.root.transform);
                    Vector3 doorSize = doorGo.GetComponent<Collider>().bounds.size;
                    float doorSizeX = doorSize.x;
                    float doorSizeY = doorSize.y;
                    Vector3 doorOffset = new Vector3(
                        wallOffset.x,
                        -wallScale.y/2f + doorSizeY/2f,
                        wallOffset.z);
                    doorGo.transform.localPosition = doorOffset;
                    doorGo.transform.localEulerAngles = wallRot;
                    r.doors.Add(doorGo.GetComponent<GenDoor>());

                    // Add small wall on top of the door
                    GameObject topWall = Instantiate(roomStyle.wallPrefab, r.root.transform);
                    Vector3 topWallScale = new Vector3(doorSizeX, wallScale.y - doorSizeY, 1f);
                    Vector3 topWallOffset = doorOffset + new Vector3(0, doorSizeY/2f + topWallScale.y/2f, 0);
                    topWall.transform.localScale = topWallScale;
                    topWall.transform.localPosition = topWallOffset;
                    topWall.transform.localEulerAngles = wallRot;
                    
                    // Add half walls on the door's sides
                    foreach (float direction in new float[] { -1f, 1f })
                    {
                        GameObject halfWall = Instantiate(roomStyle.wallPrefab, r.root.transform);
                        Vector3 halfWallScale = new Vector3(
                            wallScale.x/2f - doorSizeX/2f, 
                            wallScale.y, 
                            wallScale.z);
                        Vector3 halfWallOffset = wallOffset + direction * new Vector3(
                            wallOffset.z != 0 ? halfWallScale.x / 2f + doorSizeX/2f : 0f,
                            0,
                            wallOffset.x != 0 ? halfWallScale.x / 2f + doorSizeX/2f : 0f);
                        
                        halfWall.transform.localScale = halfWallScale;
                        halfWall.transform.localPosition = halfWallOffset;
                        halfWall.transform.localEulerAngles = wallRot;
                        
                        r.walls.Add(halfWall);
                    }
                    continue;
                }
                
                // generate one piece wall
                GameObject wall = Instantiate(roomStyle.wallPrefab, r.root.transform);
                wall.transform.localScale = wallScale;
                wall.transform.localPosition = wallOffset;
                wall.transform.localEulerAngles = wallRot;
                r.walls.Add(wall);
            }
            
        }
        
    }

    async void BuildCorridorsAsync()
    {
        await Awaitable.NextFrameAsync();
        BuildCorridors();
    } 
    
    [Button]
    void BuildCorridors()
    {
        for (int i = _validatedRooms.Count - 1; i >= 0; i--)
        {
            GenRoom room = _validatedRooms[i];
            foreach (GenDoor door in room.doors)
            {
                if(door.isConnected)
                    continue;
                
                // Faire un path finding dans une grid ?
                // Faire un mesh de liaison door1 - corridor - door2 ?
                
                GenDoor closestDoor = FindClosestDoor(room, door);
                if(closestDoor == null)
                    continue;
                
                door.isConnected = true;
                closestDoor.isConnected = true;
                
                // main corridor
                Vector3 start = door.corridorStart.transform.position;
                Vector3 end = closestDoor.corridorStart.transform.position;
                float scaleZ = Vector3.Distance(start, end);
                GameObject corridor = Instantiate(roomStyle.corridorPrefab, room.root.transform);
                corridor.transform.position = new Vector3(
                    (start.x + end.x) / 2f, 
                    (start.y + end.y) / 2f, 
                    (start.z + end.z) / 2f);
                corridor.transform.LookAt(end);
                corridor.transform.localScale = new Vector3(1, 1, scaleZ);
                room.corridors.Add(corridor);
                
            }
        }
    }
    
    [Button]
    void ResetAll()
    {
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
        
        _connectedRooms ??= new List<GenRoom>();
        _validatedRooms ??= new List<GenRoom>();
    }
    
    static public void DestroyObjectsInList(ref List<GameObject> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
            DestroyImmediate(list[i]);
        list.Clear();
    }    
    static public void DestroyObjectsInList(ref List<GenRoom> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
            list[i].Destroy();
        list.Clear();
    }
    
    void Clear()
    {
        _rays.Clear(); // debug
        _boxs.Clear(); // debug
        
        DestroyObjectsInList(ref _connectedRooms);
        DestroyObjectsInList(ref _validatedRooms);
        
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }
    
    bool IsRoomValid(GameObject go)
    {
        bool isValid = true;
        foreach (GenRoom valid in _validatedRooms)
        {
            if (CollidingWith(go, valid.global))
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

    bool CollidingWithCorridorObstacle(Vector3 start, Vector3 halfSize, Vector3 dir, int mask)
    {
        Quaternion rot = Quaternion.LookRotation(dir.normalized);
        RaycastHit[] hits = Physics.BoxCastAll(start, halfSize, dir.normalized,
            rot, dir.magnitude, 
            mask);
        
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null)
                continue;
                    
            if (hit.collider.gameObject.TryGetComponent(out CorridorObstacle obs) == false)
                continue;

            return true;
        }
        return false;
    }
    
    GenDoor FindClosestDoor(GenRoom r, GenDoor door)
    {
        float dist = float.MaxValue;
        GenDoor closest = null;
        // get the corridor dimensions
        GameObject tmpCorridor = Instantiate(roomStyle.corridorPrefab);
        Vector3 corridorPrefabSize = tmpCorridor.GetComponent<Collider>().bounds.size;
        DestroyImmediate(tmpCorridor);
        
        foreach (GenRoom otherRoom in _validatedRooms)
        {
            if(otherRoom == r)
                continue;
            
            foreach (GenDoor otherDoor in otherRoom.doors)
            {
                if(otherDoor.isConnected) 
                    continue;
                
                Vector3 start = door.corridorStart.transform.position;
                Vector3 end = otherDoor.corridorStart.transform.position;
                
                float d = Vector3.Distance(start, end);
                if (d > dist)
                    continue;
                
                if(CollidingWithCorridorObstacle(start, corridorPrefabSize, end - start, ~0))
                    continue;
                
                dist = d;
                closest = otherDoor;
            }
        }

        if (closest == null)
            Debug.LogWarning("No closest door found");

        return closest;
    }


    
    
}
