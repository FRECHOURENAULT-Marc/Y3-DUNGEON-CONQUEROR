using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

public class ObjectBrain : MonoBehaviour
{
    [SerializeField] ObjectFloorReferences _OReferences;

    [Header("Settings")]
    [SerializeField] SOWeapon _weapon;
    [SerializeField] GameObject _curModel;
    [SerializeField] bool _isWeaponGet;

    void Start()
    {
        _isWeaponGet = false;
    }


    public SOWeapon Weapon 
    { 
        get => _weapon; 
        set { if ( _weapon == null ) _weapon = value; }
    }

    public bool IsWeaponGet { get => _isWeaponGet; set => _isWeaponGet = value; }


    void Update()
    {
        if ( _isWeaponGet ) RemovePrefab();

    }

    void RemovePrefab()
    {
        ResetGen();
        DestroyImmediate(_OReferences.ObjectFloorRoot);
    }



    [ContextMenu("Gen")]
    public void GenerateWeapon()
    {
        _curModel = Instantiate(_weapon.weaponPrefabe);
        _curModel.transform.position = _OReferences.ObjectFloorModel.transform.position + new Vector3(0, 1, 0);

        ParentConstraint constraint = _curModel.AddComponent<ParentConstraint>();

        ConstraintSource origin = new ConstraintSource();
        origin.sourceTransform = _OReferences.ObjectFloorModel.transform;
        origin.weight = 1;
        constraint.AddSource(origin);

        // Active constraint
        constraint.constraintActive = true;
    }

    [ContextMenu("Reset")]
    void ResetGen()
    {
        _weapon = null;
        DestroyImmediate(_curModel);
    }
}
