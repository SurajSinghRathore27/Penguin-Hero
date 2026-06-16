using UnityEngine;
using UnityEngine.InputSystem;

public class CherryThrowController : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject cherryPrefab;
    public Transform throwPoint;
    public float minThrowForce = 2.5f;
    public float maxThrowForce = 12f;
    public float chargeRate = 4f;
    public float throwAngle = 40f;

    [Header("Cooldown Settings")]
    public float throwCooldown = 1f;

    [Header("References")]
    public Trajectory Trajectory;

    private float currentChargeTime;
    private bool isCharging;
    private float cooldownTimer;
    private PlayerMovement playerMovement;
    private PickupController pickupController;
    private PlayerAnimationController animationController;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        pickupController = GetComponent<PickupController>();
        animationController = GetComponent<PlayerAnimationController>();

        if (throwPoint == null)
        {
            GameObject throwPointObj = new GameObject("ThrowPoint");
            throwPointObj.transform.SetParent(transform);
            throwPointObj.transform.localPosition = new Vector3(0.5f, 0.5f, 0);
            throwPoint = throwPointObj.transform;
        }

        if (Trajectory == null)
        {
            Trajectory = GetComponent<Trajectory>();
        }
    }

    void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (isCharging)
        {
            currentChargeTime += Time.deltaTime * chargeRate;
            currentChargeTime = Mathf.Clamp(currentChargeTime, 0, maxThrowForce);

            float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, currentChargeTime / maxThrowForce);
            Vector2 throwVelocity = CalculateThrowVelocity(throwForce);

            if (Trajectory != null)
            {
                Trajectory.ShowTrajectory(throwPoint.position, throwVelocity, Physics2D.gravity.y);
            }
        }
    }

    public void OnThrow(InputAction.CallbackContext context)
    {
        if (pickupController != null && pickupController.IsHoldingObject())
        {
            return;
        }

        if (cooldownTimer > 0)
        {
            return;
        }

        if (context.started)
        {
            isCharging = true;
            currentChargeTime = 0f;
        }
        else if (context.canceled && isCharging)
        {
            ThrowCherry();
            isCharging = false;

            if (Trajectory != null)
            {
                Trajectory.HideTrajectory();
            }

            cooldownTimer = throwCooldown;

            if (animationController != null)
            {
                animationController.PlayAttackAnimation();
            }
        }
    }

    void ThrowCherry()
    {
        if (cherryPrefab == null)
        {
            Debug.LogWarning("Cherry prefab is not assigned!");
            return;
        }

        float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, currentChargeTime / maxThrowForce);
        Vector2 throwVelocity = CalculateThrowVelocity(throwForce);

        GameObject cherry = Instantiate(cherryPrefab, throwPoint.position, Quaternion.identity);

        CherryProjectile projectile = cherry.GetComponent<CherryProjectile>();
        if (projectile != null)
        {
            projectile.Launch(throwVelocity);
        }
    }

    Vector2 CalculateThrowVelocity(float force)
    {
        float direction = playerMovement != null && transform.localScale.x < 0 ? -1 : 1;

        float angleInRadians = throwAngle * Mathf.Deg2Rad;
        float vx = Mathf.Cos(angleInRadians) * force * direction;
        float vy = Mathf.Sin(angleInRadians) * force;

        return new Vector2(vx, vy);
    }

    public bool IsOnCooldown()
    {
        return cooldownTimer > 0;
    }

    public float GetCooldownProgress()
    {
        return 1f - (cooldownTimer / throwCooldown);
    }

    public bool IsCharging()
    {
        return isCharging;
    }

    public float GetChargeProgress()
    {
        return currentChargeTime / maxThrowForce;
    }
}
