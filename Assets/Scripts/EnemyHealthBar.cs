using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI References")]
    public Image fillImage;
    public GameObject barContainer;

    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 1f, 0);
    public bool hideWhenFull = true;

    private Health health;
    private Transform target;

    void Start()
    {
        health = GetComponentInParent<Health>();
        target = health.transform;

        if (health != null)
        {
            health.onHealthChanged.AddListener(UpdateHealthBar);
        }

        UpdateHealthBar(1f);
    }

    void UpdateHealthBar(float healthPercentage)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = healthPercentage;
        }

        if (hideWhenFull && barContainer != null)
        {
            barContainer.SetActive(healthPercentage < 1f);
        }
    }

    void OnDestroy()
    {
        if (health != null)
        {
            health.onHealthChanged.RemoveListener(UpdateHealthBar);
        }
    }
}

