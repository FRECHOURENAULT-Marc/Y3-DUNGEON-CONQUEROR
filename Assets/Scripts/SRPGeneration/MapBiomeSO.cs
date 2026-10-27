using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class BiomeData
{
    public string name;
    [Range(0f, 1f)]
    public float prob;
    public int value;
    public GameObject biomeBehaviorPrefab;
}


[CreateAssetMenu(fileName = "MapBiomeSO", menuName = "SO/Gen/BiomeSO")]
public class MapBiomeSO : ScriptableObject
{
    [Header("Biome list")]
    public List<BiomeData> biomeList;

    [Button]
    void Clear() => biomeList?.Clear();

    public int GetValue(string nameId)
    {
        BiomeData b = GetBiomeData(nameId);
        if(b == null)
            return int.MinValue;

        return b.value;
    }       
    public float GetProbability(string nameId)
    {
        BiomeData b = GetBiomeData(nameId);
        if(b == null)
            return 0f;

        return b.prob;
    }    
    public BiomeData GetBiomeData(string nameId)
    {
        foreach (var b in biomeList)
        {
            if(b.name == nameId)
                return b;
        }
        
        Debug.LogWarning("No biome found with name " +  nameId);
        return null;
    }
    
    private void Reset()
    {
        biomeList ??= new List<BiomeData>();
        if (biomeList.Count > 0)
            return;
        biomeList.Add(new() { name = "Default", value = 0, prob = 1f});
    }
}
