using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
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
    private float currentPowerup = 0f;
    private float maxPowerup = 100f;
    public float powerupIncrement = 10f; // Amount to increase per successful hit


    private float startHealth = 0;

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

    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        gameManager = FindObjectOfType<GameManager>();
        MoveAction.Enable();

        AttackAreaLeft.enabled = false;
        AttackAreaRight.enabled = false;

        startHealth = playerHealth;
        powerupBar.fillAmount = 0f; 
        DisableAttackCollider();

        UpdateHeartsUI();
    }

    void Update()
    {
        Movement();

        // Check for the "E" key to use a heart and heal
        if (Input.GetKeyDown(KeyCode.E) && heartsInUI > 0)
        {
            RemoveHeart(); // Remove a heart
            HealPlayer(2f); // Heal the player (adjust the value as needed)
        }

    }

    void FixedUpdate()
    {
        Vector2 position = rigidbody2d.position + move * speed * Time.deltaTime;
        rigidbody2d.MovePosition(position);
    }

    IEnumerator TriggerAttack(int attackIndex)
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Set the appropriate trigger and enable colliders based on the attack index
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Set the appropriate trigger for the attack based on the attack index
        if (!stateInfo.IsName($"Attack {attackIndex + 1}"))
        {
            animator.SetTrigger($"Attack {attackIndex + 1}");
        }

        // Wait for the attack animation to finish
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
    }
    void Movement()
    {
        move = MoveAction.ReadValue<Vector2>();

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
            }
        }

        if (other.CompareTag("EndLevel") && gameManager.levelComplete)
        {
            gameManager.LoadNextLevel(); // Call the end level function in GameManager
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
        Debug.Log("Player has died.");
        animator.SetTrigger("Death");
        gameManager.GameOver();
    }
    public void IncrementPowerup()
    {
        currentPowerup += powerupIncrement;
        if (currentPowerup > maxPowerup) currentPowerup = maxPowerup;
        UpdatePowerupUI();
    }

    private void UpdatePowerupUI()
    {
        powerupBar.fillAmount = currentPowerup / maxPowerup;
    }

    private void UsePowerUp()
    {

    }

    public void EnableAttackCollider(int attackIndex)
    {
        if (moveDirection.x > 0)
        {
            AttackAreaRight.GetComponent<DamageInfo>().playerDamageAmount = attackDamage[attackIndex];
            AttackAreaRight.enabled = true;
        }
        else
        {
            AttackAreaLeft.GetComponent<DamageInfo>().playerDamageAmount = attackDamage[attackIndex];
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

}

