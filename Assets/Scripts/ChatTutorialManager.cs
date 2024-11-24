using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatTutorialManager : MonoBehaviour
{
    // Public text objects
    public TMP_Text chatText; // Drag your "ChatText" TMP_Text object here in the inspector
    public Canvas tutorialCanvas; // Reference to the Canvas containing the chat box
    
    // Private text QueueStrings
    private Queue<string> tutorialMessages; // Queue to store tutorial messages
    private Queue<string> secondaryMessages; // Second queue for additional messages

    // Public Objects
    public GameObject[] enemies; // Array to hold enemy prefabs
    public Transform[] spawnPoints; // Array to hold spawn points for enemies
    public GameObject wizard;
    public GameObject endLevelCollision;

    // Private variables for tracking
    private bool enemiesSpawned = false; // Track if enemies have been spawned
    private bool secondaryMessagesActive = false; // Track if secondary messages are active
    private bool enterHasBeenPressed = false; // Track if enter has been pressed to skip 
    private int partOfTutorial = 1; // Tracks what part of the tutorial the player is at 
    private int enemyRound = 0; // Tracks what round the player is at
    
   // Refrences to scripts
    private GameManager gameManager;

    void Start()
    {
        tutorialMessages = new Queue<string>(); // Loads the string for message 1
        secondaryMessages = new Queue<string>(); // Loads the string for message 2

        gameManager = FindObjectOfType<GameManager>();

        // Load initial tutorial messages (can be customized per level or scene)
        LoadTutorialMessages(new string[]
        {
            "Welcome, brave warrior!",
            "Thank you for volunteering on this journey!",
            "I'm Lyra, and I'll be guiding you.",
            "Let’s start by learning the basics.",
            "Use WASD to move around.",
            "Press Space to jump.",
            "Use the Up Arrow for a medium attack.",
            "Try the Left Arrow for a quick light attack.",
            "Press the Right Arrow for a powerful heavy attack.",
            "Hold the Down Arrow to raise your shield.",
            "Now, let’s put those skills to the test.",
            "I’ll summon some enemies!"
        });

        // Load additional secondary messages
        LoadSecondaryMessages(new string[]
        {
            "If you saw a enemy dropped a heart!",
            "Walk over it to pick it up",
            "You can store up to 3 hearts if your health is full",
            "You can press E at any time to use one!",
            "Congradulations you finished the tutorial!",
            "Lets continue into this journy."
        });

        // Start displaying messages
        DisplayNextMessage();
    }

    void Update()
    {
        // Check if enemies have been spawned and if there are no remaining enemies
        if (enemiesSpawned && GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            if (enemyRound < 3)
            {
                enemiesSpawned = false; // Reset spawn flag to allow spawning again
                SpawnEnemies(); // Spawn the next round of enemies
                Debug.Log(enemyRound);
            }
            else
            {
                enemiesSpawned = false; // Reset spawn flag for final completion
                ShowChatBox("Well done! You've defeated all enemies!"); // Show a message after defeating all enemies
                partOfTutorial = 2;
                enterHasBeenPressed = false;

                // Activate the secondary messages
                secondaryMessagesActive = true;
                wizard.gameObject.SetActive(true);
                StartCoroutine(DisplaySecondaryMessagesAfterDelay(3f)); // Start displaying secondary messages after a delay
            }
        }

        // Skip tutorial messages and spawn enemies immediately if Enter is pressed
        if (Input.GetKeyDown(KeyCode.Return) && !enterHasBeenPressed)
        {
            if (partOfTutorial == 1)
            {
                SpawnEnemies();
                SkipTutorial();
            }
            else if (partOfTutorial == 2)
            {
                SkipSecondaryMessages();
                Debug.Log("End Tutorial");
            }

            enterHasBeenPressed = true;
        }
    }

    public void LoadTutorialMessages(string[] messages)
    {
        // Clear any existing messages
        tutorialMessages.Clear();

        // Enqueue new messages
        foreach (string message in messages)
        {
            tutorialMessages.Enqueue(message);
        }
    }

    public void LoadSecondaryMessages(string[] messages)
    {
        // Clear any existing secondary messages
        secondaryMessages.Clear();

        // Enqueue new secondary messages
        foreach (string message in messages)
        {
            secondaryMessages.Enqueue(message);
        }
    }

    public void DisplayNextMessage()
    {
        if (tutorialMessages.Count > 0)
        {
            // Display the next message from the tutorial queue
            chatText.text = tutorialMessages.Dequeue();
            StartCoroutine(DisplayNextMessageWithDelay(5f)); // Adjust delay time as needed
        }
        else
        {
            chatText.text = ""; // Clear chat box when done
            tutorialCanvas.gameObject.SetActive(false); // Hide the entire canvas
            wizard.gameObject.SetActive(false);
            // Spawn enemies once the tutorial messages are done
            SpawnEnemies();
        }
    }

    private IEnumerator DisplayNextMessageWithDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Real-time delay to work even when paused
        DisplayNextMessage();
    }

    private void SpawnEnemies()
    {
        if (!enemiesSpawned)
        {
            for (int i = 0; i < enemies.Length && i < spawnPoints.Length; i++)
            {
                Instantiate(enemies[i], spawnPoints[i].position, spawnPoints[i].rotation);
            }
            enemiesSpawned = true; // Set flag to true once enemies are spawned
            enemyRound++; // Increment only when enemies are spawned
        }
    }


    private void ShowChatBox(string message)
    {
        tutorialCanvas.gameObject.SetActive(true); // Show the entire canvas
        wizard.gameObject.SetActive(true);
        chatText.text = message; // Display the message
    }

    private IEnumerator DisplaySecondaryMessagesAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Delay before starting secondary messages
        DisplaySecondaryMessage();
    }

    private void DisplaySecondaryMessage()
    {
        if (secondaryMessages.Count > 0 && secondaryMessagesActive)
        {
            tutorialCanvas.gameObject.SetActive(true); // Show the entire canvas
            wizard.gameObject.SetActive(true);
            chatText.text = secondaryMessages.Dequeue();
            StartCoroutine(DisplaySecondaryMessageWithDelay(5f)); // Adjust delay time as needed
        }
        else
        {
            // Hide the canvas if no more secondary messages
            tutorialCanvas.gameObject.SetActive(false);
            wizard.gameObject.SetActive(false);
            secondaryMessagesActive = false;
            
        }
    }

    private IEnumerator DisplaySecondaryMessageWithDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        DisplaySecondaryMessage();
    }

    private void SkipTutorial()
    {
        tutorialMessages.Clear(); // Clear all tutorial messages
        tutorialCanvas.gameObject.SetActive(false); // Hide the entire canvas
        wizard.SetActive(false);
    }

    private void SkipSecondaryMessages()
    {
        secondaryMessages.Clear(); // Clear all secondary messages
        tutorialCanvas.gameObject.SetActive(false); // Hide the entire canvas
        wizard.gameObject.SetActive(false);
        secondaryMessagesActive = false; // Stop secondary messages from displaying
    }
}
