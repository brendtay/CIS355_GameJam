using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 20f;             // Normal movement speed
    public float health = 10f;            // Player's health
    public bool isBlocking = false;       // Flag to check if player is blocking
    public float hitCooldown = 0.5f;      // Time in seconds between allowed hits

    public float[] attackTime = { 0.3f, 0.5f, 0.2f };      // Cooldowns for attacks
    public float[] attackDamage = { 1f, 1.5f, 0.8f };      // Damage for attacks

    public InputAction MoveAction;
    public Image healthBar;                // Reference to the health bar UI image

    public bool canMove = false; // Flag to control whether the player can move

    public float healthRestoreAmount = 2f;       // Amount of health restored by health items

    private float startHealth = 0;

    private Rigidbody2D rigidbody2d;
    private Vector2 move;
    private Vector2 moveDirection = new Vector2(1, 0);

    private Animator animator;
    private AnimatorStateInfo stateInfo;
    private float lastHitTime;            // Track the last time the player was hit

    public Collider2D AttackAreaLeft;
    public Collider2D AttackAreaRight;

    private float[] lastAttackTime = { 0f, 0f, 0f };


    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        MoveAction.Enable();

        AttackAreaLeft.enabled = false;
        AttackAreaRight.enabled = false;

        startHealth = health;
    }

    void Update()
    {
        if(canMove)
        {
            Movement(); 
        }
        
    }

    void FixedUpdate()
    {
        Vector2 position = rigidbody2d.position + move * speed * Time.deltaTime;
        rigidbody2d.MovePosition(position);
    }

    IEnumerator Attack(int attackIndex)
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Set the appropriate trigger and enable colliders based on the attack index
        if (!stateInfo.IsName($"Attack {attackIndex + 1}"))
        {
            animator.SetTrigger($"Attack {attackIndex + 1}");

            yield return new WaitForSeconds(0.3f);

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

            yield return new WaitForSeconds(0.3f);
            AttackAreaLeft.enabled = false;
            AttackAreaRight.enabled = false;
        }
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
            jumpAnimation();
        }

        // Check for each attack's cooldown and trigger attack accordingly
        if (Input.GetKeyDown(KeyCode.UpArrow) && Time.time > lastAttackTime[0] + attackTime[0])
        {
            lastAttackTime[0] = Time.time;
            StartCoroutine(Attack(0));
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && Time.time > lastAttackTime[1] + attackTime[1])
        {
            lastAttackTime[1] = Time.time;
            StartCoroutine(Attack(1));
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && Time.time > lastAttackTime[2] + attackTime[2])
        {
            lastAttackTime[2] = Time.time;
            StartCoroutine(Attack(2));
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            blockAnimation(true);
            isBlocking = true;
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            blockAnimation(false);
            isBlocking = false;
        }
    }

    void jumpAnimation()
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        animator.SetTrigger("Jump");
    }

    void blockAnimation(bool isBlocking)
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        animator.SetBool("isBlocking", isBlocking);
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

        // Check if the player collided with a health item
        if (other.CompareTag("Health") && health < startHealth)
        {
            // Restore health, but don't exceed max health
            HealPlayer(healthRestoreAmount);

            // Optionally, destroy the health item
            Destroy(other.gameObject);
        }
    }
    private void TakeDamage(float damage)
    {
        if (isBlocking)
        {
            health -= damage/2;
            Debug.Log("Player blocked the attack! Health: " + health);
        }
        else
        {
            health -= damage;
            Debug.Log("Player was hit! Health: " + health);
            
        }

        if(health > 0)
        {
            animator.SetTrigger("Hit");
            UpdateHealthUI();
        }
        else
        {
            Die(); 
        }
    }

    public void HealPlayer(float healAmount)
    {
        health += healAmount;
        if (health > startHealth)
        {
            health = startHealth;
        }
    }
    private void UpdateHealthUI()
    {
        // Assuming 100 is the maximum health, adjust as necessary
        healthBar.fillAmount = health / startHealth;
    }

    void Die()
    {
        Debug.Log("Player has died.");
        animator.SetTrigger("Death");
    }
}
