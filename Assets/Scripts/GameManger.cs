using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
   
    public GameObject endGameScreen;
    public string CurrentLevel;
    public bool levelComplete; 
  
    
    void Start()
    {
        endGameScreen.SetActive(false);
        levelComplete = false;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Level 1");
        Time.timeScale = 1;
    }

    public void LoadHowToPlay()
    {
        SceneManager.LoadScene("HowToPlay");
        Time.timeScale = 1;
    }
    public void LoadMainScreen()
    {
        SceneManager.LoadScene("MainScreen");
    }
    
    public void GameOver()
    {
        endGameScreen.SetActive(true); // Show the end game screen
        Time.timeScale = 0;
    }

    public void RestartGame()
    {
        // Reload the current level and reset time scale
        Time.timeScale = 1;
        CurrentLevel = SceneManager.GetActiveScene().name;
        if(CurrentLevel == "HowToPlay")
        {
            SceneManager.LoadScene("HowToPlay");
        }
        else
        {
            SceneManager.LoadScene("Level 1");
        }
        
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1;
        CurrentLevel = SceneManager.GetActiveScene().name;
        if(CurrentLevel == "HowToPlay")
        {
            LoadMainScreen();
        }
        else
        {   
            SceneManager.LoadScene("Level " + CurrentLevel); 
        }
        
    }
}
