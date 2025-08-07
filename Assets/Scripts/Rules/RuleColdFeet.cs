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
        timeSinceLastMovement = 0f;
        hasInitializedPosition = false;
        Debug.Log("Cold Feet restriction deactivated.");
    }
    
    public override void UpdateRule(PlayerController player, float deltaTime)
    {
        if (player == null || !hasInitializedPosition) return;
        
        Vector2 currentPosition = player.transform.position;
        float distanceMoved = Vector2.Distance(currentPosition, lastPlayerPosition);
        
        if (distanceMoved > movementThreshold)
        {
            timeSinceLastMovement = 0f;
            lastPlayerPosition = currentPosition;
        }
        else
        {
            timeSinceLastMovement += deltaTime;
            
            if (timeSinceLastMovement >= maxIdleTime)
            {
                Debug.Log("GAME OVER - Cold Feet! You stopped moving for too long!");
                TriggerGameOver(player);
            }
        }
    }
    
    private void TriggerGameOver(PlayerController player)
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.GameOver("Cold Feet - You stopped moving for too long!");
        }
        else
        {
            Debug.LogError("GameStateManager not found! Make sure it's in the scene.");
        }
        
        OnRuleRemoved();
    }
}