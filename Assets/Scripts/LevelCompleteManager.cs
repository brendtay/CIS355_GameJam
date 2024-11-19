using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelCompleteManager : MonoBehaviour
{
    // Public Text Objects
    public TMP_Text chatText; // Reference to the text component in your chat box
    public Canvas chatCanvas; // Reference to the Canvas containing the chat box
    public GameObject wizard; // Reference to the wizard GameObject

    // Private variables for text
    // Queue is provided by the .net framwork and acts like a array
    private Queue<string> levelCompleteMessages; // Queue to store level completion messages
    private bool isDisplayingMessages = false; // Track if messages are currently being displayed

    // You can set these messages in the Inspector for each level
    [TextArea(3, 10)]
    public string[] messagesToDisplay;

    private void Start()
    {
        levelCompleteMessages = new Queue<string>();
        LoadLevelCompleteMessages(messagesToDisplay);

        // Initially hide the chat box and wizard
        chatCanvas.gameObject.SetActive(false);
        wizard.SetActive(false);
    }

    private void Update()
    {
        if (IsLevelCompleted() && !isDisplayingMessages)
        {
            StartCoroutine(DisplayLevelCompleteMessages());
        }

        if (Input.GetKeyDown(KeyCode.Return) && isDisplayingMessages)
        {
            SkipMessages();
        }
    }

    // Function to load messages into the queue
    public void LoadLevelCompleteMessages(string[] messages)
    {
        levelCompleteMessages.Clear();
        foreach (string message in messages) // Runs as long as theres a next message object
        {
            levelCompleteMessages.Enqueue(message); // Loads messages into queue 
        }
    }

    // Coroutine to display messages one after another with a delay
    public IEnumerator DisplayLevelCompleteMessages()
    {
        isDisplayingMessages = true;
        chatCanvas.gameObject.SetActive(true);
        wizard.SetActive(true);

        while (levelCompleteMessages.Count > 0)
        {
            chatText.text = levelCompleteMessages.Dequeue(); //Pulls the first element of the array
            yield return new WaitForSecondsRealtime(5f); // Adjust the delay as needed
        }

        // Hide the chat box and wizard after displaying all messages
        chatCanvas.gameObject.SetActive(false);
        wizard.SetActive(false);
        isDisplayingMessages = false;

        // Optionally, proceed to the next level or perform other actions
        // For example:
        // GameManager.Instance.LoadNextLevel();
    }

    // Function to check if the level is completed
    private bool IsLevelCompleted()
    {
        // Implement your own logic to determine if the level is completed
        // This could be a flag from your GameManager or any other condition
        // For example:
        GameManager gameManager = FindObjectOfType<GameManager>();
        return gameManager != null && gameManager.levelComplete;
    }

    // Function to skip messages when Enter is pressed
    public void SkipMessages()
    {
        StopAllCoroutines();
        levelCompleteMessages.Clear();
        chatCanvas.gameObject.SetActive(false);
        wizard.SetActive(false);
        isDisplayingMessages = false;
    }
}
