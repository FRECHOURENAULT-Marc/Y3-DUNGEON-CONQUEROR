using System.Collections.Generic;
using UnityEngine;

public class IBiomeBehavior : MonoBehaviour
{
    [SerializeField] Color _biomeColor = Color.red;
    [SerializeField] List<ICondition> conditions;
    
    [HideInInspector] public SRPRoom room;
    
    void OnDrawGizmos()
    {
        if (room == null)
            return;
        Gizmos.color = _biomeColor;
        Gizmos.DrawWireCube(room.root.transform.position, room.global.transform.lossyScale);
    }
    
    void FixedUpdate()
    {
        if(room.IsFinished)
            return;
        
        foreach (var condition in conditions)
        {
            if (condition.Test() == false)
                return;
        }
        CompleteRoom();
    }
    
    public virtual bool IsRoomCompleted() // override if needed
    {
        foreach (var condition in conditions)
        {
            if (condition.Test() == false)
                return false;
        }
        return true;
    }
    
    /// <summary>
    /// Call method "OpenDoor" of all doors of the room
    /// </summary>
    public virtual void CompleteRoom() // override if needed
    {
        room.Finish();
    }
}
