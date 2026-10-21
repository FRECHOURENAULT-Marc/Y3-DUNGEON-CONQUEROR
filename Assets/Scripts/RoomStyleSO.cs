using UnityEngine;

[CreateAssetMenu(fileName = "RoomStyleSO", menuName = "SO/Gen/RoomSO")]
public class RoomStyleSO : ScriptableObject
{
    [Header("Width (x)")]
    [Range(0, 100)] public int minWidth;
    [Range(0, 100)] public int maxWidth;
    [Header("Depth (z)")]
    [Range(0, 100)] public int minDepth;
    [Range(0, 100)] public int maxDepth;
    [Header("Height (y)")]
    [Range(0, 100)] public int minHeight;
    [Range(0, 100)] public int maxHeight;
    
    [Header("Padding (spacing between rooms)")]
    [Range(0, 10)] public int padding;    
    
    [Header("Connexion with other rooms")]
    [Range(0, 4)] public int maxConnexion;
    [Range(0, 4)] public int minConnexion;
    
    [Header("Prefabs")]
    public GameObject rootPrefab;
    public GameObject roomPrefab;
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject doorPrefab;
    
    public GameObject corridorPrefab;
    public GameObject stairStartPrefab;
    public GameObject stairMiddlePrefab;
    public GameObject stairEndPrefab;
    
    private void Reset()
    {
        padding = 1;
        maxConnexion = 3;
        minConnexion = 2;
        
        minWidth = 8;
        maxWidth = 32; 
        minDepth = 8;
        maxDepth = 32;
        minHeight = 4;
        maxHeight = 16;
    }
}
