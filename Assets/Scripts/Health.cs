using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Player Death Settings")]
    public bool isPlayer = false;
    public float restartDelay = 2f;
    public string introSceneName = "Intro Scene";

    [Header("Events")]
    public UnityEvent<float> onHealthChanged;
    public UnityEvent onDeath;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        if (isPlayer && CompareTag("Player"))
        {
            isPlayer = true;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        onHealthChanged?.Invoke(GetHealthPercentage());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        onHealthChanged?.Invoke(GetHealthPercentage());
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        onDeath?.Invoke();

        if (isPlayer)
        {
            DisablePlayerControls();
            StartCoroutine(RestartGame());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void DisablePlayerControls()
    {
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        CherryThrowController cherryThrow = GetComponent<CherryThrowController>();
        if (cherryThrow != null)
        {
            cherryThrow.enabled = false;
        }

        PickupController pickup = GetComponent<PickupController>();
        if (pickup != null)
        {
            pickup.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(restartDelay);
        SceneManager.LoadScene(introSceneName);
    }
}
