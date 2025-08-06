using UnityEngine;

public class EnemySensors : MonoBehaviour
{
    [Header("Vision Settings")]
    public float visionRange = 10f;
    public float lockOnRange = 15f;
    [Range(0f, 360f)] public float visionAngle = 90f;
    public LayerMask obstacleLayerMask = 1;
    
    [Header("Hearing Settings")]
    public float hearingRadius = 15f;
    public float playerNoiseLevel = 1f;
    public bool hearingIgnoresWalls = true;
    
    [Header("Radar Settings")]
    public float radarRotationSpeed = 45f;
    public float rotationSpeed = 180f;
    
    private SensorData currentData;
    private float currentRotation = 0f;
    private EnemyState currentState = EnemyState.Idle;
    
    public void UpdateSensors(Vector3 playerPosition)
    {
        UpdateVision(playerPosition);
        UpdateHearing(playerPosition);
        UpdateRotation(playerPosition);
        UpdateTimers();
    }
    
    public SensorData GetSensorData() => currentData;
    public Vector3 GetFacingDirection() => new Vector3(Mathf.Cos(currentRotation * Mathf.Deg2Rad), 
                                                      Mathf.Sin(currentRotation * Mathf.Deg2Rad), 0);
    public void SetState(EnemyState state) => currentState = state;
    
    private void UpdateVision(Vector3 playerPosition)
    {
        var visionCheck = new VisionCheck(transform.position, playerPosition, GetCurrentVisionRange(), 
                                         visionAngle, GetFacingDirection(), obstacleLayerMask);
        
        bool previousCanSee = currentData.canSeePlayer;
        currentData.canSeePlayer = visionCheck.CanSeeTarget();
        
        if (currentData.canSeePlayer && !previousCanSee)
            UpdateLastKnownPosition(playerPosition);
    }
    
    private void UpdateHearing(Vector3 playerPosition)
    {
        var hearingCheck = new HearingCheck(transform.position, playerPosition, hearingRadius, 
                                          playerNoiseLevel, hearingIgnoresWalls, obstacleLayerMask);
        
        bool previousCanHear = currentData.canHearPlayer;
        currentData.canHearPlayer = hearingCheck.CanHearTarget();
        
        if (currentData.canHearPlayer && !previousCanHear)
            UpdateLastKnownPosition(playerPosition);
    }
    
    private void UpdateRotation(Vector3 playerPosition)
    {
        var rotationStrategy = RotationStrategyFactory.GetStrategy(currentState);
        currentRotation = rotationStrategy.UpdateRotation(currentRotation, transform.position, 
                                                         playerPosition, currentData.lastKnownPosition, 
                                                         radarRotationSpeed, rotationSpeed, Time.deltaTime);
    }
    
    private void UpdateTimers()
    {
        currentData.timeSinceLastSeen = currentData.canSeePlayer ? 0f : currentData.timeSinceLastSeen + Time.deltaTime;
        currentData.timeSinceLastHeard = currentData.canHearPlayer ? 0f : currentData.timeSinceLastHeard + Time.deltaTime;
    }
    
    private void UpdateLastKnownPosition(Vector3 position)
    {
        currentData.lastKnownPosition = position;
    }
    
    private float GetCurrentVisionRange() => 
        (currentState == EnemyState.Chasing || currentState == EnemyState.Searching) ? lockOnRange : visionRange;
}