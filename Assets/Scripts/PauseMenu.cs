using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject UIObject;
    
    private bool isPaused;

    public void Start(){
        Resume();
    }

    public void Update(){
        if(Input.GetKeyDown("escape")){
            Debug.Log("escaped");
            if(!isPaused){ 
                Debug.Log("paused");
                Pause();
            } else {
                Debug.Log("resumed");
                Resume();
            }
        }
    }

    public void FixedUpdate(){

    }

    public void Pause(){
        isPaused = true;
        UIObject.SetActive(false);
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume(){
        isPaused = false;
        UIObject.SetActive(true);
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void MainMenu(int sceneID){
        isPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneID);
    }
}
