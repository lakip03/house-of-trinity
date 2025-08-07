using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Basic Settings")]
    public Transform player;
    public float moveSpeed = 3f;
    public float visionRange = 10f;
    public float hearingRadius = 15f;
    public LayerMask obstacleLayer = 1;
    
    [Header("AI Behavior")]
    public float loseTargetTime = 5f;
    public float rotationSpeed = 90f;
    public float visionAngle = 90f;
    public float searchRadius = 3f; 
    
    private AIVision vision;
    private AIHearing hearing;  
    private AIMovement movement;
    private AIState currentState = AIState.Patrolling;
    
    private Vector3 lastKnownPosition;
    private Vector3 searchTarget; 
    private float lastSeenTime;
    private float currentRotation;
    private float searchStartTime;
    private bool reachedLastKnownPosition;
    private int searchDirection = 1; 
    
    void Start()
    {
        vision = new AIVision(visionRange, visionAngle, obstacleLayer);
        hearing = new AIHearing(hearingRadius);
        movement = new AIMovement(FindAnyObjectByType<AStarPathfinding2D>());
        
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;
            
        lastKnownPosition = transform.position;
        searchTarget = transform.position;
    }
    
    void Update()
    {
        if (player == null) return;
        
        UpdateSensors();
        UpdateState();
        UpdateMovement();
        UpdateRotation();
    }
    
    void UpdateSensors()
    {
        Vector3 facingDirection = GetFacingDirection();
        bool canSee = vision.CanSeeTarget(transform.position, player.position, facingDirection);
        bool canHear = hearing.CanHearTarget(transform.position, player.position);
        
        if (canSee || canHear)
        {
            lastKnownPosition = player.position;
            reachedLastKnownPosition = false;
            if (canSee) lastSeenTime = Time.time;
        }
        
        vision.SetLastResult(canSee);
        hearing.SetLastResult(canHear);
    }
    
    void UpdateState()
    {
        bool canDetectPlayer = vision.LastResult || hearing.LastResult;
        
        switch (currentState)
        {
            case AIState.Patrolling:
                if (canDetectPlayer)
                {
                    currentState = AIState.Chasing;
                    searchTarget = lastKnownPosition;
                }
                break;
                
            case AIState.Chasing:
                if (canDetectPlayer)
                {
                    searchTarget = player.position;
                }
                else
                {
                    currentState = AIState.Searching;
                    searchTarget = lastKnownPosition;
                    searchStartTime = Time.time;
                    reachedLastKnownPosition = false;
                }
                break;
                
            case AIState.Searching:
                if (canDetectPlayer)
                {
                    currentState = AIState.Chasing;
                    searchTarget = player.position;
                }
                else if (Time.time - lastSeenTime > loseTargetTime)
                {
                    currentState = AIState.Patrolling;
                }
                else
                {
                    UpdateSearchBehavior();
                }
                break;
        }
    }
    
    void UpdateSearchBehavior()
    {
        float distanceToTarget = Vector3.Distance(transform.position, searchTarget);
        
        if (distanceToTarget < 0.5f)
        {
            if (!reachedLastKnownPosition)
            {
                reachedLastKnownPosition = true;
                searchStartTime = Time.time;
                SetNewSearchTarget();
            }
            else
            {
                searchDirection *= -1;
                SetNewSearchTarget();
            }
        }
        
        Vector3 directionToTarget = (searchTarget - transform.position).normalized;
        RaycastHit2D wallCheck = Physics2D.Raycast(transform.position, directionToTarget, 0.8f, obstacleLayer);
        
        if (wallCheck.collider != null)
        {
            searchDirection *= -1;
            SetNewSearchTarget();
        }
    }
    
    void SetNewSearchTarget()
    {
        Vector3 baseDirection = (lastKnownPosition - transform.position).normalized;
        
        if (Vector3.Distance(transform.position, lastKnownPosition) < 1f)
        {
            float angle = 90f * searchDirection; // 90 degrees left or right
            baseDirection = Quaternion.Euler(0, 0, angle) * GetFacingDirection();
        }
        
        searchTarget = transform.position + baseDirection * searchRadius;
        
        RaycastHit2D hit = Physics2D.Raycast(transform.position, baseDirection, searchRadius, obstacleLayer);
        if (hit.collider != null)
        {
            searchTarget = (Vector3)hit.point - baseDirection * 0.5f;
        }
    }
    
    void UpdateMovement()
    {
        switch (currentState)
        {
            case AIState.Chasing:
                movement.MoveTowards(transform, searchTarget, moveSpeed);
                break;
                
            case AIState.Searching:
                movement.MoveTowards(transform, searchTarget, moveSpeed * 0.8f);
                break;
                
            case AIState.Patrolling:
                break;
        }
    }
    
    void UpdateRotation()
    {
        switch (currentState)
        {
            case AIState.Patrolling:
                currentRotation += rotationSpeed * Time.deltaTime;
                if (currentRotation >= 360f) currentRotation -= 360f;
                break;
                
            case AIState.Chasing:
                Vector3 chaseDirection = (searchTarget - transform.position).normalized;
                if (chaseDirection != Vector3.zero)
                {
                    float targetAngle = Mathf.Atan2(chaseDirection.y, chaseDirection.x) * Mathf.Rad2Deg;
                    currentRotation = Mathf.MoveTowardsAngle(currentRotation, targetAngle, rotationSpeed * 3f * Time.deltaTime);
                }
                break;
                
            case AIState.Searching:
                if (reachedLastKnownPosition)
                {
                    Vector3 lookDirection = (searchTarget - transform.position).normalized;
                    if (lookDirection != Vector3.zero)
                    {
                        float searchAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
                        currentRotation = Mathf.MoveTowardsAngle(currentRotation, searchAngle, rotationSpeed * 2f * Time.deltaTime);
                    }
                }
                else
                {
                    Vector3 moveDirection = (lastKnownPosition - transform.position).normalized;
                    if (moveDirection != Vector3.zero)
                    {
                        float targetAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                        currentRotation = Mathf.MoveTowardsAngle(currentRotation, targetAngle, rotationSpeed * 2f * Time.deltaTime);
                    }
                }
                break;
        }
        
        transform.rotation = Quaternion.Euler(0, 0, currentRotation);
    }
    
    Vector3 GetFacingDirection()
    {
        return new Vector3(Mathf.Cos(currentRotation * Mathf.Deg2Rad), 
                          Mathf.Sin(currentRotation * Mathf.Deg2Rad), 0f);
    }
    
    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Vector3 facingDir = GetFacingDirection();
            
            if (vision != null)
            {
                Gizmos.color = vision.LastResult ? Color.green : Color.yellow;
                
                Gizmos.DrawRay(transform.position, facingDir * visionRange);
                
                float halfAngle = visionAngle * 0.5f;
                Vector3 leftEdge = Quaternion.Euler(0, 0, halfAngle) * facingDir;
                Vector3 rightEdge = Quaternion.Euler(0, 0, -halfAngle) * facingDir;
                
                Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
                Gizmos.DrawRay(transform.position, leftEdge * visionRange);
                Gizmos.DrawRay(transform.position, rightEdge * visionRange);
            }
            
            if (hearing != null)
            {
                Gizmos.color = hearing.LastResult ? Color.blue : new Color(0.5f, 0.5f, 1f, 0.2f);
                Gizmos.DrawWireSphere(transform.position, hearingRadius);
            }
            
            Gizmos.color = GetStateColor();
            Gizmos.DrawCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(lastKnownPosition, 0.8f);
            
            if (currentState == AIState.Searching || currentState == AIState.Chasing)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(searchTarget, 0.4f);
                Gizmos.DrawLine(transform.position, searchTarget);
            }
            
            if (currentState == AIState.Searching && reachedLastKnownPosition)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
                Gizmos.DrawWireSphere(lastKnownPosition, searchRadius);
            }
        }
    }
    
    Color GetStateColor()
    {
        return currentState switch
        {
            AIState.Patrolling => Color.white,
            AIState.Chasing => Color.red,
            AIState.Searching => Color.orange,
            _ => Color.gray
        };
    }
}