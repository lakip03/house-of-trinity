using UnityEngine;

/// <summary>
/// Health rule granting temporary invincibility to the player.
/// </summary>
[CreateAssetMenu(fileName = "MysticalSheild", menuName = "Rules/Health/Mystical Sheild")]
public class MysticalSheild : Rule
{
    [Header("Shield Settings")]
    public float shieldDuration = 5f;
    public bool blocksAllCollisions = true;
    
    public override void ActivateRule(PlayerController player)
    {
        if (player == null)
        {
            Debug.LogWarning($"[{ruleName}] ActivateRule called with null player - rule will be activated when player is available");
            return;
        }
        
        player.SetInvincible(this.duration);
        Debug.Log($"[{ruleName}] Mystical Shield activated for {this.duration} seconds");
    }
    
    public override void DeactivateRule(PlayerController player)
    {
        if (player == null)
        {
            Debug.LogWarning($"[{ruleName}] DeactivateRule called with null player - skipping deactivation");
            return;
        }
        
        player.RemoveInvincibility();
        Debug.Log($"[{ruleName}] Mystical Shield deactivated");
    }
    
    public override void UpdateRule(PlayerController player, float deltaTime)
    {
        if (player == null) return;
    }
}