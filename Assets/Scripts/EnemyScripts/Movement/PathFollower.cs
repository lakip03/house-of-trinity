using UnityEngine;

public class PathFollower
{
    private readonly Transform transform;
    private readonly float stoppingDistance;
    private System.Collections.Generic.List<Vector3> currentPath;
    private int currentPathIndex = 0;
    
    public PathFollower(Transform enemyTransform, float stopDistance)
    {
        transform = enemyTransform;
        stoppingDistance = stopDistance;
    }
    
    public void SetPath(System.Collections.Generic.List<Vector3> newPath)
    {
        currentPath = newPath;
        currentPathIndex = 0;
    }
    
    public void FollowPath(float speed)
    {
        if (!HasValidPath()) return;
        
        Vector3 targetPos = currentPath[currentPathIndex];
        MoveTowards(targetPos, speed);
        
        if (HasReachedWaypoint(targetPos))
            AdvanceToNextWaypoint();
    }
    
    private bool HasValidPath() => 
        currentPath != null && currentPath.Count > 0 && currentPathIndex < currentPath.Count;
    
    private void MoveTowards(Vector3 target, float speed)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }
    
    private bool HasReachedWaypoint(Vector3 waypoint) => 
        Vector3.Distance(transform.position, waypoint) < 0.2f;
    
    private void AdvanceToNextWaypoint() => currentPathIndex++;
}
