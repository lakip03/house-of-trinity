using UnityEngine;

public readonly struct HearingCheck
{
    private readonly Vector3 position;
    private readonly Vector3 targetPosition;
    private readonly float radius;
    private readonly float noiseLevel;
    private readonly bool ignoresWalls;
    private readonly LayerMask obstacleMask;
    
    public HearingCheck(Vector3 pos, Vector3 target, float hearingRadius, float noise, 
                       bool throughWalls, LayerMask obstacles)
    {
        position = pos;
        targetPosition = target;
        radius = hearingRadius;
        noiseLevel = noise;
        ignoresWalls = throughWalls;
        obstacleMask = obstacles;
    }
    
    public bool CanHearTarget()
    {
        float distance = Vector3.Distance(position, targetPosition);
        if (distance > radius) return false;
        
        float effectiveness = CalculateHearingEffectiveness(distance);
        return ignoresWalls ? effectiveness > 0.1f : IsAudibleThroughWalls(effectiveness);
    }
    
    private float CalculateHearingEffectiveness(float distance) => 
        noiseLevel * (1f - (distance / radius));
    
    private bool IsAudibleThroughWalls(float effectiveness)
    {
        Vector3 direction = (targetPosition - position).normalized;
        float distance = Vector3.Distance(position, targetPosition);
        bool hasObstacles = Physics2D.Raycast(position, direction, distance, obstacleMask).collider != null;
        
        return hasObstacles ? effectiveness > 0.5f : effectiveness > 0.1f;
    }
}