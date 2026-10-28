using UnityEngine;

[CreateAssetMenu(fileName = "BiomeStyleSO", menuName = "SO/Gen/BiomeStyleSO")]
public class BiomeStyleSO : ScriptableObject
{
    public string biomeName;
    public int value;
    public GameObject biomeBehaviorPrefab;
}
