using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatTutorialManager : MonoBehaviour
{
    public TMP_Text chatText; // Drag your "ChatText" TMP_Text object here in the inspector
    public GameObject chatBox; // Reference to the GameObject containing the chat text (usually the parent)
    private Queue<string> tutorialMessages; // Queue to store tutorial messages

    private PlayerMovement playerMovement; // Reference to PlayerMovement script

    public GameObject[] enemies; // Array to hold enemy prefabs
    public Transform[] spawnPoints; // Array to hold spawn points for enemies

    private bool enemiesSpawned = false; // Track if enemies have been spawned


    void Start()
    {
        tutorialMessages = new Queue<string>();

        // Load initial tutorial messages (can be customized per level or scene)
        LoadTutorialMessages(new string[]
        {
            "Welcome! Use WASD to move around",
            "Press Space to jump.",
            "Use the Up Arrow for a medium attack.",
            "Try the Left Arrow for a light attack.",
            "Press the Right Arrow to unleash a heavy attack.",
            "Hold the Down Arrow to raise your shield.",
            "Now, let’s put those skills to the test.",
            "I’ll summon some enemies!"
        });

        // Get the PlayerMovement component
        playerMovement = FindObjectOfType<PlayerMovement>();

        // Disable player movement initially
        if (playerMovement != null)
        {
            playerMovement.canMove = true;
        }

        // Start displaying messages
        DisplayNextMessage();
    }

    void Update()
    {
        // Check if enemies have been spawned and if there are no remaining enemies
        if (enemiesSpawned && GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            enemiesSpawned = false; // Reset spawn flag
            ShowChatBox("Well done! You've defeated all enemies!"); // Show a message after defeating all enemies
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

    public void DisplayNextMessage()
    {
        if (tutorialMessages.Count == 0)
        {
            chatText.text = ""; // Clear chat box when done
            chatBox.SetActive(false); // Hide the chat box

            // Enable player movement when tutorial is complete
            if (playerMovement != null)
            {
                playerMovement.canMove = true;
            }
            Debug.Log("Spawned Enemies");
            SpawnEnemies();
            return;
        }

        // Display the next message in the queue
        chatText.text = tutorialMessages.Dequeue();

        // Automatically proceed to the next message after a delay
        StartCoroutine(DisplayNextMessageWithDelay(5f)); // Adjust delay time as needed
    }

    private IEnumerator DisplayNextMessageWithDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Real-time delay to work even when paused
        DisplayNextMessage();
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < enemies.Length && i < spawnPoints.Length; i++)
        {
            Instantiate(enemies[i], spawnPoints[i].position, spawnPoints[i].rotation);
        }

        enemiesSpawned = true; // Set flag to true once enemies are spawned
    }

    private void ShowChatBox(string message)
    {
        chatBox.SetActive(true); // Show the chat box
        chatText.text = message; // Display the message
    }
}
