using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NaughtyAttributes;
using Unity.AI.Navigation;
using UnityEngine;
using Random = UnityEngine.Random;


public class SRPGenerator : MonoBehaviour
{
    [Header("Room Style")] 
    [SerializeField] RoomStyleSO roomStyle;    
    
    [Header("Map Style")]
    [SerializeField] MapStyleSO mapStyle;
    
    [Header("Generation")]
    [SerializeField] GenerationMenuParameters parameters;
    [Header("Old Fields")]
    [SerializeField] bool generateWhenStarting;
    [Range(0, 100000)] 
    [SerializeField] int maxTries;    
    [Range(0, 1000)] 
    [SerializeField] int target;
    [Header("Seed")] 
    [SerializeField] int seed;
    [SerializeField] bool randomSeed;
    
    CancellationTokenSource _cs;
    List<SRPRoom> _generatedRooms;
    List<SRPRoom> _uselessRooms;
    List<SRPRoom> _validatedRooms;

    SRPRoom startRoom;
    SRPRoom endRoom;
    
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
        generateWhenStarting = false;
        
        maxTries = 100;
        target = 10;
        
        seed = 0;
        randomSeed = true;
        
    }

    void Start()
    {
        seed = parameters.seed;
        randomSeed = parameters.rdmSeed;
        maxTries = parameters.generationEffort;
        target = parameters.maxRooms;
        
        DependenciesInjector.PDeps.SetWeapon(parameters.weapon);
        
        if (generateWhenStarting == false)
            return;
        GenerateAllSteps();
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
        DateTime last = DateTime.Now;
        DateTime begin = DateTime.Now;

        ResetAll();
        InitSeed();
        
        double resetDt = Utils.GetElapsedTime(last, DateTime.Now).Millisecond; // Debug
        last = DateTime.Now;
        Debug.Log("Generation started..."); // Debug
        
        GenerateRooms();
        
        double genRoomDt = Utils.GetElapsedTime(last, DateTime.Now).Millisecond; // Debug
        last = DateTime.Now;
        
        BuildRooms();
        
        double buildRoomDt = Utils.GetElapsedTime(last, DateTime.Now).Millisecond; // Debug
        last = DateTime.Now;

        // wait physics update (rework without physics ? like obb class)
        await Awaitable.WaitForSecondsAsync(Time.fixedDeltaTime, _cs.Token);
        if (_cs.IsCancellationRequested)
            return;
        
        FromGeneratedToValidate(_generatedRooms[0]); // Add first room 
        BuildCorridors();
        
        double buildCorridorDt = Utils.GetElapsedTime(last, DateTime.Now).Millisecond; // Debug
        last = DateTime.Now;
        
        RemoveNotConnectedRooms(); // Destroy room not connected
        ReplaceNotConnectedDoors(); // Replace useless doors by walls
        
        double cleanDt = Utils.GetElapsedTime(last, DateTime.Now).Millisecond;
        last = DateTime.Now;

        SelectStartAndEnd(); // Place start room and end room
        GenerateBiomes(); // Choose biome for each room
        
        double customRoomDt = Utils.GetElapsedTime(last, DateTime.Now).Millisecond; // Debug
        
        // Debug
        Debug.Log(
            "Generation ended (total time: " + (Utils.GetElapsedTime(begin, DateTime.Now).Millisecond) + "ms)" +
            "\n----- Detailed time data -----" +
            "\nReset : " + resetDt + "ms" +
            "\nGen : " + genRoomDt + "ms" +
            "\nBuildR : " + buildRoomDt + "ms" +
            "\nBuildC : " + buildCorridorDt + "ms" +
            "\nClean : " + cleanDt + "ms" +
            "\nCustomization : " + customRoomDt + "ms");
        Debug.Log("Rooms: " + _validatedRooms.Count + "/"+target);
        
        // check if all rooms have one door used
        foreach (SRPRoom r in _validatedRooms)
        {
            int doorUsed = 0;
            foreach (GameObject doorGo in r.doors)
            {
                IDoor door = doorGo.GetComponent<IDoor>();
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


            GameObject roomGo = Instantiate(roomStyle.rootPrefab, transform);
            SRPRoom nSrpRoom = roomGo.GetComponent<SRPRoom>();
            nSrpRoom.root.name += i;
            nSrpRoom.global = Instantiate(roomStyle.roomPrefab, nSrpRoom.root.transform);
            
            nSrpRoom.root.transform.position = pos;
            nSrpRoom.global.transform.localScale = scale;

            if (IsRoomValid(nSrpRoom) == false)
            {
                Utils.Destroy(nSrpRoom);
                continue;
            }
            
            _generatedRooms.Add(nSrpRoom);
        }
    }

    [Button]
    void BuildRooms()
    {
        BuildRoomsFloors();
        BuildRoomsRoofs();
        BuildRoomsWallsAndDoors();
    } 
    
    void BuildRoomsFloors()
    {
        foreach (SRPRoom r in _generatedRooms)
        {
            r.floor = Instantiate(roomStyle.floorPrefab, r.root.transform);
            Vector3 roomScale = new Vector3(r.global.transform.localScale.x, 1f, r.global.transform.localScale.z);
            Vector3 floorScaleFactor = r.floor.transform.localScale;
            Vector3 offset = new Vector3(0, -r.global.transform.localScale.y / 2, 0);
            r.floor.transform.localScale = new Vector3(
                roomScale.x * floorScaleFactor.x,
                roomScale.y * floorScaleFactor.y,
                roomScale.z * floorScaleFactor.z);
            r.floor.transform.localPosition = offset;
            r.floor.GetComponent<NavMeshSurface>().BuildNavMesh();
        }
    }    
    void BuildRoomsRoofs()
    {
        foreach (SRPRoom r in _generatedRooms)
        {
            r.floor = Instantiate(roomStyle.roofPrefab, r.root.transform);
            Vector3 roomScale = new Vector3(r.global.transform.localScale.x, 1f, r.global.transform.localScale.z);
            Vector3 roofScaleFactor = r.floor.transform.localScale;
            Vector3 offset = new Vector3(0, r.global.transform.localScale.y / 2, 0);
            r.floor.transform.localScale = new Vector3(
                roomScale.x * roofScaleFactor.x,
                roomScale.y * roofScaleFactor.y,
                roomScale.z * roofScaleFactor.z);
            r.floor.transform.localPosition = offset;
        }
    }
    
    void BuildRoomsWallsAndDoors()
    {
        foreach (SRPRoom r in _generatedRooms)
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
                    IDoor door = doorGo.GetComponent<IDoor>();
                    door.roomFrom = r;
                    door.Init();
                    float doorSizeX = doorSize.x;
                    float doorSizeY = doorSize.y;
                    Vector3 doorOffset = new Vector3(
                        wallOffset.x,
                        -wallScale.y/2f + doorSizeY/2f,
                        wallOffset.z);
                    doorGo.transform.localPosition = doorOffset;
                    doorGo.transform.localEulerAngles = wallRot;
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
                
            
            SRPRoom room = _generatedRooms[i];
            foreach (GameObject doorGo in room.doors)
            {
                IDoor door = doorGo.GetComponent<IDoor>();
                
                if(door.isUsed)
                    continue;
                
                IDoor closestDoor = FindClosestDoor(door);
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
                GameObject corridor = Instantiate(roomStyle.corridorPrefab, room.root.transform);
                corridor.transform.position = new Vector3(
                    (start.x + end.x) / 2f, 
                    (start.y + end.y) / 2f, 
                    (start.z + end.z) / 2f);
                corridor.transform.LookAt(end);
                corridor.transform.localScale = new Vector3(1, 1, scaleZ);
                room.corridors.Add(corridor);
                
                // Link doors <-> corridor edges
                BuildDoorLink(room, door, corridor);
                BuildDoorLink(room, closestDoor, corridor);

                door.otherDoor = closestDoor;
                closestDoor.otherDoor = door;

                FromGeneratedToValidate(room);
            }

            if (_validatedRooms.Contains(room))
                continue;
            
            // room cannot be connected, remove it to avoid useless computations in next checks
            _uselessRooms.Add(room);
            _generatedRooms.RemoveAt(i);
        }
    }

    [Button]
    void RemoveNotConnectedRooms()
    {
        Utils.DestroyObjectsInList(ref _uselessRooms);
    }
    [Button]
    void ReplaceNotConnectedDoors()
    {
        foreach (SRPRoom r in _validatedRooms)
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
                IDoor door = r.doors[i].GetComponent<IDoor>();
                if (door.isUsed)
                    continue;
                
                // OnDestroy of SRPDoor not called when edit mode
                if (Application.isPlaying == false)
                {
                    for (int j = door.doorWalls.Count - 1; j >= 0; j--)
                        Utils.Destroy(door.doorWalls[j]);
                }
                
                // delete useless door
                Utils.Destroy(r.doors[i]);
                r.doors.RemoveAt(i);

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
    void SelectStartAndEnd()
    {
        if (mapStyle.biomes.startBiome == null || mapStyle.biomes.endBiome == null)
        {
            Debug.LogWarning("Missing start room or end room in mapStyleSO", mapStyle);
            return;
        }
        
        List<SRPRoom> startCandidates = new();
        foreach (SRPRoom r in _validatedRooms)
        {
            if(r.doors.Count <= 1)
                continue;
            startCandidates.Add(r);
        }        
        List<SRPRoom> endCandidates = new();
        foreach (SRPRoom r in _validatedRooms)
        {
            if(r.doors.Count > 1)
                continue;
            endCandidates.Add(r);
        }
 
        if (startCandidates.Count == 0)
        {
            Debug.LogError("No room are valid to be the start room.");
            return;
        }        
        if (endCandidates.Count == 0)
        {
            Debug.LogError("No room are valid to be the end room.");
            return;
        }
        
        int startId = Random.Range(0, startCandidates.Count);
        int endId = Random.Range(0, endCandidates.Count);
        
        startRoom = startCandidates[startId];
        endRoom = endCandidates[endId];

        if (startRoom == endRoom)
        {
            Debug.LogError("Start and end room are the same.");
            return;
        }

        BiomeStyleSO startBiome = mapStyle.biomes.startBiome;
        BiomeStyleSO endBiome = mapStyle.biomes.endBiome;
        
        CreateBiomeBehavior(startBiome, startRoom)
            .transform.localPosition = Vector3.zero;
        CreateBiomeBehavior(endBiome, endRoom)
            .transform.localPosition = Vector3.zero;
    }
    
    [Button]
    void GenerateBiomes()
    {
        foreach (SRPRoom r in _validatedRooms)
        {
            if(r.biome >= 0)
                continue;
            
            float sum = 0f;
            foreach (var biomeData in mapStyle.biomes.biomeList)
            {
                if(biomeData.occurrences >= biomeData.maxOccurrences)
                    continue;
                sum += biomeData.prob;
            }
            float rdmBiome = Random.Range(0f, sum);
            
            // Select Biome
            foreach (var biomeData in mapStyle.biomes.biomeList)
            {
                if(biomeData.occurrences >= biomeData.maxOccurrences)
                    continue;
                
                rdmBiome -= biomeData.prob;
                if (rdmBiome > 0) continue;
                r.biome = biomeData.style.value;

                // Add Biome Behavior
                if(biomeData.style.biomeBehaviorPrefab == null) 
                    break;

                biomeData.occurrences++;
                CreateBiomeBehavior(biomeData.style, r);
                break;
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
        
        _generatedRooms ??= new List<SRPRoom>();
        _validatedRooms ??= new List<SRPRoom>();
        _uselessRooms ??= new List<SRPRoom>();

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

    void FromGeneratedToValidate(SRPRoom r)
    {
        if (FromTo(_generatedRooms, _validatedRooms, r))
            return;
        
        Debug.Log($"Room {r.root.name} has already been validated or removed from generated", r.global);
    }      
    void FromGeneratedToUseless(SRPRoom r)
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
    bool IsRoomValid(SRPRoom room)
    {
        // Check collision with _validatedRooms
        foreach (SRPRoom valid in _validatedRooms)
        {
            if (CollidingWith(room.global, valid.global))
                return false;
        }    
        // Check collision with _generatedRooms
        foreach (SRPRoom gen in _generatedRooms)
        {
            if (gen == room)
                continue;
            if (CollidingWith(room.global, gen.global))
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Do a Bounds.Intersects (Utils.CollidingWith) with GameObject scales and roomStyle.padding
    /// </summary>
    bool CollidingWith(GameObject go1, GameObject go2)
    {
        Bounds b1 = new Bounds(go1.transform.position, new Vector3(
            go1.transform.localScale.x + roomStyle.padding, 
            go1.transform.localScale.y + roomStyle.padding, 
            go1.transform.localScale.z + roomStyle.padding));        
        Bounds b2 = new Bounds(go2.transform.position, go2.transform.localScale);
        
        return Utils.CollidingWith(b1, b2);
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
    [CanBeNull] IDoor FindClosestDoor(IDoor door, List<IDoor> ignore = null)
    {
        bool ignoreListExist = (ignore == null) == false;
        float dist = float.MaxValue;
        IDoor closest = null;
        // get the corridor dimensions
        Vector3 corridorPrefabHalfSize = 
            Utils.GetWorldSize(roomStyle.corridorPrefab.GetComponent<Collider>()) 
            / 2f;
        
        // for each room, try to get the closest door of doorData
        foreach (SRPRoom otherRoom in _validatedRooms)
        {
            if(otherRoom == door.roomFrom)
                continue;
            
            // each door of the room
            foreach (GameObject otherDoorGo in otherRoom.doors)
            {
                IDoor otherDoor = otherDoorGo.GetComponent<IDoor>();
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
    static Vector3 GetDoorOutward(IDoor door)
    {
        Vector3 center = door.GetComponent<Collider>().bounds.center;
        Vector3 a = door.transform.forward;
        if (Vector3.Dot(a, door.corridorStart.transform.position - center) < 0f)
            a = -a;
        a.y = 0f;
        return a.normalized;
    }

    GameObject CreateBiomeBehavior(BiomeStyleSO style, SRPRoom room)
    {
        GameObject biomeGo = Instantiate(style.biomeBehaviorPrefab, room.root.transform);
        biomeGo.transform.localPosition = Vector3.zero;
        
        BiomeBehavior biome = biomeGo.GetComponent<BiomeBehavior>();
        biome.room = room;
        
        room.biome = style.value;
        
        return biomeGo;
    }
    
    /// <summary>
    /// Create a GameObject that links door (door.corridorStart) and main corridor
    /// The GameObject get a custom mesh of 4 quads (plans)
    /// </summary>
    void BuildDoorLink(SRPRoom room, IDoor door, GameObject corridor)
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
        List<Vector2> uvs = new();
        // Build 4 quads
        for (int i = 0; i < 4; i++)
        {
            int next = (i + 1) % 4;
            // i=0 : bot
            // i=1 : right
            // i=2 : top
            // i=3 : left
            // Build a quad
            Utils.AddQuad(vertices, triangles, uvs, room.root.transform,
                d[i], d[next], c[next], c[i],
                true);
        }

        // Set up the new Mesh
        Mesh linkMesh = new Mesh { name = "DoorLinkMesh" };
        linkMesh.SetVertices(vertices);
        linkMesh.SetTriangles(triangles, 0);
        linkMesh.SetUVs(0, uvs);
        
        linkMesh.RecalculateNormals();
        linkMesh.RecalculateBounds();
        // And the Go
        GameObject linkGo = new GameObject("DoorLink");
        linkGo.transform.SetParent(room.root.transform, false);
        linkGo.AddComponent<MeshFilter>().sharedMesh = linkMesh;
        
        // Set custom material on link
        MeshRenderer linkMeshR = linkGo.AddComponent<MeshRenderer>();
        linkMeshR.sharedMaterial = roomStyle.corridorLinkMat;
            
        // Mesh Collider
        MeshCollider linkMeshCol = linkGo.AddComponent<MeshCollider>();
        linkMeshCol.sharedMesh = linkMesh;
        linkMeshCol.convex = false; // corridor = open / not close --> not convex (im not sure if it's by default false)
        
        room.corridors.Add(linkGo);
    }
}