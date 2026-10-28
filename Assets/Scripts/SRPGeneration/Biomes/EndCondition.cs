using UnityEngine;

public class EndCondition : IBiomeCondition
{
    [SerializeField] public int distance;
    
    public override bool Test()
    {
        float d = Vector3.Distance(
            DependenciesInjector.PDeps.Player.transform.position,
            room.transform.position);
        return d < distance;
    }
}
