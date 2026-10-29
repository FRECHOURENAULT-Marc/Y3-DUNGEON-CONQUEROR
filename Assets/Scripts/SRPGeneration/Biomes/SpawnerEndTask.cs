using UnityEngine;

public class SpawnerEndTask : IBiomeTask
{
    public override bool Do()
    {
        if (base.Do() == false)
            return false;

        DependenciesInjector.PDeps.Player.GetComponentInChildren<HealthController>().Heal(int.MaxValue);
        
        Debug.Log("Spawner cleared");
        return true;
    }
}
