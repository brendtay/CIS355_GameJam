using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton instance

    public GameObject endGameScreen;
    public string CurrentLevel;
    public bool levelComplete;
    public int currentLevel = 1;

    void Awake()
    {
        // Singleton pattern to ensure one instance of GameManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist the GameManager between scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate GameManager instances
        }
    }

    void Start()
    {
        // Don't assume endGameScreen is assigned; handle null cases dynamically
        if (endGameScreen != null)
        {
            endGameScreen.SetActive(false);
        }

        levelComplete = false;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to the sceneLoaded event
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe from the event
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find the object by its name
        GameObject foundObject = GameObject.Find("EndGame");

        if (foundObject != null)
        {
            endGameScreen = foundObject;

            // Optionally set it inactive at the start
            endGameScreen.SetActive(false);

            Debug.Log("EndGame object successfully assigned by name.");
        }
        else
        {
            Debug.LogWarning("EndGame object not found in the current scene by name.");
        }
    }


    public void StartGame()
    {
        SceneManager.LoadScene("Level 1");
        Time.timeScale = 1;
        currentLevel = 1;
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
        if (endGameScreen != null)
        {
            endGameScreen.SetActive(true); // Show the end game screen
            Time.timeScale = 0;
        }
        else
        {
            Debug.LogError("EndGameUI is not assigned. Cannot display end game screen.");
        }
    }

    public void RestartGame()
    {
        // Reload the current level and reset time scale
        Time.timeScale = 1;
        CurrentLevel = SceneManager.GetActiveScene().name;
        if (CurrentLevel == "HowToPlay")
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
        currentLevel++;
        if (CurrentLevel == "HowToPlay")
        {
            LoadMainScreen();
        }
        else
        {
            SceneManager.LoadScene("Level " + currentLevel);
        }
    }
}
