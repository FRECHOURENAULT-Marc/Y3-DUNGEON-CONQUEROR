using UnityEngine;

[CreateAssetMenu(fileName = "MapStyleSO", menuName = "SO/Gen/MapSO")]
public class MapStyleSO : ScriptableObject
{
    [Header("Biomes")]
    public MapBiomeSO biome;    
    
    [Header("Start")]
    public GameObject startBehavior;    
    [Header("End")]
    public GameObject endBehavior;
    
    [Header("Size")]
    [Range(0, 1000)] public int width;
    [Range(0, 1000)] public int depth;
    [Range(0, 1000)] public int height;

    void Reset()
    {
        width = 500;
        depth = 500;
        height = 50;
    }
}
