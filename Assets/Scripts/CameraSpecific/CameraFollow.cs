using UnityEngine;

/// <summary>
/// A smooth camera following system that tracks a target player transform.
/// The camera smoothly interpolates to follow the player with a configurable offset and speed.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform player;
    
    public float followSpeed = 2f;
    
    public Vector3 offset = new Vector3(0, 0, -10);

    /// <summary>
    /// Updates the camera position to smoothly follow the target player.
    /// Uses LateUpdate to ensure the camera moves after all other objects have been updated in the current frame.
    /// </summary>
    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 targetPosition = player.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
    }
}