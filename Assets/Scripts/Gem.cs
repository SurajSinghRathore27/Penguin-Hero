using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour, Iitem
{
    public static event Action<int> OnGemCollect;
    public int worth = 1;

    public void collect()
    {
        OnGemCollect?.Invoke(worth);

        MinimapIcon minimapIcon = GetComponent<MinimapIcon>();
        if (minimapIcon != null)
        {
            Destroy(minimapIcon);
        }

        Destroy(gameObject);
    }
}
