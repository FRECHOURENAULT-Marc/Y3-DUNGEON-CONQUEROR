using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;


public class SRPGenerator : MonoBehaviour
{
    [Header("Room Style")] 
    [SerializeField] RoomStyleSO roomStyle;
    
    [Header("Map Style")]
    [SerializeField] MapStyleSO mapStyle;
    [Range(0, 100000)] 
    [SerializeField] int maxTries;    
    [Range(0, 1000)] 
    [SerializeField] int target;

    [Header("Seed")] 
    [SerializeField] int seed;
    [SerializeField] bool randomSeed;
    
    CancellationTokenSource _cs;
    List<SRPRoomData> _generatedRooms;
    List<SRPRoomData> _uselessRooms;
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
        double startTime = Time.time;
        
        ResetAll();
        InitSeed();
        
        double resetDt = Time.time - startTime; // Debug
        Debug.Log("Generation started..."); // Debug
        
        GenerateRooms();
        
        double genRoomDt = Time.time - startTime - resetDt; // Debug
        
        BuildRooms();
        
        double buildRoomDt = Time.time - startTime - resetDt - genRoomDt; // Debug

        // wait physics update (rework without physics ? like obb class)
        await Awaitable.WaitForSecondsAsync(Time.fixedDeltaTime, _cs.Token);
        if (_cs.IsCancellationRequested)
            return;
        
        FromGeneratedToValidate(_generatedRooms[0]); // Add first room 
        BuildCorridors();
        
        double buildCorridorDt = Time.time - startTime - resetDt - genRoomDt - buildRoomDt - Time.fixedDeltaTime; // Debug
        
        RemoveNotConnectedRooms(); // Destroy room not connected
        ReplaceNotConnectedDoors(); // Replace useless doors by walls
        
        // Debug
        double cleanDt = Time.time - startTime - resetDt - genRoomDt - buildRoomDt - Time.fixedDeltaTime - buildCorridorDt;
        Debug.Log("Generation ended (total time: " + (Time.time - startTime) + ")");
        Debug.Log("Reset Time: " + resetDt + " GenTime: " + genRoomDt + " BuildRTime: " + buildRoomDt + " BuildCTime: " + buildCorridorDt + " CleanTime: " + cleanDt);
        Debug.Log("Rooms: " + _validatedRooms.Count + "/"+target);
        
        // check if all rooms have one door used
        foreach (SRPRoomData r in _validatedRooms)
        {
            int doorUsed = 0;
            foreach (GameObject doorGo in r.doors)
            {
                SRPDoor door = doorGo.GetComponent<SRPDoor>();
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
        for (int i = 0; i < maxTries; i++)
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
            nSrpRoomData.root.name += i;
            nSrpRoomData.global = Instantiate(roomStyle.roomPrefab, nSrpRoomData.root.transform);
            
            nSrpRoomData.root.transform.position = pos;
            nSrpRoomData.global.transform.localScale = scale;

            if (IsRoomValid(nSrpRoomData) == false)
            {
                nSrpRoomData.Destroy();
                continue;
            }
            
            _generatedRooms.Add(nSrpRoomData);
        }
    }

    [Button]
    void BuildRooms()
    {
        BuildRoomsFloors();
        BuildRoomsWallsAndDoors();
    } 
    
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
            
            Vector3 doorSize = Utils.GetWorldSize(roomStyle.doorPrefab.GetComponent<Collider>());
            
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
                    //Vector3 doorSize = doorGo.GetComponent<Collider>().bounds.size;
                    float doorSizeX = doorSize.x;
                    float doorSizeY = doorSize.y;
                    Vector3 doorOffset = new Vector3(
                        wallOffset.x,
                        -wallScale.y/2f + doorSizeY/2f,
                        wallOffset.z);
                    doorGo.transform.localPosition = doorOffset;
                    doorGo.transform.localEulerAngles = wallRot;
                    SRPDoor door = doorGo.GetComponent<SRPDoor>();
                    door.roomFrom = r;
                    r.doors.Add(doorGo);

                    // Add small wall on top of the door
                    GameObject topWall = Instantiate(roomStyle.wallPrefab, r.root.transform);
                    Vector3 topWallScale = new Vector3(doorSizeX, wallScale.y - doorSizeY, 1f);
                    Vector3 topWallOffset = doorOffset + new Vector3(0, doorSizeY/2f + topWallScale.y/2f, 0);
                    topWall.transform.localScale = topWallScale;
                    topWall.transform.localPosition = topWallOffset;
                    topWall.transform.localEulerAngles = wallRot;
                    door.doorWalls.Add(topWall);
                    
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
                        
                        door.doorWalls.Add(halfWall);
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
        if (_validatedRooms.Count == 0)
            FromGeneratedToValidate(_generatedRooms[0]);
        
