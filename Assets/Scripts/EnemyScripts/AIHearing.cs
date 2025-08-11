using UnityEngine;

/// <summary>
/// Handles AI hearing detection for enemy AI systems.
/// Determines if a target is within hearing range based on distance calculations.
/// </summary>
[System.Serializable]
public class AIHearing
{
    private float radius;
    private bool lastResult;
    public bool LastResult => lastResult;
    
    /// <summary>
    /// Initializes a new instance of the AIHearing class with the specified hearing radius.
    /// </summary>
    /// <param name="hearingRadius">The maximum distance at which the AI can detect targets through hearing.</param>
    public AIHearing(float hearingRadius)
    {
        radius = hearingRadius;
    }
    
    /// <summary>
    /// Determines if a target at the specified position can be heard from the AI's position.
    /// Uses simple distance calculation without considering obstacles or line-of-sight.
    /// </summary>
    /// <param name="fromPosition">The position of the AI entity doing the hearing check.</param>
    /// <param name="targetPosition">The position of the target to check for.</param>
    /// <returns>True if the target is within hearing range, false otherwise.</returns>
    public bool CanHearTarget(Vector3 fromPosition, Vector3 targetPosition)
    {
        float distance = Vector3.Distance(fromPosition, targetPosition);
        return distance <= radius;
    }
    
    /// <summary>
    /// Sets the result of the most recent hearing detection check.
    /// This is typically called by the AI system to cache the hearing state.
    /// </summary>
    /// <param name="result">True if the target was heard, false otherwise.</param>
    public void SetLastResult(bool result) => lastResult = result;
}