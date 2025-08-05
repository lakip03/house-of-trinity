using UnityEngine;

[CreateAssetMenu(fileName = "SprintRule", menuName = "Rules/Movement/Sprint Rule")]
public class SprintRule : Rule
{
    private float speedModifier = 2f;
    private float originalSpeed;

    public override void ActivateRule(PlayerController player)
    {
        originalSpeed = player.moveSpeed;
        player.moveSpeed *= speedModifier;
        
    }
    public override void DeactivateRule(PlayerController player)
    {
        player.moveSpeed = originalSpeed;
    }
    public override void UpdateRule(PlayerController player, float deltaTime)
    {
        //TBA: Sound particles etc...
    }
}
