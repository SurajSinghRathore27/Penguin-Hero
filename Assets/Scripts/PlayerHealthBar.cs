using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [Header("UI References")]
    public Image fillImage;
    public Health playerHealth;

    void Start()
    {
        if (playerHealth == null)
        {
            playerHealth = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Health>();
        }

        if (playerHealth != null)
        {
            playerHealth.onHealthChanged.AddListener(UpdateHealthBar);
            UpdateHealthBar(playerHealth.GetHealthPercentage());
        }
    }

    void UpdateHealthBar(float healthPercentage)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = healthPercentage;
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.onHealthChanged.RemoveListener(UpdateHealthBar);
        }
    }
}

