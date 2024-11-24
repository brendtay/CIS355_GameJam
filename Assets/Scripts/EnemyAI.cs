using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    public float speed = 3.0f;             // Enemy's movement speed
    public float stopDistance = 1.5f;      // Distance at which enemy stops following player
    public float health = 100;             // Enemy's health, set to max health (e.g., 100)
    public float attackCooldown = 1.0f;    // Cooldown time between attacks
    public float attackDamage = 1.0f;
    public float deathAnimationTime = 1.0f;
    public float damageCooldown = 0.4f;    // Cooldown time between damage instances
    public Image healthBar;                // Reference to the health bar UI image
    public Collider2D AttackAreaLeft;      // Enemy's left attack collider
    public Collider2D AttackAreaRight;     // Enemy's right attack collider
    public GameObject heart;               // Heart prefab to spawn upon enemy death
    public float heartDropChance = 50.0f;  // Percentage chance to drop a heart, default is 50%
    public float timeBeforeAttack = 0.5f;  // How long the animation takes before it swings

    private Transform player;              // Reference to the player's position
    private float startHealth = 0;
    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private bool isDead = false;           // Flag to check if enemy is dead
    private bool isAttacking = false;      // Flag to check if enemy is currently attacking
    private float lastAttackTime;          // Tracks the last attack time


    private float lastDamageTime = 0;      // Tracks the last damage time
    private Animator animator;
    private PlayerMovement playerScript;

    public float powerUpValue = 0.5f;
    

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // Automatically find the player
        rb = GetComponent<Rigidbody2D>();
        playerScript = player.GetComponent<PlayerMovement>();

        // Disable both attack colliders initially
        AttackAreaLeft.enabled = false;
        AttackAreaRight.enabled = false;
        animator = GetComponent<Animator>();

        startHealth = health;
    }

    void Update()
    {
        if (isDead) return; // Only allow actions if the enemy is not dead

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > stopDistance)
        {
            // Move towards the player if outside stop distance
            moveDirection = (player.position - transform.position).normalized;
            rb.velocity = moveDirection * speed;
            animator.SetFloat("Move_X", moveDirection.x);
        }
        else
        {
            // Stop moving and trigger an attack if within stop distance
            rb.velocity = Vector2.zero;
            if (Time.time > lastAttackTime + attackCooldown && !isAttacking && !isDead)
            {
                StartCoroutine(Attack());
            }
        }
    }

    IEnumerator Attack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        TriggerAttack(); // This will play the attack animation

        // Wait for the attack animation to finish
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        isAttacking = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerAttack") && !isDead)
        {
            DamageInfo damageInfo = other.GetComponent<DamageInfo>();
            if (damageInfo != null)
            {
                TakeDamage(damageInfo.playerDamageAmount);
            }
        }
    }

    private void TakeDamage(float damage)
    {
        if (Time.time >= lastDamageTime + damageCooldown) // Check if enough time has passed
        {
            health -= damage;
            UpdateHealthUI(); // Update the health bar UI whenever damage is taken
            lastDamageTime = Time.time; // Update last damage time

            if (health > 0)
            {
                TriggerHit(); // Play hit reaction animation
                              //Debug.Log("Enemy hit");

            }
            else
            {
                Die();
                
            }
        }
    }

    private void UpdateHealthUI()
    {
        // Assuming 100 is the maximum health, adjust as necessary
        healthBar.fillAmount = health / startHealth;
    }

    private void Die()
    {
        isDead = true;
        TriggerDeath(); // Trigger death animation
        rb.velocity = Vector2.zero;

        playerScript.IncrementPowerup(powerUpValue);
        
        // Check if a heart should be dropped based on heartDropChance
        float dropChance = Random.Range(0f, 100f);
        if (dropChance <= heartDropChance)
        {
            Instantiate(heart, transform.position, Quaternion.identity); // Spawn the heart at the enemy's position
            Debug.Log("enemy dropped health!");
        }

        Destroy(gameObject, deathAnimationTime); // Adjust delay to match death animation length
    }
    private void TriggerAttack()
    {
        animator.SetTrigger("Attack");
    }

    private void TriggerHit()
    {
        animator.SetTrigger("Hit");
    }

    private void TriggerDeath()
    {
        animator.SetTrigger("Death");
    }
    public void EnableAttackCollider()
    {
        if (moveDirection.x > 0)
        {
            AttackAreaRight.GetComponent<DamageInfo>().enemyDamageAmount = attackDamage;
            AttackAreaRight.enabled = true;
        }
        else
        {
            AttackAreaLeft.GetComponent<DamageInfo>().enemyDamageAmount = attackDamage;
            AttackAreaLeft.enabled = true;
        }
    }

    public void DisableAttackCollider()
    {
        AttackAreaLeft.enabled = false;
        AttackAreaRight.enabled = false;
    }
}
