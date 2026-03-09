using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnStartClick()
    {
        SceneManager.LoadScene("mainGame");
    }

    public void OnRestartClick()
    {
        GameStateManager.GameState = GameState.Intro;
        Application.LoadLevel(Application.loadedLevelName);
    }

    public void OnExitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
