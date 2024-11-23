using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton instance

    public float playerHealthStore;
    public float currentPowerUpStore;
    public int heartsInUIStore;
    public float startPlayerHealth = 10f;

    public string currentLevelName;
    public bool levelComplete;
    public int currentLevel = 1;

   

    void Awake()
    {
        // Implement Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist this object between scenes
        }
        else
        {
            Destroy(gameObject); // Ensure there's only one instance of GameManager
        }
    }

    public void SavePlayerState(PlayerMovement player)
    {
        // Save the player's state
        playerHealthStore = player.playerHealth;
        currentPowerUpStore = player.powerupIncrement;
        heartsInUIStore = player.heartsInUI;
    }

    public void LoadPlayerState(PlayerMovement player)
    {
        // Restore the player's state
        player.playerHealth = playerHealthStore;
        player.powerupIncrement = currentPowerUpStore;
        player.heartsInUI = heartsInUIStore;
    }

    void Start()
    {
        levelComplete = false;
        playerHealthStore = startPlayerHealth; // Initialize player health in GameManager
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Level 1");
        Time.timeScale = 1;
        currentLevel = 1;
    }

    public void HowToPlay()
    {
        SceneManager.LoadScene("HowToPlay");
        Time.timeScale = 1;
    }

   
   
}
