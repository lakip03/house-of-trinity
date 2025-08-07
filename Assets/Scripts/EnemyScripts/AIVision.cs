
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
        return hit.collider == null;
    }
    
    public void SetLastResult(bool result) => lastResult = result;
}

