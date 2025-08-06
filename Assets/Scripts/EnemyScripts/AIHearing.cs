using UnityEngine;

[System.Serializable]
public class AIHearing
{
    private float radius;
    private bool lastResult;
    
    public bool LastResult => lastResult;
    
    public AIHearing(float hearingRadius)
    {
        radius = hearingRadius;
    }
    
    public bool CanHearTarget(Vector3 fromPosition, Vector3 targetPosition)
    {
        float distance = Vector3.Distance(fromPosition, targetPosition);
        return distance <= radius;
    }
    
    public void SetLastResult(bool result) => lastResult = result;
}

