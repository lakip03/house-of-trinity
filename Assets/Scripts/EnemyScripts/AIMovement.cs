
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AIMovement
{
    private AStarPathfinding2D pathfinder;
    private List<Vector3> currentPath;
    private int pathIndex;
    private float lastPathUpdate;
    
    public AIMovement(AStarPathfinding2D pathfindingSystem)
    {
        pathfinder = pathfindingSystem;
    }
    
    public void MoveTowards(Transform enemyTransform, Vector3 targetPosition, float speed)
    {
        if (Time.time - lastPathUpdate > 0.5f)
        {
            if (pathfinder != null)
                currentPath = pathfinder.FindPath(enemyTransform.position, targetPosition);
            pathIndex = 0;
            lastPathUpdate = Time.time;
        }
        
        if (currentPath != null && pathIndex < currentPath.Count)
        {
            Vector3 waypoint = currentPath[pathIndex];
            enemyTransform.position = Vector3.MoveTowards(enemyTransform.position, waypoint, speed * Time.deltaTime);
            
            if (Vector3.Distance(enemyTransform.position, waypoint) < 0.2f)
                pathIndex++;
        }
    }
}