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
    
    // Properties
    public GameObject Player { get => _mainParent; } // "rename"

    public override void TryGettingFields(GameObject gameObject)
    {
        //Get other fields (if needed) in player hierarchy
    }
}


[Serializable]
public class SerializableGameObject {
    public string Name;
    public GameObject GameObject;
}

[Serializable]
public class ParticulesDependencies : Dependencies
{
    // Fields
    [SerializeField] List<SerializableGameObject> _partPrefabs;
    

    public override void TryGettingFields(GameObject gameObject) {}

    public GameObject GetPrefab(string name)
    {
        foreach(SerializableGameObject p in _partPrefabs)
            if (p.Name == name)
                return p.GameObject;
        return null;
    }
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
