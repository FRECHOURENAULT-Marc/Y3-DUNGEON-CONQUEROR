using UnityEngine;

[CreateAssetMenu(fileName = "SOWeapon", menuName = "Scriptable Objects/SOWeapon")]
public class SOWeapon : ScriptableObject
{
    [Header("Description")]
    public string weaponName;

    [Header("Settings")]
    public float weaponDamage;
    public float weaponAtttackDistance;
    public float weaponSweepAngle;
    public float weaponAttackSpeed;

    [Header("Offset")]
    public Vector3 positionOffset;
    public Vector3 rotationOffset;
    public Vector3 scale;

    [Header("Prefab")]
    public GameObject weaponPrefabe;
}
