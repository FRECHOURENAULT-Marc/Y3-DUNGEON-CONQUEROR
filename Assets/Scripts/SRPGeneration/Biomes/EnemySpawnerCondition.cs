using System.Collections.Generic;
using System.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;

public class EnemySpawnerCondition : IBiomeCondition
{
    [SerializeField] EnemySpawner spawner;
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
            Debug.Log("Player not in room");  
        if (isPlayerInRoom == false)
            return false;
        if(_lastIndex > spawner.LastWaveIndex)
            Debug.Log("index was too height : " + _lastIndex + "/" + spawner.LastWaveIndex); 
        if (_lastIndex > spawner.LastWaveIndex)
            return false;
        if(spawner.IsWaveCompleted == false)
            Debug.Log("Previous wave in not completed");
        if (spawner.IsWaveCompleted == false)
            return false;

        Debug.Log("Incrementing wave index");
        spawner.waveIndexToPlay = _lastIndex;
        _lastIndex++;
        return true;
    }
}
