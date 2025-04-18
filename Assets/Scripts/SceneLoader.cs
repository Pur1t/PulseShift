using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void MusicSelected()
    {
        SceneManager.LoadScene("MusicSelectScene");
    }

    public void Settings()
    {
        Debug.Log("Show Settings Canvas (TODO)");
    }

    public void MusicEditSelected()
    {
        SceneManager.LoadScene("MusicSelectedEditScene");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit!");
    }

    public void StartMenu()
    {
        SceneManager.LoadScene("StartMenuScene");
    }

    public void Gameplay()
    {
        SceneManager.LoadScene("GameplayScene");
    }
}
