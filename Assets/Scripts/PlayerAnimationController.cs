using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private Health health;
    private float lastHealthValue;

    void Start()
    {
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();

        if (health != null)
        {
            lastHealthValue = health.currentHealth;
        }
    }

    void Update()
    {
        if (health != null && health.currentHealth < lastHealthValue)
        {
            PlayHurtAnimation();
            lastHealthValue = health.currentHealth;
        }
    }

    public void PlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    public void PlayHurtAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }
    }
}
