using UnityEditor;
using UnityEngine;

public class EndTask : IBiomeTask
{
    public override bool Do()
    {
        if (base.Do() == false)
            return false;
        
        Debug.Log("End reached");
        EditorApplication.isPaused = true;
        return true;
    }
}
