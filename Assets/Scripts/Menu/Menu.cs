using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Menu : MonoBehaviour
{
   public void SceneMain()
    {
        SceneManager.LoadScene("StageA1");
    }

    public void SceneCredits()
    {
        SceneManager.LoadScene("Credits");
    }

    public void SceneMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void SceneExit()
    {
        Environment.Exit(0);
    }
}
