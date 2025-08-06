using UnityEngine;

public readonly struct VisionCheck
{
    private readonly Vector3 position;
    private readonly Vector3 targetPosition;
    private readonly float range;
    private readonly float angle;
    private readonly Vector3 facingDirection;
    private readonly LayerMask obstacleMask;
    
    public VisionCheck(Vector3 pos, Vector3 target, float visionRange, float visionAngle, 
                      Vector3 facing, LayerMask obstacles)
    {
        position = pos;
        targetPosition = target;
        range = visionRange;
        angle = visionAngle;
        facingDirection = facing;
        obstacleMask = obstacles;
    }
    
    public bool CanSeeTarget()
    {
        return IsInRange() && IsInAngle() && HasClearLineOfSight();
    }
    
    private bool IsInRange() => Vector3.Distance(position, targetPosition) <= range;
    
    private bool IsInAngle()
    {
        Vector3 directionToTarget = (targetPosition - position).normalized;
        return Vector3.Angle(facingDirection, directionToTarget) <= angle / 2f;
    }
    
    private bool HasClearLineOfSight()
    {
        Vector3 direction = (targetPosition - position).normalized;
        float distance = Vector3.Distance(position, targetPosition);
        return Physics2D.Raycast(position, direction, distance, obstacleMask).collider == null;
    }
}