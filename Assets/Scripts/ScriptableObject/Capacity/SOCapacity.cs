using UnityEngine;

[CreateAssetMenu(fileName = "SOCapacity", menuName = "Scriptable Objects/SOCapacity")]
public class SOCapacity : ScriptableObject
{
    public string CapacityName;
    public string description;

    public Sprite icon;
    public Sprite iconDisable;

}
