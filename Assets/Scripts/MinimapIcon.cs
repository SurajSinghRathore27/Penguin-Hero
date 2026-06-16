using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapIcon : MonoBehaviour
{
    [Header("Icon Settings")]
    public GameObject iconPrefab;
    public Vector3 iconOffset = new Vector3(0, 0, 0);
    public bool rotateWithObject = false;

    private GameObject iconInstance;

    void Start()
    {
        CreateIcon();
    }

    void CreateIcon()
    {
        if (iconPrefab != null)
        {
            iconInstance = Instantiate(iconPrefab, transform.position + iconOffset, Quaternion.identity);
            iconInstance.transform.SetParent(transform);

            int minimapLayer = LayerMask.NameToLayer("MiniMap");
            if (minimapLayer != -1)
            {
                SetLayerRecursively(iconInstance, minimapLayer);
            }
            else
            {
                Debug.LogWarning("MiniMap layer not found! Create 'MiniMap' layer in Project Settings.");
            }
        }
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    void LateUpdate()
    {
        if (iconInstance != null)
        {
            iconInstance.transform.position = transform.position + iconOffset;
            iconInstance.transform.rotation = Quaternion.identity;
        }
    }

    void OnDestroy()
    {
        if (iconInstance != null)
        {
            Destroy(iconInstance);
        }
    }
}