        for (int i = _generatedRooms.Count - 1; i >= 0; i--)
        {
            // If target reached, stop generating corridor and remove remaining rooms
            if (_validatedRooms.Count >= target)
            {
                for(int j = i; j >= 0; j--)
                    FromGeneratedToUseless(_generatedRooms[j]);
                break;
            }
                
            
            SRPRoomData roomData = _generatedRooms[i];
            foreach (GameObject doorGo in roomData.doors)
            {
                SRPDoor door = doorGo.GetComponent<SRPDoor>();
                
                if(door.isUsed)
                    continue;
                
                SRPDoor closestDoor = FindClosestDoor(door);
                if(closestDoor == null)
                    continue;
                
                Vector3 start = door.corridorStart.transform.position;
                Vector3 end = closestDoor.corridorStart.transform.position;
                
                // Maximum Angle difference between corridor and door
                Vector3 dirToOther = (end - start).normalized;
                // x : Acos(x) * Mathf.Rad2Deg;
                // 0.7f : 45° max
                // 0.5f : 60° max
                // 0.3f : 72° max
                // 0.0f : 90° max
                if (Vector3.Dot(GetDoorOutward(door), dirToOther) < 0.7f) continue;       
                if (Vector3.Dot(GetDoorOutward(closestDoor), -dirToOther) < 0.7f) continue;
                
                // Mark door as use
                door.isUsed = true;
                closestDoor.isUsed = true;
                
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
                BuildDoorLink(roomData, closestDoor, corridor);

                FromGeneratedToValidate(roomData);
            }

            if (_validatedRooms.Contains(roomData))
                continue;
            
            // room cannot be connected, remove it to avoid useless computations in next checks
            // _generatedRooms[i].Destroy();
            _uselessRooms.Add(roomData);
            _generatedRooms.RemoveAt(i);
        }
    }

    [Button]
    void RemoveNotConnectedRooms()
    {
        //Utils.DestroyObjectsInList(ref _generatedRooms);
        Utils.DestroyObjectsInList(ref _uselessRooms);
    }
    
    [Button]
    void ReplaceNotConnectedDoors()
    {
        foreach (SRPRoomData r in _validatedRooms)
        {
            List<Vector3> sideOffset = new()
            {
                new Vector3(0, 0, r.global.transform.localScale.z / 2),
                new Vector3(0, 0, -r.global.transform.localScale.z / 2),
                new Vector3(r.global.transform.localScale.x / 2, 0, 0),
                new Vector3(-r.global.transform.localScale.x / 2, 0, 0),
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

            for (int i = r.doors.Count - 1; i >= 0; i--)
            {
                SRPDoor door = r.doors[i].GetComponent<SRPDoor>();
                if (door.isUsed)
                    continue;

                // delete useless door
                Utils.Destroy(r.doors[i].GetComponent<SRPDoor>());
                Utils.Destroy(r.doors[i]);

                // generate new wall
                // scale.x with global.localScale.x and after global.transform.localScale.y
                bool scaleFromX = i < 2;
                Vector3 wallScale = scaleFromX ? defaultWallScales[0] : defaultWallScales[1];
                Vector3 wallOffset = sideOffset[i];
                Vector3 wallRot = rotations[i];
                
                GameObject wall = Instantiate(roomStyle.wallPrefab, r.root.transform);
                wall.transform.localScale = wallScale;
                wall.transform.localPosition = wallOffset;
                wall.transform.localEulerAngles = wallRot;
                r.walls.Add(wall);
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
        _uselessRooms ??= new List<SRPRoomData>();

        _cs ??= new CancellationTokenSource();
    }
    
    void Clear()
    {
        _rays.Clear(); // debug
        _boxs.Clear(); // debug
        
        StopGeneration();
        _cs = new CancellationTokenSource();
        
        Utils.DestroyObjectsInList(ref _generatedRooms);
        Utils.DestroyObjectsInList(ref _validatedRooms);
        Utils.DestroyObjectsInList(ref _uselessRooms);
        
        for (int i = transform.childCount - 1; i >= 0; i--)
            Utils.Destroy(transform.GetChild(i).gameObject);
    }

    void FromGeneratedToValidate(SRPRoomData r)
    {
        if (FromTo(_generatedRooms, _validatedRooms, r))
            return;
        
        Debug.Log($"Room {r.root.name} has already been validated or removed from generated", r.global);
    }      
    void FromGeneratedToUseless(SRPRoomData r)
    {
        if (FromTo(_generatedRooms, _uselessRooms, r))
            return;
        
        Debug.Log($"Room {r.root.name} has already been moved to useless or removed from generated", r.global);
    }    
    
    bool FromTo<T>(List<T> origin, List<T> dest, T r)
    {
        if (r == null) return false;
        if (dest.Contains(r))
        {
            Debug.LogError($"{r.GetType()} has already been moved to dest list");
            return false;
        }
        
        dest.Add(r);

        if (origin.Contains(r) == false)
        {
            Debug.LogError($"{r.GetType()} has already been removed from origin list");
            return false;
        }
        
        origin.Remove(r);
        return true;
    }
    
    /// <summary>
    /// Check collision with other rooms (no physics involved)
    /// </summary>
    bool IsRoomValid(SRPRoomData room)
    {
        // Check collision with _validatedRooms
        foreach (SRPRoomData valid in _validatedRooms)
        {
            if (CollidingWith(room.global, valid.global))
                return false;
        }    
        // Check collision with _generatedRooms
        foreach (SRPRoomData gen in _generatedRooms)
        {
            if (gen == room)
                continue;
            if (CollidingWith(room.global, gen.global))
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Do a Bounds.Intersects with GameObject scales and roomStyle.padding
    /// </summary>
    bool CollidingWith(GameObject go1, GameObject go2)
    {
        Bounds b1 = new Bounds(go1.transform.position, new Vector3(
            go1.transform.localScale.x + roomStyle.padding, 
            go1.transform.localScale.y + roomStyle.padding, 
            go1.transform.localScale.z + roomStyle.padding));        
        Bounds b2 = new Bounds(go2.transform.position, go2.transform.localScale);
        
        return b1.Intersects(b2);
    }    
    /// <summary>
    /// Do a Bounds.Intersects with Collider.bounds.size and roomStyle.padding
    /// </summary>
    bool CollidingWith(Collider c1, Collider c2)
    {
        Bounds b1 = new Bounds(c1.transform.position, new Vector3(
            c1.bounds.size.x + roomStyle.padding, 
            c1.bounds.size.y + roomStyle.padding, 
            c1.bounds.size.z + roomStyle.padding));
        Bounds b2 = c2.bounds;
        
        return b1.Intersects(b2);
    }
    
    /// <summary>
    /// BoxCastAll with CorridorObstacle Component require on other Colliders
    /// </summary>
    /// <param name="ignore">List of GameObjects to skip in BoxCast</param>
    /// <returns>true if colliding with one or more GameObject with CorridorObstacle, otherwise false</returns>
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
                    
            // hit do not count for path of corridor
            if (hit.collider.gameObject.TryGetComponent(out CorridorObstacle obs) == false)
                continue;

            // if actual GameObject is in the ignore list, skip it
            if (ignoreListExist)
            {
                if(ignore.Contains(hit.collider.gameObject))
                    continue;
            }

            // collision detected
            return true;
        }
        
        // path clear
        return false;
    }
    
    /// <summary>
    /// Find the closest reachable Door
    /// </summary>
    /// <returns>Return the SRPDoor Component if succeeded, otherwise null</returns>
    [CanBeNull] SRPDoor FindClosestDoor(SRPDoor door, List<SRPDoor> ignore = null)
    {
        bool ignoreListExist = (ignore == null) == false;
        float dist = float.MaxValue;
        SRPDoor closest = null;
        // get the corridor dimensions
        Vector3 corridorPrefabHalfSize = 
            Utils.GetWorldSize(roomStyle.corridorPrefab.GetComponent<Collider>()) 
            / 2f;
        
        // for each room, try to get the closest door of doorData
        foreach (SRPRoomData otherRoom in _validatedRooms)
        {
            if(otherRoom == door.roomFrom)
                continue;
            
            // each door of the room
            foreach (GameObject otherDoorGo in otherRoom.doors)
            {
                SRPDoor otherDoor = otherDoorGo.GetComponent<SRPDoor>();
                // Skip if door is already used for another corridor
                if(otherDoor.isUsed) 
                    continue;

                if (ignoreListExist)
                {
                    if(ignore.Contains(otherDoor))
                        continue;
                }
                
                Vector3 start = door.corridorStart.transform.position;
                Vector3 end = otherDoor.corridorStart.transform.position;
                
                // Skip if the actual door is further than the last further door found
                float d = Vector3.Distance(start, end);
                if (d > dist)
                    continue;
                
                // Skip if the path to the actual door can't be reach
                if(CollidingWithCorridorObstacle(start, corridorPrefabHalfSize, end - start))
                    continue;
                
                // Keep actual door as the closest
                dist = d;
                closest = otherDoor;
            }
        }

        return closest;
    }
    
    /// <summary>
    /// Horizontal direction of the door exit (going to corridorStart).
    /// </summary>
    static Vector3 GetDoorOutward(SRPDoor door)
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
    void BuildDoorLink(SRPRoomData roomData, SRPDoor door, GameObject corridor)
    {
        Collider doorCol = door.GetComponent<Collider>();
        if (doorCol == null) return;

        // Dimensions 
        Vector3 doorSize = Utils.GetWorldSize(doorCol);
        Vector3 corridorSize = Utils.GetWorldSize(roomStyle.corridorPrefab.GetComponent<Collider>());

        // Door 
        Vector3 doorCenter = doorCol.bounds.center;
        Vector3 doorRight = door.transform.right;
        Vector3 doorUp = door.transform.up;

        // Corridor
        Vector3 corridorCenter = door.corridorStart.transform.position;
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