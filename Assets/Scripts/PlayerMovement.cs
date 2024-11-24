using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 20f;             // Normal movement speed
    public float playerHealth = 10f;            // Player's health
    public bool isBlocking = false;       // Flag to check if player is blocking
    public float hitCooldown = 0.5f;      // Time in seconds between allowed hits

    public float[] attackTime = { 0.3f, 0.5f, 0.2f };      // Cooldowns for attacks
    public float[] attackDamage = { 1f, 1.5f, 0.8f };      // Damage for attacks

    public InputAction MoveAction;
    public Image healthBar;                // Reference to the health bar UI image

    public float healthRestoreAmount = 2f;       // Amount of health restored by health items
    public float scoreToPowerUp = 10f;

    public Image powerupBar;       // Reference to the power-up bar UI image
    public float currentPowerUpValue = 0f;
    private float maxPowerup = 100f;
  


    private float startHealth = 10;

    private Rigidbody2D rigidbody2d;
    private Vector2 move;
    private Vector2 moveDirection = new Vector2(1, 0);

    private Animator animator;
    private AnimatorStateInfo stateInfo;
    private float lastHitTime;            // Track the last time the player was hit

    public Collider2D AttackAreaLeft;
    public Collider2D AttackAreaRight;

    public int heartsInUI = 0;
    // Reference to heart UI elements
    public SpriteRenderer[] heartRenderers;

    // References to the full and empty heart sprites
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;

    private float[] lastAttackTime = { 0f, 0f, 0f };
    private GameManager gameManager;

    public GameObject endGameScreen;
    public LevelCompleteManager chatManager; 

    private float powerUpDamageMultiplyer = 1f;

    public AudioClip heartSound; 
    public AudioClip hurtSound;
    public AudioClip swordSwing;
    private AudioSource playerAudio;

    private bool isAlive = true;
    private bool isAttacking = false; // Flag to prevent attack spammin

    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        chatManager = FindObjectOfType<LevelCompleteManager>();
        gameManager = FindObjectOfType<GameManager>();

        playerAudio = GetComponent<AudioSource>();

        MoveAction.Enable();

        isAlive = true;

        AttackAreaLeft.enabled = false;
        AttackAreaRight.enabled = false;

        if (gameManager != null)
        {
            // Load saved state from GameManager
            gameManager.LoadPlayerState(this);
        }

        // Initialize UI and other variables
        healthBar.fillAmount = playerHealth / startHealth;
        powerupBar.fillAmount = currentPowerUpValue / maxPowerup;
        UpdateHeartsUI();
        endGameScreen.SetActive(false);
        DisableAttackCollider();
    }


    void Update()
    {
        Movement();

        // Check for the "E" key to use a heart and heal
        if (Input.GetKeyDown(KeyCode.E) && heartsInUI > 0 && isAlive)
        {
            RemoveHeart(); // Remove a heart
            HealPlayer(2f); // Heal the player (adjust the value as needed)
            playerAudio.PlayOneShot(heartSound, 2.0f);
        }

        if (currentPowerUpValue == maxPowerup && Input.GetKeyDown(KeyCode.Q) && isAlive) // Only activate if power-up meter is full
        {
            StartCoroutine(ActivatePowerUp());
            
        }

    }

    void FixedUpdate()
    {
        Vector2 position = rigidbody2d.position + move * speed * Time.deltaTime;
        rigidbody2d.MovePosition(position);
    }

    IEnumerator TriggerAttack(int attackIndex)
    {
        if (isAttacking) // Prevent triggering another attack
            yield break;

        isAttacking = true; // Set the attacking flag
        playerAudio.PlayOneShot(swordSwing, 1.5f);

        // Trigger the attack animation based on the attack index
        animator.SetTrigger($"Attack {attackIndex + 1}");

        // Enable the attack collider for the current attack
        EnableAttackCollider(attackIndex);

        // Wait for the duration of the attack animation
        float attackDuration = attackTime[attackIndex];
        yield return new WaitForSeconds(attackDuration);

        // Disable the attack collider after the animation
        DisableAttackCollider();

        // Wait for the attack cooldown to complete
        yield return new WaitForSeconds(attackTime[attackIndex]);

        isAttacking = false; // Reset the attacking flag
    }
    void Movement()
    {
        move = MoveAction.ReadValue<Vector2>();
        if (!isAlive)
        {
            return;
        }
        if (move.sqrMagnitude > 0.0f)
        {
            moveDirection = move.normalized;
        }

        animator.SetFloat("Move_X", moveDirection.x);
        animator.SetFloat("Speed", move.magnitude);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerJump();
        }

        // Check for each attack's cooldown and trigger attack accordingly
        if (Input.GetKeyDown(KeyCode.UpArrow) && Time.time > lastAttackTime[0] + attackTime[0])
        {
            lastAttackTime[0] = Time.time;
            StartCoroutine(TriggerAttack(0));
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && Time.time > lastAttackTime[1] + attackTime[1])
        {
            lastAttackTime[1] = Time.time;
            StartCoroutine(TriggerAttack(1));
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && Time.time > lastAttackTime[2] + attackTime[2])
        {
            lastAttackTime[2] = Time.time;
            StartCoroutine(TriggerAttack(2));
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            TriggerBlock(true);
            isBlocking = true;
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            TriggerBlock(false);
            isBlocking = false;
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyAttack") && Time.time > lastHitTime + hitCooldown)
        {
            DamageInfo damageInfo = other.GetComponent<DamageInfo>();

            lastHitTime = Time.time;

            if (damageInfo != null)
            {
                TakeDamage(damageInfo.enemyDamageAmount);
                playerAudio.PlayOneShot(hurtSound, 1.0f); 
            }
        }

        if (other.CompareTag("EndLevel") && chatManager.levelComplete)
        {
            gameManager.currentLevel++; 
            LoadNextLevel(); // Call the end level function in GameManager
        }

        if (other.CompareTag("EndLevelTutorual"))
        {
            LoadMainScreen();
        }

        if (other.CompareTag("Health"))
        {
            AddHeart(); // Add a heart to the UI
            Destroy(other.gameObject); // Remove the heart object from the scene
        }
    }
    void TriggerJump()
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        animator.SetTrigger("Jump");
    }

    void TriggerBlock(bool isBlocking)
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        animator.SetBool("isBlocking", isBlocking);
    }

    private void TakeDamage(float damage)
    {
        if (isBlocking)
        {
            playerHealth -= damage / 2;
            Debug.Log("Player blocked the attack! Health: " + playerHealth);
        }
        else
        {
            playerHealth -= damage;
            Debug.Log("Player was hit! Health: " + playerHealth);
        }

        if (playerHealth > 0)
        {
            animator.SetTrigger("Hit");
            DecreaseHealthUI();
        }
        else
        {
            Death();
        }
    }

    public void HealPlayer(float healAmount)
    {
        float targetHealth = Mathf.Min(playerHealth + healAmount, startHealth);

        playerHealth = targetHealth; // Update player's health
        healthBar.fillAmount = playerHealth / startHealth; // Update health bar UI
    }

    private void DecreaseHealthUI()
    {
        healthBar.fillAmount = playerHealth / startHealth;
    }

    private void Death()
    {
        isAlive = false; 
        Debug.Log("Player has died.");
        animator.SetTrigger("Death");
        GameOver();
    }
    public void IncrementPowerup(float powerUpValue)
    {
       currentPowerUpValue += powerUpValue;

        // Clamp currentPowerup to the maximum allowed value
        this.currentPowerUpValue = Mathf.Min(this.currentPowerUpValue, maxPowerup);

        // Update the UI to reflect the new powerup value
        UpdatePowerupUI();
    }


    public void UpdatePowerupUI()
    {
        powerupBar.fillAmount = currentPowerUpValue / maxPowerup;
    }

    private IEnumerator ActivatePowerUp()
    {
        powerUpDamageMultiplyer = 2.5f; // Set the damage multiplier
        Debug.Log("Power-up activated!");

        float duration = 5f; // Duration for the power-up
        float elapsedTime = 0f; // Track elapsed time
        float initialPowerUpValue = currentPowerUpValue;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // Gradually reduce the power-up meter
            currentPowerUpValue = Mathf.Lerp(initialPowerUpValue, 0f, elapsedTime / duration);
            UpdatePowerupUI(); // Update the UI to reflect the meter drain

            yield return null; // Wait for the next frame
        }

        // Ensure the power-up meter is fully drained at the end
        currentPowerUpValue = 0f;
        UpdatePowerupUI();

        // Reset the damage multiplier
        powerUpDamageMultiplyer = 1f;
        Debug.Log("Power-up ended.");
    }

    public void EnableAttackCollider(int attackIndex)
    {
        if (moveDirection.x > 0)
        {
            AttackAreaRight.GetComponent<DamageInfo>().playerDamageAmount = attackDamage[attackIndex] * powerUpDamageMultiplyer;
            AttackAreaRight.enabled = true;
        }
        else
        {
            AttackAreaLeft.GetComponent<DamageInfo>().playerDamageAmount = attackDamage[attackIndex] * powerUpDamageMultiplyer;
            AttackAreaLeft.enabled = true;
        }
    }

    public void DisableAttackCollider()
    {
        AttackAreaLeft.enabled = false;
        AttackAreaRight.enabled = false;
    }

    public void UpdateHeartsUI()
    {
        for (int i = 0; i < heartRenderers.Length; i++)
        {
            // Set the sprite to full heart if index is less than heartsInUI, else set it to empty heart
            heartRenderers[i].sprite = i < heartsInUI ? fullHeartSprite : emptyHeartSprite;
        }
    }

    // Example: Increase hearts and update UI
    public void AddHeart()
    {
        if (heartsInUI < heartRenderers.Length)
        {
            heartsInUI++;
            UpdateHeartsUI();
        }
    }

    // Example: Decrease hearts and update UI
    public void RemoveHeart()
    {
        if (heartsInUI > 0)
        {
            heartsInUI--;
            UpdateHeartsUI();
        }
    }
    public void GameOver()
    {
        endGameScreen.SetActive(true); // Show the end game screen
        Time.timeScale = 0;
    }

    public void LoadMainScreen()
    {
        SceneManager.LoadScene("MainScreen");
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1;

        chatManager.StartLevel();
        // Save player's state to GameManager
        if (gameManager != null)
        {
            gameManager.SavePlayerState(this);
        }

        // Update the level and load the next scene
        string currentLevelName = SceneManager.GetActiveScene().name;
        

        if (currentLevelName == "HowToPlay")
        {
            SceneManager.LoadScene("MainScreen");
        }
        else
        {
            SceneManager.LoadScene("Level " + gameManager.currentLevel);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        isAlive = true; 
        
        // Reset player's state
        if (gameManager != null)
        {
            gameManager.playerHealthStore = gameManager.startPlayerHealth;
            gameManager.currentPowerUpStore = 0f;
            gameManager.heartsInUIStore = 0;
            gameManager.currentLevel = 1; 
        }

        // Reload the initial scene
        string currentLevelName = SceneManager.GetActiveScene().name;

        if (currentLevelName == "HowToPlay")
        {
            SceneManager.LoadScene("HowToPlay");
        }
        else
        {
            SceneManager.LoadScene("Level 1");
        }
    }
}

