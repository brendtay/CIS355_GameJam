using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void UpdateMoveDirection(float moveDirectionX)
    {
        animator.SetFloat("Move_X", moveDirectionX);
    }

    public void TriggerAttack()
    {
        animator.SetTrigger("Attack");
    }

    public void TriggerHit()
    {
        animator.SetTrigger("Hit");
    }

    public void TriggerDeath()
    {
        animator.SetTrigger("Death");
    }
}
