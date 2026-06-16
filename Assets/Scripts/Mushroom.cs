using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mushroom : MonoBehaviour, Iitem
{
    [Header("Healing Settings")]
    public float healAmount = 25f;
    public bool destroyOnCollect = true;

    public void collect()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.Heal(healAmount);
            }
        }

        if (destroyOnCollect)
        {
            Destroy(gameObject);
        }
    }
}
