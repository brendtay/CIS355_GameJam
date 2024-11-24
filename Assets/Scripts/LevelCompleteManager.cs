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
    private Queue<string> levelCompleteMessages; // Queue to store level completion messages
    private Queue<string> preGameMessages; // Queue to store pre-game messages
    private bool isDisplayingMessages = false; // Track if messages are currently being displayed

    // Public configuration
    [TextArea(3, 10)]
    public string[] messagesToDisplay; // Messages for level completion
    [TextArea(3, 10)]
    public string[] preGameMessagesArray; // Messages for pre-game
    public bool showPreGameMessages = false; // Toggle to enable/disable pre-game messages
    public bool levelComplete;

    private void Start()
    {
        levelCompleteMessages = new Queue<string>();
        preGameMessages = new Queue<string>();

        // Load messages
        LoadMessages(preGameMessagesArray, preGameMessages);
        LoadMessages(messagesToDisplay, levelCompleteMessages);

        // Start the game
        if (showPreGameMessages)
        {
            StartCoroutine(DisplayPreGameMessages());
        }
        else
        {
            StartLevel();
        }
    }

    public void StartLevel()
    {
        chatCanvas.gameObject.SetActive(false);
        wizard.SetActive(false);
        levelComplete = false;
  
    }

    private void Update()
    {
        // Ensure the script does not start displaying messages prematurely
        if (!levelComplete)
        {
            return; // Exit early if the level is not completed
        }

        if (!isDisplayingMessages)
        {
            StartCoroutine(DisplayLevelCompleteMessages());
        }

        if (Input.GetKeyDown(KeyCode.Return) && isDisplayingMessages)
        {
            SkipMessages();
        }
    }

    // Function to load messages into a queue
    public void LoadMessages(string[] messages, Queue<string> messageQueue)
    {
        messageQueue.Clear();
        foreach (string message in messages)
        {
            messageQueue.Enqueue(message);
        }
    }

    // Coroutine to display pre-game messages
    public IEnumerator DisplayPreGameMessages()
    {
        isDisplayingMessages = true;
        chatCanvas.gameObject.SetActive(true);
        wizard.SetActive(true);

        while (preGameMessages.Count > 0)
        {
            chatText.text = preGameMessages.Dequeue();
            yield return new WaitForSecondsRealtime(2.5f); // Adjust the delay as needed
        }

        // Hide chat box and wizard, then start the level
        chatCanvas.gameObject.SetActive(false);
        wizard.SetActive(false);
        isDisplayingMessages = false;
        StartLevel();
    }

    // Coroutine to display level complete messages
    public IEnumerator DisplayLevelCompleteMessages()
    {
        isDisplayingMessages = true;
        chatCanvas.gameObject.SetActive(true);
        wizard.SetActive(true);

        while (levelCompleteMessages.Count > 0)
        {
            chatText.text = levelCompleteMessages.Dequeue();
            yield return new WaitForSecondsRealtime(2.5f); // Adjust the delay as needed
        }

        // Hide the chat box and wizard after displaying all messages
        chatCanvas.gameObject.SetActive(false);
        wizard.SetActive(false);
        isDisplayingMessages = false;

        // Optionally, proceed to the next level or perform other actions
    }

    // Function to skip messages when Enter is pressed
    public void SkipMessages()
    {
        StopAllCoroutines();
        levelCompleteMessages.Clear();
        preGameMessages.Clear();
        chatCanvas.gameObject.SetActive(false);
        wizard.SetActive(false);
        isDisplayingMessages = false;
    }

    // Method to clean up the chat display when leaving the level
    public void CleanupChat()
    {
        if (isDisplayingMessages)
        {
            StopAllCoroutines();
            levelCompleteMessages.Clear();
            preGameMessages.Clear();
            chatCanvas.gameObject.SetActive(false);
            wizard.SetActive(false);
            isDisplayingMessages = false;
        }
    }
}
