using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndTask : IBiomeTask
{
    public override bool Do()
    {
        if (base.Do() == false)
            return false;
        
        Debug.Log("End reached");
        //EditorApplication.isPaused = true;
        Leaderboard.AsWin = true;
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        SceneManager.LoadScene("EndMenu");
        return true;
    }
}
