using System.Collections.Generic;
using System.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;

public class EnemySpawnerCondition : IBiomeCondition
{
    [SerializeField] EnemySpawnerTask spawnerTask;
    [ShowNonSerializedField] int _lastIndex;

    public override void Reset()
    {
        base.Reset();
        _lastIndex = 0;
    }
    
    public override bool Test()
    {
        if(base.Test() == false)
            return false;
        
        Collider pCollider = DependenciesInjector.PDeps.PCollider;
        Collider roomCollider = room.global.GetComponent<Collider>();
        bool isPlayerInRoom = Utils.CollidingWith(pCollider, roomCollider);
        
        if(isPlayerInRoom == false)
            return false;
        if (_lastIndex > spawnerTask.LastWaveIndex)
            return false;
        if (spawnerTask.IsWaveCompleted == false)
            return false;
        
        spawnerTask.waveIndexToPlay = _lastIndex;
        _lastIndex++;
        return true;
    }
}
