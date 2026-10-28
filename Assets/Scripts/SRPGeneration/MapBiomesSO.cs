using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public class BiomeData
{
    public BiomeStyleSO style;
    public float prob;
    public int maxOccurrences;
    [HideInInspector] public int occurrences;
}

[CreateAssetMenu(fileName = "MapBiomesSO", menuName = "SO/Gen/MapBiomesSO")]
public class MapBiomesSO : ScriptableObject
{
    [Header("Biome list")]
    public BiomeStyleSO startBiome;
    public BiomeStyleSO endBiome;
    public List<BiomeData> biomeList;

    [Button]
    void Clear() => biomeList?.Clear();

    public int GetValue(string nameId)
    {
        BiomeData b = GetBiomeData(nameId);
        if(b == null)
            return int.MinValue;

        return b.style.value;
    }       
    public float GetProbability(string nameId)
    {
        BiomeData b = GetBiomeData(nameId);
        if(b == null)
            return 0f;

        return b.prob;
    }    
    public BiomeStyleSO GetBiomeStyle(string nameId)
    {
        BiomeData b = GetBiomeData(nameId);
        if(b == null)
            return null;

        return b.style;
    }    
    public BiomeData GetBiomeData(string nameId)
    {
        foreach (var b in biomeList)
        {
            if(b.style.biomeName == nameId)
                return b;
        }
        
        Debug.LogWarning("No biome found with name " +  nameId);
        return null;
    }
    
    private void Reset()
    {
        biomeList ??= new List<BiomeData>();
    }
}
