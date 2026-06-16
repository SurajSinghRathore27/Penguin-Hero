using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableObject : MonoBehaviour
{
    [Header("Object Settings")]
    public float damage = 15f;
    public float weight = 2f;
    public LayerMask damageLayer;

    [Header("Pickup Settings")]
    public float pickupRadius = 1.5f;

    private Rigidbody2D rb;
    private Collider2D objectCollider;
    private bool isPickedUp;
    private bool hasBeenThrown;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        objectCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void PickUp(Transform holder)
    {
        isPickedUp = true;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector2.zero;
        }

        if (objectCollider != null)
        {
            objectCollider.enabled = false;
        }

        transform.SetParent(holder);
        transform.localPosition = Vector3.zero;
    }

    public void Throw(Vector2 velocity)
    {
        isPickedUp = false;
        hasBeenThrown = true;

        transform.SetParent(null);

        if (objectCollider != null)
        {
            objectCollider.enabled = true;
        }

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = velocity;
            rb.angularVelocity = Random.Range(-360f, 360f);
        }
    }

    public bool IsPickedUp()
    {
        return isPickedUp;
    }

    public float GetWeight()
    {
        return weight;
    }

    public Vector3 GetVisualCenter()
    {
        if (spriteRenderer != null)
        {
            return spriteRenderer.bounds.center;
        }
        return transform.position;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasBeenThrown) return;

        if (((1 << collision.gameObject.layer) & damageLayer) != 0)
        {
            Health enemyHealth = collision.gameObject.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
        else
        {
            hasBeenThrown = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
