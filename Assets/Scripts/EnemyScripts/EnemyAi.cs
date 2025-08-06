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
    public float loseTargetTime = 3f;
    public float rotationSpeed = 90f;
    
    private AIVision vision;
    private AIHearing hearing;  
    private AIMovement movement;
    private AIState currentState = AIState.Patrolling;
    
    private Vector3 lastKnownPosition;
    private float lastSeenTime;
    private float currentRotation;
    
    void Start()
    {
        vision = new AIVision(visionRange, 90f, obstacleLayer);
        hearing = new AIHearing(hearingRadius);
        movement = new AIMovement(FindAnyObjectByType<AStarPathfinding2D>());
        
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;
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
        bool canSee = vision.CanSeeTarget(transform.position, player.position, GetFacingDirection());
        
        bool canHear = hearing.CanHearTarget(transform.position, player.position);
        
        if (canSee || canHear)
        {
            lastKnownPosition = player.position;
            if (canSee) lastSeenTime = Time.time;
        }
        
        vision.SetLastResult(canSee);
        hearing.SetLastResult(canHear);
    }
    
    void UpdateState()
    {
        switch (currentState)
        {
            case AIState.Patrolling:
                if (vision.LastResult || hearing.LastResult)
                    currentState = AIState.Chasing;
                break;
                
            case AIState.Chasing:
                if (!vision.LastResult && !hearing.LastResult)
                {
                    if (Time.time - lastSeenTime > loseTargetTime)
                        currentState = AIState.Patrolling;
                    else
                        currentState = AIState.Searching;
                }
                break;
                
            case AIState.Searching:
                if (vision.LastResult || hearing.LastResult)
                    currentState = AIState.Chasing;
                else if (Time.time - lastSeenTime > loseTargetTime)
                    currentState = AIState.Patrolling;
                break;
        }
    }
    
    void UpdateMovement()
    {
        Vector3 targetPosition = transform.position;
        
        switch (currentState)
        {
            case AIState.Chasing:
                targetPosition = player.position;
                break;
            case AIState.Searching:
                targetPosition = lastKnownPosition;
                break;
            case AIState.Patrolling:
                break;
        }
        
        movement.MoveTowards(transform, targetPosition, moveSpeed);
    }
    
    void UpdateRotation()
    {
        Vector3 targetDirection = Vector3.zero;
        
        switch (currentState)
        {
            case AIState.Patrolling:
                // Radar sweep
                currentRotation += rotationSpeed * Time.deltaTime;
                if (currentRotation >= 360f) currentRotation -= 360f;
                break;
                
            case AIState.Chasing:
                // Look at player
                targetDirection = (player.position - transform.position).normalized;
                float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
                currentRotation = Mathf.MoveTowardsAngle(currentRotation, targetAngle, rotationSpeed * 2f * Time.deltaTime);
                break;
                
            case AIState.Searching:
                // Look towards last known position
                targetDirection = (lastKnownPosition - transform.position).normalized;
                float searchAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
                currentRotation = Mathf.MoveTowardsAngle(currentRotation, searchAngle, rotationSpeed * Time.deltaTime);
                break;
        }
    }
    
    Vector3 GetFacingDirection()
    {
        return new Vector3(Mathf.Cos(currentRotation * Mathf.Deg2Rad), 
                          Mathf.Sin(currentRotation * Mathf.Deg2Rad), 0f);
    }
    
    void OnDrawGizmos()
    {
        if (vision != null)
        {
            // Draw vision cone
            Gizmos.color = vision.LastResult ? Color.green : Color.yellow;
            Vector3 facing = GetFacingDirection();
            Gizmos.DrawRay(transform.position, facing * visionRange);
            
            // Draw vision range
            Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
            Gizmos.DrawSphere(transform.position, visionRange);
        }
        
        if (hearing != null)
        {
            // Draw hearing radius
            Gizmos.color = hearing.LastResult ? Color.blue : new Color(0.5f, 0.5f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, hearingRadius);
        }
        
        // Draw state indicator
        Gizmos.color = GetStateColor();
        Gizmos.DrawCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
        
        // Draw last known position when searching
        if (currentState == AIState.Searching)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(lastKnownPosition, 0.5f);
            Gizmos.DrawLine(transform.position, lastKnownPosition);
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
