using UnityEngine;

[System.Serializable]
public class AIVision
{
    private float range;
    private float angle;
    private LayerMask obstacles;
    private bool lastResult;
    
    public bool LastResult => lastResult;
    
    public AIVision(float visionRange, float visionAngle, LayerMask obstacleLayer)
    {
        range = visionRange;
        angle = visionAngle;
        obstacles = obstacleLayer;
    }
    
    public bool CanSeeTarget(Vector3 fromPosition, Vector3 targetPosition, Vector3 facingDirection)
    {
        float distance = Vector3.Distance(fromPosition, targetPosition);
        if (distance > range) return false;
        
        Vector3 directionToTarget = (targetPosition - fromPosition).normalized;
        float angleToTarget = Vector3.Angle(facingDirection, directionToTarget);
        if (angleToTarget > angle / 2f) return false;
        
        RaycastHit2D hit = Physics2D.Raycast(fromPosition, directionToTarget, distance, obstacles);
        
        bool canSee = hit.collider == null;
        
        if (Application.isPlaying && canSee)
        {
            Debug.DrawRay(fromPosition, directionToTarget * distance, Color.green, 0.1f);
        }
        else if (Application.isPlaying)
        {
            Debug.DrawRay(fromPosition, directionToTarget * distance, Color.red, 0.1f);
        }
        
        return canSee;
    }
    
    public void SetLastResult(bool result) => lastResult = result;
}