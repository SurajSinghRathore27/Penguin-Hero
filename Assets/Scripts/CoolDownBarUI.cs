using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CooldownBarUI : MonoBehaviour
{
    [Header("UI References")]
    public Image fillImage;
    public GameObject barContainer;

    [Header("Colors")]
    public Color chargingColor = Color.yellow;
    public Color cooldownColor = Color.red;
    public Color readyColor = Color.green;

    private CherryThrowController throwController;

    void Start()
    {
        throwController = GetComponentInParent<CherryThrowController>();

        if (barContainer != null)
        {
            barContainer.SetActive(false);
        }
    }

    void Update()
    {
        if (throwController == null || fillImage == null || barContainer == null)
            return;

        if (throwController.IsCharging())
        {
            barContainer.SetActive(true);
            fillImage.fillAmount = throwController.GetChargeProgress();
            fillImage.color = chargingColor;
        }
        else if (throwController.IsOnCooldown())
        {
            barContainer.SetActive(true);
            fillImage.fillAmount = throwController.GetCooldownProgress();
            fillImage.color = cooldownColor;
        }
        else
        {
            barContainer.SetActive(false);
        }
    }
}
