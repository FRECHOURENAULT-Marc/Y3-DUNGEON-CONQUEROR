using UnityEngine;
using UnityEngine.InputSystem;
using Deps = DependenciesInjector;

public class DefaultBiome : IBiomeBehavior
{
    [SerializeField] GameObject _centerCubePrefab;
    [SerializeField] InputActionReference _inputForFinishing;
    [SerializeField] float _maxDistanceActivation;
    
    void Reset()
    {
        _maxDistanceActivation = 20f;
    }
    
    void Start()
    {
        Instantiate(_centerCubePrefab, transform);
        
        _inputForFinishing.action.started += (cb) =>
        {
            if (IsRoomCompleted() == false)
            {
                Debug.Log("Condition not completed : " + Vector3.Distance(Deps.PDeps.Player.transform.position, roomRoot.transform.position) + "/" + _maxDistanceActivation);
                return;
            }
                
            Debug.Log("Room completed !");
            CompleteRoom();
        };
    }

    /// <summary>
    /// Override method of DefaultBiome
    /// </summary>
    /// <returns>Always true</returns>
    public override bool IsRoomCompleted()
    {
        float d = Vector3.Distance(Deps.PDeps.Player.transform.position, roomRoot.transform.position);
        return d < _maxDistanceActivation;
    }
}
