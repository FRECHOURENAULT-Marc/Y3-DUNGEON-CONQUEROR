using UnityEngine;

[CreateAssetMenu(fileName = "SOCapacity", menuName = "Scriptable Objects/SOCapacity")]
public class SOCapacity : ScriptableObject
{
    [SerializeField] public string CapacityName;
    [SerializeField] public string description;

}
