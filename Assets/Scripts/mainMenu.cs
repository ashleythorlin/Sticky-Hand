using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class mainMenu : MonoBehaviour
{
    public string mainMenuScene;
    public string settingsMenu;
    public string firstLevel;


    // // Start is called before the first frame update
    // void Start()
    // {
    // }

    // // Update is called once per frame
    // void Update()
    // {
        
    // }

    public void StartGame()
    {
        SceneManager.LoadScene(firstLevel);
    }

    public void OpenSettings() 
    {
        SceneManager.LoadScene(settingsMenu);
    }

    public void CloseSettings()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting");
    }
}
