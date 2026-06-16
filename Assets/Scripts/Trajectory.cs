using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trajectory : MonoBehaviour
{
    [Header("Trajectory Settings")]
    public int lineSegments = 30;
    public float timeStep = 0.1f;

    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = lineSegments;
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.enabled = false;
        }
    }

    public void ShowTrajectory(Vector2 startPosition, Vector2 velocity, float gravity)
    {
        if (lineRenderer == null) return;

        lineRenderer.enabled = true;
        lineRenderer.positionCount = lineSegments;

        Vector2 position = startPosition;
        Vector2 currentVelocity = velocity;

        for (int i = 0; i < lineSegments; i++)
        {
            lineRenderer.SetPosition(i, position);

            position += currentVelocity * timeStep;
            currentVelocity.y += gravity * timeStep;

            if (Physics2D.Raycast(lineRenderer.GetPosition(i), (Vector2)lineRenderer.GetPosition(Mathf.Max(0, i - 1)) - (Vector2)lineRenderer.GetPosition(i), 0.1f))
            {
                lineRenderer.positionCount = i + 1;
                break;
            }
        }
    }

    public void HideTrajectory()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }
}
