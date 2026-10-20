
using System;
using System.Collections.Generic;
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
    [SerializeField] int tries;

    [Header("Seed")] 
    [SerializeField] int seed;
    [SerializeField] bool randomSeed;
    
    List<GameObject> _finalRooms;
    List<GameObject> _validatedRooms;
    List<GameObject> _floors;
    List<GameObject> _walls;
    List<GameObject> _doors;
    List<GameObject> _corridors;

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
        foreach (GameObject go in _corridors)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(go.transform.position, go.transform.forward * 20);
        }
    }

    // Generation

    [Button]
    public void GenerateAllSteps()
    {
        ResetAll();
        GenerateRooms();
        BuildRoomsFloors();
        BuildRoomsWallsAndDoors();
        BuildDoors();
        
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
            
            GameObject room = Instantiate(roomStyle.roomPrefab, transform);
            room.transform.position = pos;
            room.transform.localScale = scale;

            if (IsRoomValid(room) == false)
            {
                DestroyImmediate(room);
                continue;
            }
            
            _validatedRooms.Add(room);
        }
    }

    [Button]
    void BuildRoomsFloors()
    {
        foreach (GameObject go in _validatedRooms)
        {
            Vector3 offset = new Vector3(0, -go.transform.localScale.y / 2, 0);
            GameObject floor = Instantiate(roomStyle.floorPrefab, go.transform);
            floor.transform.position = go.transform.position + offset;
            _floors.Add(floor);
        }
    }
    
    [Button]
    void BuildRoomsWallsAndDoors()
    {
        foreach (var room in _validatedRooms)
        {
            Vector3 scale = new Vector3(1 / room.transform.localScale.x, 1, 1);
            List<Vector3> offsets = new()
            {
                new Vector3(0, 0, room.transform.localScale.z/2),
                new Vector3(0, 0, -room.transform.localScale.z/2),
                new Vector3(room.transform.localScale.x/2, 0, 0),
                new Vector3(-room.transform.localScale.x/2, 0, 0),
            };            
            List<Vector3> rotations = new()
            {
                new Vector3(0, 90, 0),
                new Vector3(0, -90, 0),
                new Vector3(0, 180, 0),
                new Vector3(0, -180, 0),
            };

            // refaire
            List<int> allDoorIndex = new() { 0, 1, 2, 3 };
            List<int> rdmDoorIndex = new() { Random.Range(0, allDoorIndex.Count) };
            allDoorIndex.Remove(rdmDoorIndex[0]);
            rdmDoorIndex.Add(Random.Range(0, allDoorIndex.Count)); 
            //---
            
            for (int j = 0; j < rotations.Count; j++)
            {
                Quaternion rotation = Quaternion.Euler(rotations[j]);
                Vector3 offset = offsets[j];
                float sideLength = offset.magnitude * 2;
                
                if(rdmDoorIndex.Contains(j))
                {
                    // Create door
                    GameObject door = Instantiate(roomStyle.doorPrefab, room.transform);
                    door.transform.localScale = new Vector3(
                        1 / room.transform.localScale.x, 
                        1 / room.transform.localScale.y, 
                        1 / room.transform.localScale.z);
                    
                    float doorWidth = door.GetComponent<Collider>().bounds.size.x;
                    doorWidth /= room.transform.lossyScale.x;
                    
                    float segmentLength = (sideLength - doorWidth) / 2f;
                    
                    if (segmentLength <= 0f)
                    {
                        Debug.LogWarning("Door too large for this wall ("+sideLength+" - "+doorWidth+" /2 = "+segmentLength+" )");
                        DestroyImmediate(door);
                        return;
                    }
                    
                    float distance = doorWidth / 2f + segmentLength / 2f;
                    Debug.Log("Distance: "+distance);
                    
                    foreach (float direction in new float[] { -1f, 1f })
                    {
                        GameObject halfWall = Instantiate(roomStyle.wallPrefab, room.transform);

                        halfWall.transform.localRotation = rotation;

                        // Le mur est supposé avoir une longueur sur son axe local Z.
                        halfWall.transform.localScale = new Vector3(
                            1 / room.transform.localScale.x,
                            1,
                            1 / room.transform.localScale.z * segmentLength
                        );

                        Vector3 localOffset = offset;

                        // Pour les côtés horizontaux (Z), décaler sur Z.
                        // Pour les côtés verticaux (X), décaler sur X.
                        if (Mathf.Abs(offset.z) > 0f)
                        {
                            localOffset.x += direction * distance;
                        }
                        else
                        {
                            localOffset.z += direction * distance;
                        }

                        localOffset.x /= room.transform.lossyScale.z;
                        localOffset.z /= room.transform.lossyScale.x;
                        Debug.Log(localOffset);
                        
                        halfWall.transform.localPosition = localOffset;

                        _walls.Add(halfWall);
                    }
                    
                    // Debug.Log("Door size: " + doorWidth);
                    // Debug.Log("Door local: " + door.transform.localScale);
                    // Debug.Log("Door lossy: " + door.transform.lossyScale);
                    //
                    // bool xAxis = offset.x != 0;
                    // bool zAxis = offset.z != 0;
                    // _doors.Add(door);
                    //
                    // // create left wall (with door)
                    // GameObject wallLeft = Instantiate(roomStyle.wallPrefab, room.transform);
                    // wallLeft.transform.rotation = rotation;
                    // wallLeft.transform.localScale = new Vector3(
                    //     scale.x, 
                    //     scale.y, 
                    //     scale.z/2); // set scale 
                    //
                    // Vector3 lOffset = Vector3.zero;
                    // if (xAxis)
                    // {
                    //     float wallLScaleZ = wallLeft.GetComponent<Collider>().bounds.size.z / 2;
                    //     lOffset = new Vector3(0, 0, -wallLScaleZ/2 - doorWidth/2);
                    // }                    
                    // if (zAxis)
                    // {
                    //     float wallLScaleX = wallLeft.GetComponent<Collider>().bounds.size.x / 2;
                    //     lOffset = new Vector3(-wallLScaleX/2 - doorWidth/2, 0, 0);
                    // }
                    // wallLeft.transform.position = room.transform.position + offset + lOffset;
                    //
                    // // create right wall (with door)  
                    // GameObject wallRight = Instantiate(roomStyle.wallPrefab, room.transform);
                    // wallRight.transform.rotation = rotation;
                    // wallRight.transform.localScale = new Vector3(
                    //     scale.x, 
                    //     scale.y, 
                    //     scale.z/2);
                    //
                    // Vector3 rOffset = Vector3.zero;
                    // if (xAxis)
                    // {
                    //     float wallLScaleZ = wallLeft.GetComponent<Collider>().bounds.size.z / 2;
                    //     rOffset = new Vector3(0, 0, wallLScaleZ/2 - doorWidth/2);
                    // }                    
                    // if (zAxis)
                    // {
                    //     float wallLScaleX = wallLeft.GetComponent<Collider>().bounds.size.x / 2;
                    //     rOffset = new Vector3(wallLScaleX/2 - doorWidth/2, 0, 0);
                    // }
                    // wallRight.transform.position = room.transform.position + offset + rOffset;
                    //
                    // _walls.Add(wallLeft);
                    // _walls.Add(wallRight);
                    continue;
                }
                
                GameObject wall = Instantiate(roomStyle.wallPrefab, room.transform);
                wall.transform.rotation = rotation;
                wall.transform.position = room.transform.position + offset;
                wall.transform.localScale = scale;
                _walls.Add(wall);
            }
        }
    }

    [Button]
    void BuildDoors()
    {
        // for (int i = 0; i < _walls.Count; i++)
        // {
        //     GameObject room = _validatedRooms[i];
        //     GameObject floor = _floors[i];
        //     List<Vector3> offset = new()
        //     {
        //         new Vector3(room.transform.localScale.x / 2, 0, 0),
        //         new Vector3(-room.transform.localScale.x / 2, 0, 0),
        //         new Vector3(0, 0, room.transform.localScale.z / 2),
        //         new Vector3(0, 0, -room.transform.localScale.z / 2),
        //     };
        //     List<Vector3> rotation = new()
        //     {
        //         new Vector3(0, 90, 0),
        //         new Vector3(0, -90, 0),
        //         new Vector3(0, 180, 0),
        //         new Vector3(0, -180, 0),
        //     };
        //
        //     int rdmIndex = Random.Range(0, 4);
        //     GameObject door = Instantiate(roomStyle.doorPrefab, room.transform);
        //     
        //     door.transform.localScale = new Vector3(
        //         1/room.transform.localScale.x, 
        //         1/room.transform.localScale.y, 
        //         1/room.transform.localScale.z);
        //     door.transform.position = room.transform.position + offset[rdmIndex];
        //     door.transform.rotation = Quaternion.Euler(room.transform.rotation.eulerAngles + rotation[rdmIndex]);
        //     
        //     _doors.Add(door);
        // }
    }

    [Button]
    void BuildCorridors()
    {
        for (int i = _validatedRooms.Count - 1; i >= 0; i--)
        {
            GameObject go = _validatedRooms[i];
            
            _finalRooms.Add(go);
            _validatedRooms.RemoveAt(i);
            
            GameObject closest = FindClosestRoom(go);
            if (closest == null)
                continue;
            
            Vector3 corridorPos = new Vector3(
                (go.transform.position.x + closest.transform.position.x) / 2, 
                (go.transform.position.y + closest.transform.position.y) / 2, 
                (go.transform.position.z + closest.transform.position.z) / 2);
            GameObject corridor = Instantiate(roomStyle.corridorPrefab, transform);
            corridor.transform.position = corridorPos;
            corridor.transform.LookAt(go.transform.position);
            corridor.transform.localScale = new Vector3(1, 1, Vector3.Distance(go.transform.position, closest.transform.position));
            
            _corridors.Add(corridor);
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
 
        _finalRooms ??= new List<GameObject>();
        _validatedRooms ??= new List<GameObject>();
        _floors ??= new List<GameObject>();
        _walls ??= new List<GameObject>();
        _doors ??= new List<GameObject>();
        _corridors ??= new List<GameObject>();
    }

    void DestroyObjectsInList(ref List<GameObject> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
            DestroyImmediate(list[i]);
        list.Clear();
    }
    
    void Clear()
    {
        DestroyObjectsInList(ref _finalRooms);
        DestroyObjectsInList(ref _validatedRooms);
        DestroyObjectsInList(ref _floors);
        DestroyObjectsInList(ref _walls);
        DestroyObjectsInList(ref _doors);
        DestroyObjectsInList(ref _corridors);
        
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }
    
    bool IsRoomValid(GameObject go)
    {
        bool isValid = true;
        foreach (GameObject valid in _validatedRooms)
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

    GameObject FindClosestRoom(GameObject go)
    {
        float dist = float.MaxValue;
        GameObject closest = null;
        foreach (var other in _validatedRooms)
        {
            if(other == go)
                continue;
            
            float d = Vector3.Distance(go.transform.position, other.transform.position);
            if (d > dist)
                continue;
            dist = d;
            closest = other;
        }
        
        if(closest == null)
            Debug.LogWarning("No closest room found");
        
        return closest;
    }
}
