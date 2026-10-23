
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;


public class SRPGenerator : MonoBehaviour
{
    [Header("Room Style")] 
    [SerializeField] RoomStyleSO roomStyle;
    
    [Header("Map Style")]
    [SerializeField] MapStyleSO mapStyle;
    [Range(0, 1000)] 
    [SerializeField] int maxTries;    
    [Range(0, 1000)] 
    [SerializeField] int target;

    [Header("Seed")] 
    [SerializeField] int seed;
    [SerializeField] bool randomSeed;
    
    int _triesCount;
    CancellationTokenSource _cs;
    List<SRPRoomData> _generatedRooms;
    List<SRPRoomData> _validatedRooms;
    
    // Debug
    List<RayData> _rays = new();
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
        maxTries = 100;
        target = 10;
        
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
    public void StopGeneration()
    {
        _cs.Cancel();
    }
    
    [Button]
    public async Task GenerateAllSteps()
    {
        ResetAll();
        InitSeed();

        Debug.Log("Generation started...");
        
        int control = 0; // ensure no infinite loop
        int controlLimit = 200; // ensure no infinite loop
        while (_triesCount < maxTries 
               && _validatedRooms.Count < target 
               && control < controlLimit 
               && _cs.IsCancellationRequested == false)
        {
            GenerateRooms();
            BuildRoomsFloors();
            BuildRoomsWallsAndDoors();

            // wait physics update
            await Awaitable.WaitForSecondsAsync(Time.fixedDeltaTime, _cs.Token);
            if (_cs.IsCancellationRequested)
                return;
            
            BuildCorridors();
            RemoveNotConnectedRooms(); // Destroy room not connected
            
            // wait physics update
            await Awaitable.WaitForSecondsAsync(Time.fixedDeltaTime, _cs.Token);
            if (_cs.IsCancellationRequested)
                return;
            
            // need to check if rooms can reach startRoom and endRoom
            control++;
        }
        
        Debug.Log("Generation ended.");
        Debug.Log("triesCount: " + _triesCount + " rooms: " + _validatedRooms.Count + " control var: "+ control + "/" + controlLimit);

        if (await Utils.WaitOneAsyncFrame() == false)
            return;
        
        // check if all rooms have one door used
        foreach (SRPRoomData r in _validatedRooms)
        {
            int doorUsed = 0;
            foreach (SRPDoorData door in r.doors)
            {
                if (door.isUsed == false)
                    continue;
                doorUsed++;
            }
            if (doorUsed > 0)
                continue;
            
            Debug.Log("Room at: " + r.global.transform.position + " has no connection.", r.global);
        }
    }
    
    
    [Button]
    void InitSeed()
    {
        if(seed == 0 || randomSeed)
            seed = Random.Range(int.MinValue, int.MaxValue);
        Random.InitState(seed);
    }
    
