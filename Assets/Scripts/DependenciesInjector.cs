using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public abstract class Dependencies
{
    [SerializeField] protected GameObject _mainParent;
    
    public void TryGetComponent<C>(GameObject go, ref C component) where C : Component
    {
        if (component != null) return;
        
        go.TryGetComponent(out component);
        if (component == null)
            component = go.GetComponentInChildren<C>();
    }
    
    public void TryGettingFieldsFromParent() => TryGettingFields(_mainParent);

    public abstract void TryGettingFields(GameObject gameObject);
}

[Serializable] public class PlayerDependencies : Dependencies
{
    // Fields
    [SerializeField] Collider _pCollider = null;
    [SerializeField] List<SOWeapon> _pWeaponList;
    [SerializeField] WEAPON_SELECTED _pWeapon = 0;
    
    // Properties
    public GameObject Player { get => _mainParent; } // "rename"
    public Collider PCollider { get => _pCollider; }

    public SOWeapon PWeapon { get => _pWeaponList[(int)_pWeapon]; }

    public override void TryGettingFields(GameObject gameObject)
    {
        //Get other fields (if needed) in player hierarchy
        _pCollider = Player.transform.GetChild(2).GetComponentInChildren<Collider>();
    }

    public void SetWeapon(WEAPON_SELECTED w) => _pWeapon = w;
}



public class DependenciesInjector : MonoBehaviour
{
    [SerializeField] PlayerDependencies _playerDeps;
    public static PlayerDependencies PDeps;    
    [Button("Get Player Dependencies")]
    void TryGettingFieldsP() => _playerDeps.TryGettingFieldsFromParent();
    
    private void Awake()
    {
        PDeps = _playerDeps;        
    }
}
