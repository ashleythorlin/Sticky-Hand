using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject UIObject;
    
    private bool isPaused;
    private bool escaped;

    public void Start(){
        Resume();
    }

    public void Update(){
        if(Input.GetKeyDown("escape")){
            Debug.Log("escaped");
            if(!isPaused){ 
                Pause();
            } else {
                Resume();
            }
        }
    }

    public void FixedUpdate(){

    }

    public void Pause(){
        Debug.Log("paused");
        isPaused = true;
        UIObject.SetActive(false);
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume(){
        Debug.Log("resumed");
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

    // public void Start(){
    //     Resume();
    // }

    // public void Update(){
    //     escaped = Input.GetKeyDown("escape");
    //     if(escaped){Debug.Log("escaped");}
    // }

    // public void FixedUpdate(){
    //     if(escaped){
    //         Debug.Log("escaped from FixedUpdate");
    //         if(!isPaused){ 
    //             Pause();
    //         } else {
    //             Resume();
    //         }
    //     }
    // }

    // public void Pause(){
    //     Debug.Log("paused");
    //     isPaused = true;
    //     UIObject.SetActive(false);
    //     pauseMenu.SetActive(true);
    //     Time.timeScale = 0f;
    // }

    // public void Resume(){
    //     Debug.Log("resumed");
    //     isPaused = false;
    //     UIObject.SetActive(true);
    //     pauseMenu.SetActive(false);
    //     Time.timeScale = 1f;
    // }

    // public void MainMenu(int sceneID){
    //     isPaused = false;
    //     Time.timeScale = 1f;
    //     SceneManager.LoadScene(sceneID);
    // }
}
