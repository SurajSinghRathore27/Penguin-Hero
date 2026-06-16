using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collector : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Iitem item = collision.GetComponent<Iitem>();
        if (item != null)
        {
            item.collect();
        }
    }
}
