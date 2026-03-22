using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    public void Exit()
    {
        Application.Quit();
    }

    public void EnterGame()
    {
        LoadSceneMode mode = LoadSceneMode.Single;
        SceneManager.LoadScene("00");
    }
}
