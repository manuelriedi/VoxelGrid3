using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using UnityStandardAssets.Characters.FirstPerson;

public class PauseMenuButtons : MonoBehaviour {

    public Button restartButton;
    public Button quitButton;
    public GameObject reticle;
    public FirstPersonController fpsController;

    private bool menuVisible;
    
    private void Start() {
        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitGame);
        
        restartButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
        reticle.SetActive(true);
        fpsController.enabled = true;
        menuVisible = false;
    }

    private void Update() {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            menuVisible = !menuVisible;
            restartButton.gameObject.SetActive(menuVisible);
            quitButton.gameObject.SetActive(menuVisible);
            reticle.SetActive(!menuVisible);
            fpsController.enabled = !menuVisible;
        } 
        // else if (Input.GetMouseButtonUp(0) && menuVisible) {
        //     
        // }
    }

    public void RestartGame() {
        Debug.Log("RestartGame");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame() {
        Debug.Log("QuitGame");
        Application.Quit();
    }
}
