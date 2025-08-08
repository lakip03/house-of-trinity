using UnityEngine;

/// <summary>
/// A smooth camera following system that tracks a target player transform.
/// The camera smoothly interpolates to follow the player with a configurable offset and speed.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    /// <summary>
    /// The player that the camera should follow.
    /// </summary>
    public Transform player;
    
    /// <summary>
    /// The speed at which the camera follows the target. Higher values result in faster, more responsive following.
    /// This value is multiplied by Time.deltaTime for frame-rate independent movement.
    /// </summary>
    public float followSpeed = 2f;
    
    /// <summary>
    /// The positional offset from the target that the camera should maintain.
    /// Default value of (0, 0, -10) keeps the camera 10 units behind the target on the Z-axis.
    /// </summary>
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