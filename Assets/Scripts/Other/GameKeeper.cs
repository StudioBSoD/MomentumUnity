using UnityEngine;
using UnityEngine.SceneManagement;

public class GameKeeper : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene(1);
        Game.local.playing = true;
    }

    public void Quit()
    {
        Game.local.RestartOrQuit();
    }
}
