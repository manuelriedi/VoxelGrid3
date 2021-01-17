using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using UnityStandardAssets.Characters.FirstPerson;

public class GameOver : MonoBehaviour
{

    public GameObject panel;
    public Button restartButton;
    public Button quitButton;
    public GameObject reticle;
    public FirstPersonController fpsController;

    private bool menuVisible;

    private void Start()
    {
        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitGame);

        panel.SetActive(false);
        reticle.SetActive(true);
        fpsController.enabled = true;
        menuVisible = false;
    }

    public void GameIsOver()
    {

        menuVisible = true;
        panel.SetActive(menuVisible);
        reticle.SetActive(!menuVisible);
        fpsController.enabled = !menuVisible;
 
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}