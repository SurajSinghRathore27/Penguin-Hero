using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBanditAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float patrolSpeed = 2f;
    public float patrolDistance = 5f;
    public float patrolWaitTime = 1f;

    [Header("Chase Settings")]
    public float chaseSpeed = 4f;
    public float detectionRange = 6f;
    public float attackRange = 1.5f;

    [Header("Combat Settings")]
    public float attackCooldown = 1.5f;
    public float attackCooldownWhenClose = 0.8f;
    public float attackDamage = 10f;
    public float attackDuration = 0.5f;

    [Header("Jump Settings")]
    public float jumpForce = 10f;
    public float jumpCooldown = 0.8f;
    public float wallCheckDistance = 1.2f;
    public float ledgeCheckDistance = 2.5f;
    public float ledgeCheckOffset = 1.0f;
    public float obstacleCheckHeight = 1.5f;
    public float stuckCheckTime = 1.5f;

    [Header("References")]
    public Transform playerTransform;
    public LayerMask groundLayer;
    public LayerMask wallLayer;

    private Animator animator;
    private Rigidbody2D rb;
    private Sensor_Bandit groundSensor;
    private bool isGrounded;
    private BoxCollider2D boxCollider;

    private Vector2 patrolPointA;
    private Vector2 patrolPointB;
    private Vector2 currentPatrolTarget;
    private float waitTimer;
    private bool isWaiting;
    private float attackTimer;
    private float attackAnimationTimer;
    private bool hasDealtDamage;
    private float jumpTimer;
    private bool canJump = true;
    private int currentDirection = 1;

    private Vector2 lastPosition;
    private float stuckTimer;
    private float movementCheckTimer;

    private enum EnemyState { Patrol, Chase, Attack, CombatIdle }
    private EnemyState currentState = EnemyState.Patrol;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();

        Transform groundSensorTransform = transform.Find("GroundSensor");
        if (groundSensorTransform != null)
        {
            groundSensor = groundSensorTransform.GetComponent<Sensor_Bandit>();
        }

        patrolPointA = new Vector2(transform.position.x - patrolDistance, transform.position.y);
        patrolPointB = new Vector2(transform.position.x + patrolDistance, transform.position.y);
        currentPatrolTarget = patrolPointB;
        currentDirection = 1;
        lastPosition = transform.position;

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        if (wallLayer == 0)
        {
            wallLayer = LayerMask.GetMask("Ground", "Wall");
        }

        if (groundLayer == 0)
        {
            groundLayer = LayerMask.GetMask("Ground");
        }
    }

    void Update()
    {
        CheckGroundStatus();
        CheckIfStuckAndJump();

        if (jumpTimer > 0)
        {
            jumpTimer -= Time.deltaTime;
            if (jumpTimer <= 0)
            {
                canJump = true;
            }
        }

        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
            }

            if (attackAnimationTimer > 0)
            {
                attackAnimationTimer -= Time.deltaTime;
            }

            switch (currentState)
            {
                case EnemyState.Patrol:
                    HandlePatrol(distanceToPlayer);
                    break;

                case EnemyState.Chase:
                    HandleChase(distanceToPlayer);
                    break;

                case EnemyState.Attack:
                    HandleAttack(distanceToPlayer);
                    break;

                case EnemyState.CombatIdle:
                    HandleCombatIdle(distanceToPlayer);
                    break;
            }
        }
        else
        {
            HandlePatrol(999f);
        }

        UpdateAnimations();
    }

    void CheckGroundStatus()
    {
        if (groundSensor != null)
        {
            bool wasGrounded = isGrounded;
            isGrounded = groundSensor.State();

            if (!wasGrounded && isGrounded)
            {
                canJump = true;
                if (animator != null)
                {
                    animator.SetBool("Grounded", true);
                }
            }

            if (wasGrounded && !isGrounded)
            {
                if (animator != null)
                {
                    animator.SetBool("Grounded", false);
                }
            }
        }
    }

    void CheckIfStuckAndJump()
    {
        movementCheckTimer += Time.deltaTime;

        if (movementCheckTimer >= 0.5f)
        {
            float distanceMoved = Vector2.Distance(transform.position, lastPosition);

            if (distanceMoved < 0.2f && Mathf.Abs(rb.velocity.x) > 0.5f && isGrounded)
            {
                stuckTimer += movementCheckTimer;

                if (stuckTimer >= stuckCheckTime)
                {
                    ForceJump();
                    stuckTimer = 0f;
                }
            }
            else
            {
                stuckTimer = 0f;
            }

            lastPosition = transform.position;
            movementCheckTimer = 0f;
        }
    }

    bool IsWallAhead()
    {
        if (boxCollider == null) return false;

        Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * 0.5f;
        Vector2 direction = Vector2.right * currentDirection;

        RaycastHit2D wallHit = Physics2D.Raycast(
            rayOrigin,
            direction,
            wallCheckDistance,
            wallLayer
        );

        return wallHit.collider != null;
    }

    bool IsLedgeAhead()
    {
        if (!isGrounded || boxCollider == null) return false;

        float colliderWidth = boxCollider.bounds.extents.x;
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.right * currentDirection * (colliderWidth + ledgeCheckOffset);

        RaycastHit2D ledgeHit = Physics2D.Raycast(
            rayOrigin,
            Vector2.down,
            ledgeCheckDistance,
            groundLayer
        );

        return ledgeHit.collider == null;
    }

    bool CanJumpOverObstacle()
    {
        if (!isGrounded || !canJump) return false;

        Vector2 direction = Vector2.right * currentDirection;

        RaycastHit2D lowHit = Physics2D.Raycast(
            transform.position,
            direction,
            wallCheckDistance,
            wallLayer
        );

        RaycastHit2D highHit = Physics2D.Raycast(
            transform.position + Vector3.up * obstacleCheckHeight,
            direction,
            wallCheckDistance,
            wallLayer
        );

        return lowHit.collider != null && highHit.collider == null;
    }

    void TryJump()
    {
        if (isGrounded && canJump && jumpTimer <= 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            canJump = false;
            jumpTimer = jumpCooldown;

            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }
        }
    }

    void ForceJump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            canJump = false;
            jumpTimer = jumpCooldown * 0.5f;

            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }
        }
    }

    void SwitchPatrolDirection()
    {
        currentDirection *= -1;

        if (currentDirection > 0)
        {
            currentPatrolTarget = patrolPointB;
        }
        else
        {
            currentPatrolTarget = patrolPointA;
        }

        isWaiting = true;
        waitTimer = patrolWaitTime;
    }

    void HandlePatrol(float distanceToPlayer)
    {
        if (playerTransform != null && distanceToPlayer <= detectionRange)
        {
            currentState = EnemyState.Chase;
            isWaiting = false;
            return;
        }

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            rb.velocity = new Vector2(0, rb.velocity.y);

            if (waitTimer <= 0)
            {
                isWaiting = false;
            }
            return;
        }

        bool wallAhead = IsWallAhead();
        bool ledgeAhead = IsLedgeAhead();

        if (wallAhead)
        {
            if (CanJumpOverObstacle())
            {
                TryJump();
            }
            else
            {
                SwitchPatrolDirection();
                return;
            }
        }

        if (ledgeAhead)
        {
            SwitchPatrolDirection();
            return;
        }

        float distanceToTarget = Mathf.Abs(transform.position.x - currentPatrolTarget.x);

        if (distanceToTarget <= 0.3f)
        {
            SwitchPatrolDirection();
            return;
        }

        rb.velocity = new Vector2(currentDirection * patrolSpeed, rb.velocity.y);
        FlipSprite(currentDirection);
    }

    void HandleChase(float distanceToPlayer)
    {
        if (distanceToPlayer > detectionRange * 1.5f)
        {
            currentState = EnemyState.Patrol;
            return;
        }

        if (distanceToPlayer <= attackRange)
        {
            currentState = EnemyState.CombatIdle;
            return;
        }

        float direction = Mathf.Sign(playerTransform.position.x - transform.position.x);
        currentDirection = (int)direction;

        bool shouldJump = false;

        if (IsWallAhead() && CanJumpOverObstacle())
        {
            shouldJump = true;
        }

        float heightDifference = playerTransform.position.y - transform.position.y;
        float horizontalDistance = Mathf.Abs(playerTransform.position.x - transform.position.x);

        if (heightDifference > 1.2f && horizontalDistance < 4f && isGrounded)
        {
            shouldJump = true;
        }

        if (shouldJump)
        {
            TryJump();
        }

        rb.velocity = new Vector2(currentDirection * chaseSpeed, rb.velocity.y);
        FlipSprite(currentDirection);
    }

    void HandleAttack(float distanceToPlayer)
    {
        rb.velocity = new Vector2(0, rb.velocity.y);

        if (playerTransform != null)
        {
            float direction = Mathf.Sign(playerTransform.position.x - transform.position.x);
            FlipSprite(direction);
        }

        if (!hasDealtDamage && attackAnimationTimer <= attackDuration * 0.5f && attackAnimationTimer > 0)
        {
            DealDamageToPlayer();
            hasDealtDamage = true;
        }

        if (attackAnimationTimer <= 0)
        {
            currentState = EnemyState.CombatIdle;
        }
    }

    void HandleCombatIdle(float distanceToPlayer)
    {
        rb.velocity = new Vector2(0, rb.velocity.y);

        if (playerTransform != null)
        {
            float direction = Mathf.Sign(playerTransform.position.x - transform.position.x);
            FlipSprite(direction);
        }

        if (distanceToPlayer > detectionRange * 1.5f)
        {
            currentState = EnemyState.Patrol;
        }
        else if (distanceToPlayer > attackRange * 1.2f)
        {
            currentState = EnemyState.Chase;
        }
        else if (distanceToPlayer <= attackRange && attackTimer <= 0)
        {
            StartAttack(distanceToPlayer);
        }
    }

    void StartAttack(float distanceToPlayer)
    {
        currentState = EnemyState.Attack;
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        float cooldown = distanceToPlayer <= attackRange * 0.7f ? attackCooldownWhenClose : attackCooldown;
        attackTimer = cooldown;
        attackAnimationTimer = attackDuration;
        hasDealtDamage = false;
    }

    void DealDamageToPlayer()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRange)
        {
            Health playerHealth = playerTransform.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        float speedMagnitude = Mathf.Abs(rb.velocity.x);

        if (currentState == EnemyState.Attack)
        {
            animator.SetInteger("AnimState", 1);
        }
        else if (currentState == EnemyState.CombatIdle)
        {
            animator.SetInteger("AnimState", 1);
        }
        else if (speedMagnitude > 0.1f)
        {
            animator.SetInteger("AnimState", 2);
        }
        else
        {
            animator.SetInteger("AnimState", 0);
        }

        animator.SetFloat("AirSpeed", rb.velocity.y);
    }

    void FlipSprite(float direction)
    {
        if (direction > 0)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else if (direction < 0)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;

        Vector2 pointA;
        Vector2 pointB;

        if (Application.isPlaying)
        {
            pointA = patrolPointA;
            pointB = patrolPointB;
        }
        else
        {
            pointA = new Vector2(transform.position.x - patrolDistance, transform.position.y);
            pointB = new Vector2(transform.position.x + patrolDistance, transform.position.y);
        }

        Gizmos.DrawLine(pointA, pointB);
        Gizmos.DrawWireSphere(pointA, 0.3f);
        Gizmos.DrawWireSphere(pointB, 0.3f);
    }
}
