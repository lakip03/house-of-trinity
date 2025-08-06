using UnityEngine;

public readonly struct StateTransitionEvaluator
{
    private readonly EnemyState currentState;
    private readonly SensorData sensors;
    private readonly float loseTargetTime;
    
    public StateTransitionEvaluator(EnemyState state, SensorData sensorData, float loseTime)
    {
        currentState = state;
        sensors = sensorData;
        loseTargetTime = loseTime;
    }
    
    public EnemyState EvaluateTransition()
    {
        return currentState switch
        {
            EnemyState.Idle => EvaluateFromIdle(),
            EnemyState.Chasing => EvaluateFromChasing(),
            EnemyState.Searching => EvaluateFromSearching(),
            EnemyState.Investigating => EvaluateFromInvestigating(),
            _ => EnemyState.Idle
        };
    }
    
    private EnemyState EvaluateFromIdle()
    {
        if (sensors.canSeePlayer) return EnemyState.Chasing;
        if (sensors.canHearPlayer) return EnemyState.Investigating;
        return EnemyState.Idle;
    }
    
    private EnemyState EvaluateFromChasing()
    {
        if (sensors.canSeePlayer || sensors.canHearPlayer) return EnemyState.Chasing;
        if (HasLostTarget()) return EnemyState.Idle;
        return EnemyState.Searching;
    }
    
    private EnemyState EvaluateFromSearching()
    {
        if (sensors.canSeePlayer) return EnemyState.Chasing;
        if (sensors.canHearPlayer) return EnemyState.Investigating;
        if (HasLostTarget()) return EnemyState.Idle;
        return EnemyState.Searching;
    }
    
    private EnemyState EvaluateFromInvestigating()
    {
        if (sensors.canSeePlayer) return EnemyState.Chasing;
        if (!sensors.canHearPlayer && sensors.timeSinceLastHeard > loseTargetTime * 0.5f) 
            return EnemyState.Searching;
        return EnemyState.Investigating;
    }
    
    private bool HasLostTarget() => 
        Mathf.Max(sensors.timeSinceLastSeen, sensors.timeSinceLastHeard) > loseTargetTime;
}