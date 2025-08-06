using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    [Header("Behavior Settings")]
    public float loseTargetTime = 3f;
    public float patrolSpeed = 1.5f;
    public float normalSpeed = 3f;
    
    private EnemyState currentState = EnemyState.Idle;
    private Vector3 movementTarget;
    private float movementSpeed;
    
    public void Initialize()
    {
        currentState = EnemyState.Idle;
    }
    
    public void UpdateState(SensorData sensors)
    {
        EnemyState newState = DetermineNewState(sensors);
        
        if (newState != currentState)
        {
            OnStateExit(currentState);
            currentState = newState;
            OnStateEnter(currentState, sensors);
        }
        
        UpdateStateLogic(sensors);
        NotifySensors();
    }
    
    public EnemyState GetCurrentState() => currentState;
    public Vector3 GetMovementTarget() => movementTarget;
    public float GetMovementSpeed() => movementSpeed;
    
    private EnemyState DetermineNewState(SensorData sensors)
    {
        var transitionEvaluator = new StateTransitionEvaluator(currentState, sensors, loseTargetTime);
        return transitionEvaluator.EvaluateTransition();
    }
    
    private void OnStateEnter(EnemyState state, SensorData sensors)
    {
        switch (state)
        {
            case EnemyState.Chasing:
                movementSpeed = normalSpeed;
                break;
            case EnemyState.Investigating:
                movementSpeed = normalSpeed * 0.8f;
                break;
            case EnemyState.Searching:
                movementSpeed = patrolSpeed;
                break;
            case EnemyState.Idle:
                movementSpeed = 0f;
                break;
        }
    }
    
    private void OnStateExit(EnemyState state) { }
    
    private void UpdateStateLogic(SensorData sensors)
    {
        switch (currentState)
        {
            case EnemyState.Chasing:
                movementTarget = sensors.lastKnownPosition;
                break;
            case EnemyState.Investigating:
            case EnemyState.Searching:
                movementTarget = sensors.lastKnownPosition;
                break;
            case EnemyState.Idle:
                movementTarget = transform.position;
                break;
        }
    }
    
    private void NotifySensors()
    {
        GetComponent<EnemySensors>()?.SetState(currentState);
    }
}