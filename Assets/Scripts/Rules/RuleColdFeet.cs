using UnityEngine;

[CreateAssetMenu(fileName = "ColdFeet", menuName = "Rules/Restrictions/Cold Feet")]
public class ColdFeet : Rule
{
    [Header("Movement Tracking")]
    public float maxIdleTime = 2f;
    public float movementThreshold = 0.1f; // Minimum movement to count as "moving"
    
    private float timeSinceLastMovement = 0f;
    private Vector2 lastPlayerPosition;
    private bool hasInitializedPosition = false;
    
    public override void ActivateRule(PlayerController player)
    {
        // Initialize tracking when rule becomes active
        if (player != null)
        {
            lastPlayerPosition = player.transform.position;
            timeSinceLastMovement = 0f;
            hasInitializedPosition = true;
            Debug.Log($"Cold Feet restriction activated! You must move every {maxIdleTime} seconds or it's GAME OVER!");
        }
    }
    
    public override void DeactivateRule(PlayerController player)
    {
        // Reset tracking when rule is deactivated
        timeSinceLastMovement = 0f;
        hasInitializedPosition = false;
        Debug.Log("Cold Feet restriction deactivated.");
    }
    
    public override void UpdateRule(PlayerController player, float deltaTime)
    {
        if (player == null || !hasInitializedPosition) return;
        
        Vector2 currentPosition = player.transform.position;
        float distanceMoved = Vector2.Distance(currentPosition, lastPlayerPosition);
        
        // Check if player has moved significantly
        if (distanceMoved > movementThreshold)
        {
            // Player moved - reset timer
            timeSinceLastMovement = 0f;
            lastPlayerPosition = currentPosition;
        }
        else
        {
            // Player hasn't moved enough - increment idle time
            timeSinceLastMovement += deltaTime;
            
            // Check if player has been idle too long
            if (timeSinceLastMovement >= maxIdleTime)
            {
                Debug.Log("GAME OVER - Cold Feet! You stopped moving for too long!");
                TriggerGameOver(player);
            }
        }
    }
    
    private void TriggerGameOver(PlayerController player)
    {
        // Use GameStateManager to handle game over
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.GameOver("Cold Feet - You stopped moving for too long!");
        }
        else
        {
            Debug.LogError("GameStateManager not found! Make sure it's in the scene.");
        }
        
        // Deactivate this rule since game is over
        OnRuleRemoved();
    }
}