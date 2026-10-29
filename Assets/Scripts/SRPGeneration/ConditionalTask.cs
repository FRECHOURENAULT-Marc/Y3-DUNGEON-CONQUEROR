using System.Collections.Generic;
using UnityEngine;

public class ICondition : MonoBehaviour
{
    public virtual void Reset() { }
    public virtual bool Test() => true;
}
public class IBiomeCondition : ICondition
{
    [HideInInspector] public SRPRoom room;
    public override bool Test() => true;
}

public class ITask : MonoBehaviour
{
    [SerializeField] protected bool _doMultipleTimes = false;
    protected bool _done;
    
    public bool IsDone { get => _done; }
    public bool IsRepeatable { get => _doMultipleTimes; }

    public virtual void Reset()
    {
        _done = false;
    }
    
    public virtual bool Do()
    {
        if(_doMultipleTimes == false && _done)
            return false;
        return true;
    }
}
public class IBiomeTask : ITask
{
    [HideInInspector] public SRPRoom room;
    public override bool Do() => base.Do();
}

//[CreateAssetMenu(fileName = "ConditionalTaskSO", menuName = "SO/ConditionalTask")]
public class ConditionalTask : MonoBehaviour
{
    public List<ICondition> _conditions;
    public List<ITask> _tasks;

    bool isFinished = false;

    public bool Try()
    {
        if (TestAll() == false)
            return false;
        
        DoAll();
        return true;
    }
    public bool TestAll()
    {
        if (isFinished)
            return false;
        
        foreach (var condition in _conditions)
        {
            if (condition.Test() == false)
                return false;
        }

        return true;
    }
    public void DoAll()
    {
        isFinished = true;
        foreach (var task in _tasks)
        {
            task.Do();
            if(task.IsDone == false || task.IsRepeatable)
                isFinished = false;
        }
    }
}
