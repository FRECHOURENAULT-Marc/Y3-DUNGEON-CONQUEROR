using System.Collections.Generic;
using UnityEngine;

public class ICondition : MonoBehaviour
{
    public virtual bool Test() => true;
}
public class IBiomeCondition : ICondition
{
    public SRPRoom room;
    public override bool Test() => true;
}

public class ITask : MonoBehaviour
{
    public readonly bool doMultipleTimes = false;
    public bool done = false;

    public virtual bool Do()
    {
        if(doMultipleTimes == false && done)
            return false;
        return true;
    }
}
public class IBiomeTask : ITask
{
    public SRPRoom room;
    public override bool Do() => base.Do();
}

[CreateAssetMenu(fileName = "ConditionalTaskSO", menuName = "SO/ConditionalTask")]
public class ConditionalTaskSO : ScriptableObject
{
    [SerializeField] List<ICondition> _conditions;
    [SerializeField] List<ITask> _tasks;

    bool iFinished = false;

    public bool Try()
    {
        if (TestAll() == false)
            return false;

        DoAll();
        return true;
    }
    public bool TestAll()
    {
        if (iFinished)
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
        foreach (var task in _tasks)
        {
            task.Do();
            iFinished = task.done && (task.doMultipleTimes == false);
        }
    }
}
