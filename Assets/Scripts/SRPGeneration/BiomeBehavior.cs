using System.Collections.Generic;
using UnityEngine;

public class BiomeBehavior : MonoBehaviour
{
    [SerializeField] Color _biomeColor = Color.red;
    [SerializeField] List<ICondition> completionConditions;
    [SerializeField] List<ITask> onFinishedTasks;
    
    [HideInInspector] public SRPRoom room;
    
    void OnDrawGizmos()
    {
        if (room == null)
            return;
        Gizmos.color = _biomeColor;
        Gizmos.DrawWireCube(room.root.transform.position, room.global.transform.lossyScale);
    }

    void Start()
    {
        foreach (var condition in completionConditions)
        {
            if (condition is not IBiomeCondition)
                return;
            ((IBiomeCondition)condition).room = room;
        }        
        foreach (var task in onFinishedTasks)
        {
            if (task is not IBiomeTask)
                return;
            ((IBiomeTask)task).room = room;
        }
    }
    
    void FixedUpdate()
    {
        if(room.IsFinished)
            return;

        if (completionConditions.Count == 0)
            return;
        
        foreach (var condition in completionConditions)
        {
            if (condition.Test() == false)
                return;
        }
        CompleteRoom();
    }
    
    public virtual bool IsRoomCompleted() // override if needed
    {
        foreach (var condition in completionConditions)
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

        foreach (var task in onFinishedTasks)
            task.Do();
    }
}
