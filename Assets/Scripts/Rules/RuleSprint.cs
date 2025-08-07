using UnityEngine;

[CreateAssetMenu(fileName = "SprintRule", menuName = "Rules/Movement/Sprint Rule")]
public class SprintRule : Rule
{
    private float speedModifier = 2f;
    private float originalSpeed;

    public override void ActivateRule(PlayerController player)
    {
         if (player == null)
        {
            Debug.LogWarning($"[{ruleName}] ActivateRule called with null player - rule will be activated when player is available");
            return;
        }
        originalSpeed = player.moveSpeed;
        player.moveSpeed *= speedModifier;
        
    }
    public override void DeactivateRule(PlayerController player)
    {
         if (player == null)
        {
            Debug.LogWarning($"[{ruleName}] ActivateRule called with null player - rule will be activated when player is available");
            return;
        }
        player.moveSpeed = originalSpeed;
    }
    public override void UpdateRule(PlayerController player, float deltaTime)
    {
        if (player == null) return;
        //TBA: Sound particles etc...
    }
}
