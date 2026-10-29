using UnityEngine;
using UnityEngine.InputSystem;
using Deps = DependenciesInjector;


public class InputBiome : BiomeBehavior
{
    [SerializeField] InputActionReference _inputForFinishing;
    [SerializeField] float _maxDistanceActivation;
    
    void Reset()
    {
        _maxDistanceActivation = 20f;
    }
    
    void Start()
    {
        _inputForFinishing.action.started += (cb) =>
        {
            if (IsRoomCompleted() == false)
                return;
                
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
        float d = Vector3.Distance(Deps.PDeps.Player.transform.position, room.transform.position);
        return d < _maxDistanceActivation;
    }
}
