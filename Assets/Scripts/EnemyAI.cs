using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    private Transform player;              // Reference to the player's position
    private float startHealth = 0;
    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private bool isDead = false;           // Flag to check if enemy is dead
    private bool isAttacking = false;      // Flag to check if enemy is currently attacking
    private float lastAttackTime;          // Tracks the last attack time

    private EnemyAnimation enemyAnimation; // Reference to EnemyAnimation script
    private float lastDamageTime = 0;      // Tracks the last damage time

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // Automatically find the player
        rb = GetComponent<Rigidbody2D>();
        enemyAnimation = GetComponent<EnemyAnimation>();

        // Disable both attack colliders initially
        AttackAreaLeft.enabled = false;
        AttackAreaRight.enabled = false;

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
            enemyAnimation.UpdateMoveDirection(moveDirection.x); // Update movement direction in animation
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
        enemyAnimation.TriggerAttack();

        yield return new WaitForSeconds(0.2f); // Adjust based on animation timing

        // Determine the facing direction and enable the appropriate attack area
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

        yield return new WaitForSeconds(0.1f); // Duration of the attack hitbox

        // Disable both attack areas after the attack completes
        AttackAreaLeft.enabled = false;
        AttackAreaRight.enabled = false;

        isAttacking = false; // Reset attacking flag after attack
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
                enemyAnimation.TriggerHit(); // Play hit reaction animation
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
        enemyAnimation.TriggerDeath(); // Trigger death animation
        rb.velocity = Vector2.zero;

        Destroy(gameObject, deathAnimationTime); // Adjust delay to match death animation length
    }
}
