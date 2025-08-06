using UnityEngine;

public static class RotationStrategyFactory
{
    public static IRotationStrategy GetStrategy(EnemyState state)
    {
        return state switch
        {
            EnemyState.Idle => new RadarRotationStrategy(),
            EnemyState.Chasing => new LockOnRotationStrategy(),
            EnemyState.Searching => new SlowRotationStrategy(0.5f),
            EnemyState.Investigating => new SlowRotationStrategy(0.75f),
            _ => new RadarRotationStrategy()
        };
    }
}

public interface IRotationStrategy
{
    float UpdateRotation(float currentRotation, Vector3 position, Vector3 playerPos, 
                        Vector3 lastKnownPos, float radarSpeed, float normalSpeed, float deltaTime);
}

public class RadarRotationStrategy : IRotationStrategy
{
    public float UpdateRotation(float currentRotation, Vector3 position, Vector3 playerPos, 
                               Vector3 lastKnownPos, float radarSpeed, float normalSpeed, float deltaTime)
    {
        currentRotation += radarSpeed * deltaTime;
        return currentRotation >= 360f ? currentRotation - 360f : currentRotation;
    }
}

public class LockOnRotationStrategy : IRotationStrategy
{
    public float UpdateRotation(float currentRotation, Vector3 position, Vector3 playerPos, 
                               Vector3 lastKnownPos, float radarSpeed, float normalSpeed, float deltaTime)
    {
        Vector3 direction = (playerPos - position).normalized;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return Mathf.MoveTowardsAngle(currentRotation, targetAngle, normalSpeed * deltaTime);
    }
}

public class SlowRotationStrategy : IRotationStrategy
{
    private readonly float speedMultiplier;
    
    public SlowRotationStrategy(float multiplier) => speedMultiplier = multiplier;
    
    public float UpdateRotation(float currentRotation, Vector3 position, Vector3 playerPos, 
                               Vector3 lastKnownPos, float radarSpeed, float normalSpeed, float deltaTime)
    {
        Vector3 direction = (lastKnownPos - position).normalized;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return Mathf.MoveTowardsAngle(currentRotation, targetAngle, normalSpeed * speedMultiplier * deltaTime);
    }
}