using UnityEngine;

[CreateAssetMenu(fileName = "MysticalSheild", menuName = "Rules/Health/Mystical Sheild")]
public class MysticalSheild : Rule
{
    [Header("Shield Settings")]
    public float shieldDuration = 5f;
    public bool blocksAllCollisions = true;
    
    public override void ActivateRule(PlayerController player)
    {
        player.SetInvincible(this.duration);
    }
    public override void DeactivateRule(PlayerController player)
    {
        player.RemoveInvincibility();
    }
    public override void UpdateRule(PlayerController player, float deltaTime)
    {
        //TBA: Sound particles etc...
    }
}
