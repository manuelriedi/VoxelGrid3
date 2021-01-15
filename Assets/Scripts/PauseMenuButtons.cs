using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using UnityStandardAssets.Characters.FirstPerson;

public class PauseMenuButtons : MonoBehaviour {

    public GameObject pausePanel;
    public Button restartButton;
    public Button quitButton;
    public GameObject reticle;
    public FirstPersonController fpsController;

    private bool menuVisible;
    
    private void Start() {
        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitGame);
        
        pausePanel.SetActive(false);
        reticle.SetActive(true);
        fpsController.enabled = true;
        menuVisible = false;
    }

    private void Update() {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            menuVisible = !menuVisible;
            pausePanel.SetActive(menuVisible);
            reticle.SetActive(!menuVisible);
            fpsController.enabled = !menuVisible;
        }
    }

    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame() {
        Application.Quit();
    }
}
