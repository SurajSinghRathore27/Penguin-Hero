using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBanditSpriteController : MonoBehaviour
{
    public Sprite specificSprite;

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (specificSprite != null)
        {
            SetSpecificSprite(specificSprite);
        }
    }

    public void SetSpecificSprite(Sprite sprite)
    {
        if (animator != null)
        {
            animator.enabled = false;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }
}

