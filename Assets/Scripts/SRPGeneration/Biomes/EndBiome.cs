using UnityEditor;
using UnityEngine;

public class EndBiome : ConditionalBiome
{
    public override void CompleteRoom()
    {
        base.CompleteRoom();
        Debug.Log("End reached");
        EditorApplication.isPaused = true;
    }
}