    [Button]
    void GenerateRooms()
    {
        for (int i = 0; _triesCount < maxTries && (_generatedRooms.Count + _validatedRooms.Count) < target; i++)
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
            
            SRPRoomData nSrpRoomData = new ()
            {
                root = Instantiate(roomStyle.rootPrefab, transform),
            };
            nSrpRoomData.root.name += _triesCount;
            nSrpRoomData.global = Instantiate(roomStyle.roomPrefab, nSrpRoomData.root.transform);
            
            nSrpRoomData.root.transform.position = pos;
            nSrpRoomData.global.transform.localScale = scale;

            if (IsRoomValid(nSrpRoomData) == false)
            {
                nSrpRoomData.Destroy();
                continue;
            }
            
            _generatedRooms.Add(nSrpRoomData);
            _triesCount++;
        }
    }

    [Button]
    void BuildRoomsFloors()
    {
        foreach (SRPRoomData r in _generatedRooms)
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
        foreach (SRPRoomData r in _generatedRooms)
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
                    SRPDoorData doorData = doorGo.GetComponent<SRPDoorData>();
                    doorData.roomFrom = r;
                    r.doors.Add(doorData);

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
            
            r.isGenerated = true;
        }
        
    }
    
    [Button]
    void BuildCorridors()
    {
        // Add first room 
        if (_validatedRooms.Count == 0)
        {
            Debug.Log("Add first Room: " + _generatedRooms[0].global.name, _generatedRooms[0].global);
            FromGeneratedToValidate(_generatedRooms[0]);
            // _validatedRooms.Add(_generatedRooms[0]);
            // _generatedRooms.RemoveAt(0);
        }
        
        for (int i = _generatedRooms.Count - 1; i >= 0; i--)
        {
            SRPRoomData roomData = _generatedRooms[i];
            foreach (SRPDoorData door in roomData.doors)
            {
                if(door.isUsed)
                    continue;
                
                SRPDoorData closestDoorData = FindClosestDoor(roomData, door);
                if(closestDoorData == null)
                    continue;
                
                Vector3 start = door.corridorStart.transform.position;
                Vector3 end = closestDoorData.corridorStart.transform.position;
                
                // Maximum Angle difference between corridor and door
                Vector3 dirToOther = (end - start).normalized;
                // x : Acos(x) * Mathf.Rad2Deg;
                // 0.7f : 45° max
                // 0.5f : 60° max
                // 0.3f : 72° max
                // 0.0f : 90° max
                if (Vector3.Dot(GetDoorOutward(door), dirToOther) < 0.7f) continue;       
                if (Vector3.Dot(GetDoorOutward(closestDoorData), -dirToOther) < 0.7f) continue;
                
                // Mark door as use
                door.isUsed = true;
                closestDoorData.isUsed = true;
                // Add neighbors
                roomData.neighbors.Add(closestDoorData.roomFrom);
                closestDoorData.roomFrom.neighbors.Add(roomData);
                
                // main corridor
                float scaleZ = Vector3.Distance(start, end);
                GameObject corridor = Instantiate(roomStyle.corridorPrefab, roomData.root.transform);
                corridor.transform.position = new Vector3(
                    (start.x + end.x) / 2f, 
                    (start.y + end.y) / 2f, 
                    (start.z + end.z) / 2f);
                corridor.transform.LookAt(end);
                corridor.transform.localScale = new Vector3(1, 1, scaleZ);
                roomData.corridors.Add(corridor);
                
                // Link doors <-> corridor edges
                BuildDoorLink(roomData, door, corridor);
                BuildDoorLink(roomData, closestDoorData, corridor);

                FromGeneratedToValidate(roomData);
                // _validatedRooms.Add(roomData);
                // _generatedRooms.RemoveAt(i);
            }
        }
    }

    [Button]
    void RemoveNotConnectedRooms()
    {
        DestroyObjectsInList(ref _generatedRooms);
        
        // for (int i = _generatedRooms.Count - 1; i >= 0; i--)
        // {
        //     SRPRoomData room = _generatedRooms[i];
        //     
        //     // test if room can be reach by start room
        //     
        //     bool isConnected = false;
        //     foreach (SRPDoorData door in room.doors)
        //     {
        //         if (door.isUsed == false)
        //             continue;
        //         isConnected = true;
        //         break;
        //     }
        //     if (isConnected)
        //         continue;
        //     
        //     _generatedRooms[i].Destroy();
        //     _generatedRooms.RemoveAt(i);
        // }
    }
    
    [Button]
    void ResetAll()
    {
        InitIfNull();
        Clear();
    }
    
    // Utils
    
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
        
        _generatedRooms ??= new List<SRPRoomData>();
        _validatedRooms ??= new List<SRPRoomData>();

        _cs ??= new CancellationTokenSource();
    }
    
    static public void DestroyObjectsInList(ref List<GameObject> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
            DestroyImmediate(list[i]);
        list.Clear();
    }    
    static public void DestroyObjectsInList(ref List<SRPRoomData> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
            list[i].Destroy();
        list.Clear();
    }    
    
    void Clear()
    {
        _rays.Clear(); // debug
        _boxs.Clear(); // debug

        _triesCount = 0;
        
        StopGeneration();
        _cs = new CancellationTokenSource();
        
        DestroyObjectsInList(ref _generatedRooms);
        DestroyObjectsInList(ref _validatedRooms);
        
        _generatedRooms.Clear();
        _validatedRooms.Clear();
        
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }

    void FromGeneratedToValidate(SRPRoomData r)
    {
        if (r == null) return;
        if (_validatedRooms.Contains(r))
        {
            Debug.LogError($"Room {r.root.name} has already been validated", r.global);
            return;
        }
        
        _validatedRooms.Add(r);

        if (_generatedRooms.Contains(r) == false)
        {
            Debug.LogError($"Room {r.root.name} has already been removed from generated", r.global);
            return;
        }
        
        _generatedRooms.Remove(r);
    }
    
    bool IsRoomValid(SRPRoomData room)
    {
        bool isValid = true;
        foreach (SRPRoomData valid in _validatedRooms)
        {
            if (CollidingWith(room.global, valid.global))
            {
                isValid = false;
                break;
            }
        }

        Vector3 pos = room.global.transform.position;
        Vector3 halfSize = Utils.GetWorldSize(room.global.GetComponent<Collider>()) / 2f;
        
        if(CollidingWithCorridorObstacle(pos, halfSize, Vector3.up, ~0, new () {room.global}))
            isValid = false;
        
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
    
    bool CollidingWithCorridorObstacle(Vector3 start, Vector3 halfSize, Vector3 dir, int mask = ~0, List<GameObject> ignore = null)
    {
        Quaternion rot = Quaternion.LookRotation(dir.normalized);
        RaycastHit[] hits = Physics.BoxCastAll(start, halfSize, dir.normalized,
            rot, dir.magnitude, 
            mask);
        
        bool ignoreListExist = (ignore == null) == false;
        
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null)
                continue;
                    
            if (hit.collider.gameObject.TryGetComponent(out CorridorObstacle obs) == false)
                continue;

            if (ignoreListExist)
            {
                if(ignore.Contains(hit.collider.gameObject))
                    continue;
            }

            return true;
        }
        return false;
    }
    
    SRPDoorData FindClosestDoor(SRPRoomData r, SRPDoorData doorData)
    {
        float dist = float.MaxValue;
        SRPDoorData closest = null;
        // get the corridor dimensions
        Vector3 corridorPrefabSize = Utils.GetWorldSize(roomStyle.corridorPrefab.GetComponent<Collider>());
        
        foreach (SRPRoomData otherRoom in _validatedRooms)
        {
            if(otherRoom == r)
                continue;
            
            foreach (SRPDoorData otherDoor in otherRoom.doors)
            {
                if(otherDoor.isUsed) 
                    continue;
                
                Vector3 start = doorData.corridorStart.transform.position;
                Vector3 end = otherDoor.corridorStart.transform.position;
                
                float d = Vector3.Distance(start, end);
                if (d > dist)
                    continue;
                
                if(CollidingWithCorridorObstacle(start, corridorPrefabSize, end - start))
                    continue;
                
                dist = d;
                closest = otherDoor;
            }
        }

        return closest;
    }
    
    /// <summary>
    /// Horizontal direction of the door exit (going to corridorStart).
    /// </summary>
    static Vector3 GetDoorOutward(SRPDoorData door)
    {
        Vector3 center = door.GetComponent<Collider>().bounds.center;
        Vector3 a = door.transform.forward;
        if (Vector3.Dot(a, door.corridorStart.transform.position - center) < 0f)
            a = -a;
        a.y = 0f;
        return a.normalized;
    }
    
    /// <summary>
    /// Create a GameObject that links door (door.corridorStart) and main corridor
    /// The GameObject get a custom mesh of 4 quads (plans)
    /// </summary>
    void BuildDoorLink(SRPRoomData roomData, SRPDoorData doorData, GameObject corridor)
    {
        Collider doorCol = doorData.GetComponent<Collider>();
        if (doorCol == null) return;

        // Dimensions 
        Vector3 doorSize = Utils.GetWorldSize(doorCol);
        Vector3 corridorSize = Utils.GetWorldSize(roomStyle.corridorPrefab.GetComponent<Collider>());

        // Door 
        Vector3 doorCenter = doorCol.bounds.center;
        Vector3 doorRight = doorData.transform.right;
        Vector3 doorUp = doorData.transform.up;

        // Corridor
        Vector3 corridorCenter = doorData.corridorStart.transform.position;
        Vector3 corridorRight = corridor.transform.right;
        Vector3 corridorUp = corridor.transform.up;
        // Align axis on door axis to keep rotation
        if (Vector3.Dot(corridorRight, doorRight) < 0f) corridorRight = -corridorRight;
        if (Vector3.Dot(corridorUp, doorUp) < 0f) corridorUp = -corridorUp;

        // bottom-left, bottom-right, top-right, top-left
        Vector3[] d = Utils.GetRectCorners(doorCenter, doorRight, doorUp, doorSize.x, doorSize.y);
        Vector3[] c = Utils.GetRectCorners(corridorCenter, corridorRight, corridorUp, corridorSize.x, corridorSize.y);

        // Build mesh (room.root local space)
        List<Vector3> vertices = new();
        List<int> triangles = new();
        // Build 4 quads
        for (int i = 0; i < 4; i++)
        {
            int next = (i + 1) % 4;
            // i=0 : bot
            // i=1 : right
            // i=2 : top
            // i=3 : left
            // Build a quad
            Utils.AddQuad(vertices, triangles, roomData.root.transform,
                d[i], d[next], c[next], c[i],
                doubleSided: false);
        }

        // Set up the new Mesh
        Mesh linkMesh = new Mesh { name = "DoorLinkMesh" };
        linkMesh.SetVertices(vertices);
        linkMesh.SetTriangles(triangles, 0);
        linkMesh.RecalculateNormals();
        linkMesh.RecalculateBounds();
        // And the Go
        GameObject linkGo = new GameObject("DoorLink");
        linkGo.transform.SetParent(roomData.root.transform, false);
        linkGo.AddComponent<MeshFilter>().sharedMesh = linkMesh;

        // Take corridor Material for the Mesh Renderer (temp)
        MeshRenderer corridorRenderer = roomStyle.corridorPrefab.GetComponentInChildren<MeshRenderer>();
        MeshRenderer linkMeshR = linkGo.AddComponent<MeshRenderer>();
        if (corridorRenderer != null)
            linkMeshR.sharedMaterial = corridorRenderer.sharedMaterial;
        // Mesh Collider
        MeshCollider linkMeshCol = linkGo.AddComponent<MeshCollider>();
        linkMeshCol.sharedMesh = linkMesh;
        linkMeshCol.convex = false; // corridor = open / not close --> not convex (im not sure if it's by default false)
        
        roomData.corridors.Add(linkGo);
    }
}
