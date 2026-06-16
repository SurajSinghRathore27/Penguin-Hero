using UnityEngine;
using UnityEngine.InputSystem;

public class PickupController : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float pickupRange = 1.5f;
    public Transform holdPoint;
    public LayerMask pickupLayer;

    [Header("Throw Settings")]
    public float minThrowForce = 3f;
    public float maxThrowForce = 10f;
    public float chargeRate = 3f;
    public float throwAngle = 35f;
    public float weightForceReduction = 0.3f;

    [Header("References")]
    public Trajectory Trajectory;

    private ThrowableObject heldObject;
    private CherryThrowController cherryController;
    private PlayerMovement playerMovement;
    private PlayerAnimationController animationController;
    private bool isChargingThrow;
    private float currentChargeTime;

    void Start()
    {
        cherryController = GetComponent<CherryThrowController>();
        playerMovement = GetComponent<PlayerMovement>();
        animationController = GetComponent<PlayerAnimationController>();
        Trajectory = GetComponent<Trajectory>();

        if (holdPoint == null)
        {
            GameObject holdPointObj = new GameObject("HoldPoint");
            holdPointObj.transform.SetParent(transform);
            holdPointObj.transform.localPosition = new Vector3(0, 0.5f, 0);
            holdPoint = holdPointObj.transform;
        }
    }

    void Update()
    {
        if (isChargingThrow && heldObject != null)
        {
            currentChargeTime += Time.deltaTime * chargeRate;
            currentChargeTime = Mathf.Clamp(currentChargeTime, 0, maxThrowForce);

            float throwForce = CalculateThrowForce();
            Vector2 throwVelocity = CalculateThrowVelocity(throwForce);

            if (Trajectory != null)
            {
                Vector3 startPos = heldObject.GetVisualCenter();
                Trajectory.ShowTrajectory(startPos, throwVelocity, Physics2D.gravity.y);
            }
        }
    }

    public void OnPickupThrow(InputAction.CallbackContext context)
    {
        if (heldObject == null)
        {
            if (context.performed)
            {
                TryPickupObject();
            }
        }
        else
        {
            if (context.started)
            {
                isChargingThrow = true;
                currentChargeTime = 0f;
            }
            else if (context.canceled && isChargingThrow)
            {
                ThrowObject();
                isChargingThrow = false;

                if (Trajectory != null)
                {
                    Trajectory.HideTrajectory();
                }

                if (animationController != null)
                {
                    animationController.PlayAttackAnimation();
                }
            }
        }
    }

    void TryPickupObject()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, pickupRange, pickupLayer);

        ThrowableObject closestObject = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D col in colliders)
        {
            ThrowableObject throwable = col.GetComponent<ThrowableObject>();
            if (throwable != null && !throwable.IsPickedUp())
            {
                float distance = Vector2.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestObject = throwable;
                }
            }
        }

        if (closestObject != null)
        {
            PickupObject(closestObject);
        }
    }

    void PickupObject(ThrowableObject obj)
    {
        heldObject = obj;
        heldObject.PickUp(holdPoint);

        if (cherryController != null)
        {
            cherryController.enabled = false;
        }
    }

    void ThrowObject()
    {
        if (heldObject == null) return;

        float throwForce = CalculateThrowForce();
        Vector2 throwVelocity = CalculateThrowVelocity(throwForce);

        heldObject.Throw(throwVelocity);
        heldObject = null;

        if (cherryController != null)
        {
            cherryController.enabled = true;
        }
    }

    float CalculateThrowForce()
    {
        float baseForce = Mathf.Lerp(minThrowForce, maxThrowForce, currentChargeTime / maxThrowForce);

        if (heldObject != null)
        {
            float weight = heldObject.GetWeight();
            float weightPenalty = (weight - 1f) * weightForceReduction;
            baseForce = Mathf.Max(minThrowForce, baseForce - weightPenalty);
        }

        return baseForce;
    }

    Vector2 CalculateThrowVelocity(float force)
    {
        float direction = playerMovement != null && transform.localScale.x < 0 ? -1 : 1;

        float angleInRadians = throwAngle * Mathf.Deg2Rad;
        float vx = Mathf.Cos(angleInRadians) * force * direction;
        float vy = Mathf.Sin(angleInRadians) * force;

        return new Vector2(vx, vy);
    }

    public bool IsHoldingObject()
    {
        return heldObject != null;
    }

    public bool IsChargingThrow()
    {
        return isChargingThrow;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}
