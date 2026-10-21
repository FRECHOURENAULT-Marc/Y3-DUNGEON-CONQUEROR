
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

// public class GenDoor1
// {
//     public GameObject go;
//     public GameObject corridorStart;
//     public bool isConnected;
//     
//     public GenDoor1()
//     {
//         isConnected = false;
//     }
//
//     public void Destroy()
//     {
//         UnityEngine.Object.DestroyImmediate(go);
//     }
// }

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
    public GameObject corridor;

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
        
        if(corridor != null)
            UnityEngine.Object.DestroyImmediate(corridor);
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
    //List<GameObject> _validatedRooms;
    // List<GameObject> _floors;
    // List<GameObject> _walls;
    // List<GameObject> _doors;
    // List<GameObject> _corridors;
    
    // Debug
    List<RayBis> _rays = new();

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
        foreach (var ray in _rays)
        {
            Gizmos.color = Color.red;
            //Gizmos.DrawLine(Vector3.one * 10f, Vector3.one * 50f);
            Gizmos.DrawLine(ray.origin, ray.origin + ray.direction);
        }
        
        // foreach (GameObject go in _corridors)
        // {
        //     Gizmos.color = Color.red;
        //     Gizmos.DrawLine(go.transform.position, go.transform.forward * 20);
        // }
    }

    // Generation

    [Button]
    public void GenerateAllSteps()
    {
        _rays.Clear();
        
        ResetAll();
        GenerateRooms();
        BuildRoomsFloors();
        BuildRoomsWallsAndDoors();
        BuildCorridors();
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
                new Vector3(0, 90, 0),
                new Vector3(0, -90, 0),
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
                GenDoor closestDoor = FindClosestDoor(room, door);
                if(closestDoor == null)
                    continue;
                // create connection
                closestDoor.isConnected = true;
                door.isConnected = true;
                Debug.Log("Connected 2 doors");
            }
            
        }
        
    }
    
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

    GameObject FindClosestRoom(GameObject go)
    {
        float dist = float.MaxValue;
        GameObject closest = null;
        foreach (GenRoom other in _validatedRooms)
        {
            if(other.root == go)
                continue;
            
            float d = Vector3.Distance(go.transform.position, other.root.transform.position);
            if (d > dist)
                continue;
            dist = d;
            closest = other.root;
        }
        
        if(closest == null)
            Debug.LogWarning("No closest room found");
        
        return closest;
    }    
    GenDoor FindClosestDoor(GenRoom r, GenDoor door)
    {
        float dist = float.MaxValue;
        GenDoor closest = null;
        GameObject tmpCorridor = Instantiate(roomStyle.corridorPrefab);
        Vector3 corridorSize = tmpCorridor.GetComponent<Collider>().bounds.size;
        DestroyImmediate(tmpCorridor);
        //Debug.Log("CorridorSize: " + corridorSize);
        
        foreach (GenRoom otherRoom in _validatedRooms)
        {
            if(otherRoom == r)
                continue;
            
            foreach (GenDoor otherDoor in otherRoom.doors)
            {
                if(otherDoor.isConnected) 
                    continue;
                
                float d = Vector3.Distance(door.transform.position, otherDoor.transform.position);
                if (d > dist)
                    continue;
                
                Vector3 start = door.corridorStart.transform.position;
                Vector3 end = otherDoor.corridorStart.transform.position;
                Vector3 dir = new Vector3(
                    end.x - start.x,
                    end.y - start.y,
                    end.z - start.z
                );
                
                RaycastHit[] hits = Physics.RaycastAll(start, dir.normalized, dir.magnitude, 
                    LayerMask.GetMask("Default"), QueryTriggerInteraction.Collide);
                // bool isHittingObstacle = Physics.BoxCast(start, corridorSize, dir.normalized,
                //      Quaternion.identity, maxDist, 
                //      mask);
                
                bool isHittingObstacle = false;
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.gameObject.TryGetComponent(out RoomVolume vol) == false)
                        continue;

                    isHittingObstacle = true;
                    break;
                }
                
                if (isHittingObstacle)
                    continue;
                
                dist = d;
                closest = otherDoor;
            }
        }
        
        if(closest == null)
            Debug.LogWarning("No closest door found");

        //Debug
        if (closest != null)
        {
            
            Vector3 start = door.corridorStart.transform.position;
            Vector3 end = closest.corridorStart.transform.position;
            Vector3 dir = new Vector3(
                end.x - start.x,
                end.y - start.y,
                end.z - start.z
            );
            
            RayBis ray = new RayBis();
            ray.origin = start;
            ray.direction = dir;
            _rays.Add(ray);
        }
        
        return closest;
    }
}
