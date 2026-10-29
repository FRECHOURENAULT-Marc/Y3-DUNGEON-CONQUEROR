using NUnit.Framework.Internal;
using UnityEngine;

public class SpawnerEndContidion : IBiomeCondition
{
    [SerializeField] EnemySpawnerTask spawner;
    
    public override bool Test()
    {
        if(base.Test() == false)
            return false;
        
        return spawner.IsDone;
    }
}
