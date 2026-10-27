using UnityEngine;
using UnityEngine.InputSystem;

public class DefaultBiome : IBiomeBehavior
{
    [SerializeField] GameObject _centerCubePrefab;
    [SerializeField] InputActionReference _inputForFinishing;
    
    void Start()
    {
        Instantiate(_centerCubePrefab, transform);
        
        _inputForFinishing.action.started += (cb) => CompleteRoom();
    }
}
