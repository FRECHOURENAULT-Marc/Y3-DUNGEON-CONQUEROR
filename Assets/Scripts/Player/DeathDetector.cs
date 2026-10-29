using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathDetector : MonoBehaviour
{
    public void PlayerDeath()
    {
        Leaderboard.AsWin = false;
        SceneManager.LoadScene("EndMenu");
    }
}
