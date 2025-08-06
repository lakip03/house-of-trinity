using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float stoppingDistance = 0.5f;
    public float pathUpdateRate = 0.5f;
    
    private AStarPathfinding2D pathfinder;
    private PathFollower pathFollower;
    private float lastPathUpdate = 0f;
    
    public void Initialize(AStarPathfinding2D pathfindingSystem)
    {
        pathfinder = pathfindingSystem;
        pathFollower = new PathFollower(transform, stoppingDistance);
    }
    
    public void UpdateMovement(Vector3 target, float speed)
    {
        if (ShouldUpdatePath(target))
        {
            UpdatePath(target);
        }
        
        pathFollower.FollowPath(speed);
    }
    
    private bool ShouldUpdatePath(Vector3 target) => 
        Time.time - lastPathUpdate > pathUpdateRate && 
        Vector3.Distance(transform.position, target) > stoppingDistance;
    
    private void UpdatePath(Vector3 target)
    {
        var newPath = pathfinder?.FindPath(transform.position, target);
        pathFollower.SetPath(newPath);
        lastPathUpdate = Time.time;
    }
}
