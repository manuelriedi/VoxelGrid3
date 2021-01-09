using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuButtons : MonoBehaviour {
    public void RestartGame() {
        Debug.Log("RestartGame");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame() {
        Debug.Log("QuitGame");
        Application.Quit();
    }
}
