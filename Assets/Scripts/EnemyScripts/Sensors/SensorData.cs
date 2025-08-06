using UnityEngine;

[System.Serializable]
public struct SensorData
{
    public bool canSeePlayer;
    public bool canHearPlayer;
    public Vector3 lastKnownPosition;
    public float timeSinceLastSeen;
    public float timeSinceLastHeard;
}